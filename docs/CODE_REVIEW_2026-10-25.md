# Dapper.Extensions 代码库审查报告（二次复核版）

审查日期：2026-10-25（Asia/Shanghai）  
审查基线：`bc57189`；第二轮开始时仅有上一轮生成的未提交报告。  
前两轮均只审查和更新报告；随后按用户授权实施修复并补充测试。原始审查证据保留在第 1–8 节，修复结果见第 9 节。

## 1. 代码库做什么，以及总体判断

这是一个 Dapper 扩展库：通过统一的 `IDapper` 接口提供查询、分页和事务，并支持多个数据库驱动、内存/Redis 缓存、主从连接选择、XML SQL 管理和慢 SQL 监控。Microsoft DI、Autofac 和静态工厂是三种主要入口。

本次按“作为 NuGet 库供 Web 服务使用，存在并发请求、多个数据库连接和后台监控回调”的场景审查。没有假定每个应用都启用了所有可选功能。

**总体判断：现有结构可以继续使用，无需整体重写；应先修正缓存的数据隔离与事务一致性，再修正监控和注册分支。** 第二轮重新阅读源码、检查实际使用的 Dapper/FreeRedis 实现并运行验证后，原来 **16 项缺陷的现象均再次复现，没有撤销项**；新增 **3 项缺陷**，共 **19 项**。另将原来 4 项工程改进合并到第 20 项，避免和功能缺陷混计。

第二轮的关键变化：

- **新增第 17 项，优先级最高**：分页自动缓存遗漏模板中的筛选参数。即使只有一个数据库，租户/用户筛选条件不同的请求也可能拿到同一份结果。
- **新增第 18 项**：500 毫秒 Redis 缓存期限变成不带过期选项的 `SET`。
- **新增第 19 项**：不同连接注册的慢 SQL 监控配置使用同一全局服务，后注册的配置影响之前的连接。
- **修正第 2 项建议**：除了库内 `Transaction`，还必须处理调用方在 `CommandDefinition.Transaction` 中传入的事务。
- **扩大第 9 项范围**：前五个泛型 `CommandDefinition` 查询重载还遗漏了缓存锁等待时的取消令牌。
- **重新校准优先级**：第 4、5、6 项由 P1 调整为 P2；现象仍成立，但涉及特定查询组合或配置。P1 优先留给跨请求错误数据、事务污染及成功写入被误报失败。

原缺陷编号 1—16 保留，新增问题使用 17—19；编号不代表处理顺序。优先处理 **17 → 1 → 2 → 3**，其余按第 6 节执行。

最值得优先处理的风险：

- 分页筛选参数没有进入缓存键，单库中的不同租户或用户筛选也会互相串结果。
- 不同数据库连接可能共享同一条查询缓存，造成跨库、跨租户返回错误数据。
- 事务中的未提交数据可能进入共享缓存，回滚后仍继续被读取。
- 数据库写入已成功，但同步抛异常的监控处理器会使调用方收到失败，可能引发重复写入。
- 同一条 SQL 的列表查询、单条查询和不同结果类型使用相同缓存键，会出现类型转换异常。

## 2. 结构梳理

| 模块 | 主要位置 | 职责与调用关系 |
| --- | --- | --- |
| 公共调用入口 | `IDapper`、`BaseDapper.cs`、`BaseDapper.Async.cs` | 查询、执行、分页、事务和缓存入口；多数行为集中在这里 |
| 数据库适配 | MSSQL / MySql / PostgreSql / SQLite / Oracle / Odbc 项目 | 选择对应的 `DbConnection`；适配类很薄，共享核心逻辑 |
| 缓存 | 核心 `Caching/`、Memory / Redis 项目 | 生成缓存键、读写缓存、限制缓存未命中时的数据库并发 |
| 连接与主从 | `Connection/`、`MasterSlave/` | 获取连接配置、选择主库或按权重选择从库 |
| 容器与工厂 | DI / Autofac 扩展、`IResolveContext`、`Factory/` | 注册和解析 `IDapper`；工厂创建、释放调用作用域 |
| SQL 管理 | `SQL/SQLManager.cs` | 从文件加载 SQL，并在 XML 文件变化后热更新 |
| 监控与分析 | `Monitor/`、MiniProfiler 项目 | 代理数据库调用、记录慢查询并通知自定义处理器 |
| 其他工具 | `SqlExtensions`、日期转换、`Snowflake/` | 条件拼接 SQL、时间戳转换、雪花 ID |
| 示例与验证 | `Example/`、`ConsoleApp/`、`XUnitTest/` | 展示注册方式；现有自动测试主要使用 SQLite |
| 独立 VS 扩展 | `src/SQLSpliceExtension/` | SQL 拼接高亮工具，不在当前解决方案构建范围内 |

主调用链：

```text
DI / Autofac / DapperFactory
  → 数据库适配类（可由 DapperProxy 包装）
  → BaseDapper
  → 可选的缓存读取
  → Lazy 创建并打开连接
  → Dapper 执行 SQL / 使用当前事务
  → 可选的缓存写入
  → 返回结果 / 慢 SQL 通知
```

目前的主要问题发生在这些模块的交界处，而不是各数据库适配类本身。

## 3. 已执行的验证

| 验证 | 结果 | 证据边界 |
| --- | --- | --- |
| 从 `git archive HEAD` 导出的独立源码副本执行 `dotnet restore Dapper.Extensions.sln --ignore-failed-sources` | 成功；没有 `NU1801` / `NU1301` 源失败记录 | 副本没有原工作区的 bin/obj；仍使用本机 NuGet 包缓存，不是全新操作系统验证 |
| 在该副本执行 `dotnet test XUnitTest\XUnitTest.csproj --no-restore --verbosity quiet` | 15 通过，0 失败，0 跳过；net10.0 | 排除了必须依赖旧构建输出才能通过的疑点；SQL 测试文件确实已被 Git 跟踪 |
| 在该副本执行 `dotnet build Dapper.Extensions.sln --no-restore --verbosity quiet` | 0 错误，512 警告 | 第一轮增量构建为 496 警告；警告次数受重新编译范围影响，不代表 512 个独立缺陷 |
| 第一轮复现程序重新运行 | 原 17 个问题场景全部再次复现 | 对应原 16 项缺陷，源码基线相同；没有改动库实现 |
| 第二轮新增验证程序 | 9 项检查全部符合观察预期 | 其中 1 项是字典参数关闭缓存后的正常路径对照；其余覆盖新增或补充场景，不等同于 9 个新缺陷 |
| SQL 热更新并发读取 | 再次复现已存在 SQL 短暂缺失 | 本轮约 1.10 亿次紧循环读取出现 2,881 次缺失；仅证明竞态，不作为生产故障率或性能指标 |
| Redis 期限验证 | 500 毫秒生成无期限 `SET`；原生 TimeSpan 重载生成 `PX 500` | 使用 FreeRedis 的拦截器捕获命令并返回模拟响应；未在真实 Redis 上测量 TTL |
| 监控配置隔离验证 | 配置为 100,000 毫秒阈值的连接，在 20 毫秒受控调用后仍触发通知 | 使用实际 Autofac/代理注册，替身只负责提供确定的调用耗时 |

复现程序及构建日志放在仓库外的临时目录：

`C:\Users\ww_19\AppData\Local\Temp\dapper-audit-5885058ee01b4de9924105bdb69e31c2`

第二轮源码副本、检查程序及复核日志：

`C:\Users\ww_19\AppData\Local\Temp\dapper-review-v2-20261025`

这些临时目录可能被系统清理；本报告保留了关键输入和结果。

## 4. 必须修复：已复现的缺陷

### 1. [P1] 缓存没有隔离数据库连接

位置：[BaseDapper.cs:632](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:632)、[DefaultCacheKeyBuilder.cs:29](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:29)。异步入口有相同问题。

- **作用**：把查询结果保存到多个 `IDapper` 实例共享的缓存服务中。
- **问题与复现**：连接 A 的表中保存 `11`，连接 B 保存 `22`，分别执行 `select value from item;`。A 先缓存后，B 返回 `11`；禁用缓存时 B 正确返回 `22`。自动键只包含 SQL、参数和分页信息，没有连接或租户身份。
- **最小修复**：在共享缓存入口加入稳定的数据库/租户身份摘要；不要把包含口令的连接串直接写入键。为旧缓存切换命名空间，并同步检查自定义键及 `RemoveCache` 的兼容性。
- **不修的影响**：多库或多租户应用可能把其他数据库的数据返回给当前调用方；单库测试无法暴露它。

### 2. [P1] 事务读取使用共享缓存，未提交值还会污染缓存

位置：[BaseDapper.cs:627](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:627)、[BaseDapper.Async.cs:918](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:918)。

- **作用**：所有可缓存查询都经过 `CommandExecute` / `CommandExecuteAsync`。
- **问题与复现**：先缓存数据库值 `1`；事务内更新为 `2`，普通查询仍返回 `1`。强制更新缓存后回滚事务，数据库恢复 `1`，缓存查询仍返回 `2`。入口没有检查当前 `Transaction`。
- **最小修复**：存在活动事务时跳过共享缓存的读写，必须同时检查库内的 `Transaction` 和 `CommandDefinition.Transaction`；提交后按明确的显式失效约定处理已存在的业务缓存，并覆盖同步和异步路径。不能仅在 `IsEnableCache` 中查看库内属性便认为修复完成。
- **不修的影响**：事务内读不到自己的写入，其他请求还可能读到已回滚的数据。

**二次验证补充**：直接用 `d.Conn.Value.BeginTransaction()` 开启事务，通过 `CommandDefinition` 传入并将值从 `1` 更新为 `2`；事务内缓存查询后回滚，外部查询仍读到缓存值 `2`，关闭缓存时读到 `1`。此时库内 `Transaction` 属性未被设置。

### 3. [P1] 监控处理器可以把成功写入变成调用方看到的失败

位置：[CommandExecuteMonitor.cs:14](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Monitor/CommandExecuteMonitor.cs:14)、[CommandExecuteMonitor.cs:97](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Monitor/CommandExecuteMonitor.cs:97)。

- **作用**：在数据库调用结束后的 `finally` 中发送慢 SQL 通知。
- **问题与复现**：注册一个在 `OnSlowSqlCommandAsync` 返回 `Task` 前直接抛异常的合法接口实现。执行插入后，调用方收到监控异常，但直接查询数据库发现已经插入 1 行；监控异常没有隔离，还可能覆盖原始 SQL 异常。
- **最小修复**：在监控边界处理并记录处理器/日志自身的异常，保留数据库调用原本的返回值和异常；同时处理异步任务失败。
- **不修的影响**：调用方可能重试已经成功的写入，产生重复记录或重复业务操作。

### 4. [P2] 缓存键没有区分结果类型和查询语义

位置：[BaseDapper.cs:632](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:632)、[MemoryCacheProvider.cs:16](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Memory/MemoryCacheProvider.cs:16)。

- **作用**：不同查询重载共享同一套自动缓存键。
- **问题与复现**：先调用 `Query<int>("select 1;")`，再调用 `QueryFirstOrDefault<int>("select 1;")`，第二次命中保存了 `List<int>` 的键并抛 `InvalidCastException`。只加返回类型仍不足以区分 First 与 Single 的行数约束。
- **最小修复**：在缓存身份中同时区分结果类型、列表/单条/多结果集以及需要保留的查询语义；为有不同映射逻辑的查询明确自定义键规则。
- **不修的影响**：正常的查询调用顺序就可能触发异常；缓存命中还可能绕过本应发生的 Single 多行检查。

**二次验证补充**：对 `select 1 union all select 2;` 先调用 `QueryFirstOrDefault<int>` 再调用 `QuerySingleOrDefault<int>`，后者命中缓存返回 `1`；相同 Single 查询关闭缓存后正确抛出多行异常。因此修复时只补返回类型并不完整。

### 5. [P2] 时间参数精度在生成缓存键时丢失

位置：[DefaultCacheKeyBuilder.cs:51](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:51)、[DefaultCacheKeyBuilder.cs:61](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:61)。

- **作用**：把参数值转换成文本后拼接并哈希。
- **问题与复现**：同一 SQL 传入 `2026-10-25 12:00:00.001 UTC` 与 `.002 UTC`，生成完全相同的键。`DateTime` 落入默认字符串格式化分支，毫秒没有保留；问题发生在哈希前。
- **最小修复**：按参数类型使用稳定、无损且不依赖当前区域设置的编码；日期使用保留精度的往返格式或统一序列化。
- **不修的影响**：按高精度时间范围查询的数据会互相命中缓存，返回错误结果。

### 6. [P2] Autofac 的部分监控注册分支丢失服务键

位置：[AutofacExtensions.cs:55](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/AutofacExtensions.cs:55)、[AutofacExtensions.cs:125](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/AutofacExtensions.cs:125)。

- **作用**：按服务键和主从标记注册可监控的 `IDapper` 代理。
- **问题与复现**：① 指定 `serviceKey="named"`、不启用主从且启用监控后，`ResolveKeyed<IDapper>("named")` 抛 `ComponentNotRegisteredException`；② 无服务键、启用主从和监控后，`ResolveDapper(true)` 同样失败。两个代理注册分别漏掉对应的 `.Keyed<IDapper>(...)`。
- **最小修复**：补齐这两处代理的服务键，并用一个小型组合测试覆盖“有/无服务键 × 主从开/关 × 监控开/关”。
- **不修的影响**：业务代码只增加监控配置，就可能使原本能解析的数据库服务失效。

### 7. [P2] SQL 热更新先清空在线字典，再逐项填充

位置：[SQLManager.cs:43](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/SQL/SQLManager.cs:43)。

- **作用**：XML 文件变化时重新加载命名 SQL。
- **问题与复现**：持续读取始终存在的 `anchor` SQL，同时修改包含 20,001 条 SQL 的文件；两轮分别出现 2,935 和 2,881 次 `KeyNotFoundException`，且随后读到了更新值。`ConcurrentDictionary` 只能保护单个操作，不能让“清空再重建”成为一次完整切换。
- **最小修复**：完整构造并校验新快照后，一次替换当前字典引用；加载失败继续保留旧快照。
- **不修的影响**：发布或编辑 SQL 文件时，正在处理的请求会偶发找不到有效 SQL。

### 8. [P2] 命名 SQL 的同步动态分页重载传错参数

位置：[BaseDapper.cs:533](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:533)。

- **作用**：`QueryPlainPage(SQLName, ...)` 获取 SQL 后转调字符串重载。
- **问题与复现**：SQL 为 `select @id as id limit @Take offset @Skip;`，传入 `new { id = 7 }`。直接 SQL 调用成功，命名 SQL 调用报参数不足；转发时把第四个参数 `param` 错写成了 `pageSize`。
- **最小修复**：将该实参改为 `param`，补一个带业务筛选参数的命名 SQL 分页测试。
- **不修的影响**：这一重载丢失调用方筛选参数，带参数的查询失败。

### 9. [P2] 异步查询的取消令牌在执行或缓存等待路径中丢失

位置：[BaseDapper.Async.cs:65](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:65)、[BaseDapper.Async.cs:918](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:918)。

- **作用**：公共异步查询接受 `CancellationToken`。
- **问题与复现**：预先取消令牌，再执行 `QueryAsync<int>("select 99;", enableCache:false, cancellationToken:token)`，仍正常返回 `99`。令牌只用于等待缓存信号量，没有传给实际调用 Dapper 的命令；关闭缓存后连这次检查也没有。
- **最小修复**：字符串入口使用携带当前事务、超时、命令类型和取消令牌的 `CommandDefinition` 执行；所有接收 `CommandDefinition` 的入口还须把 `command.CancellationToken` 传给共享缓存等待逻辑。同步检查命名 SQL 转发路径。
- **不修的影响**：请求取消或超时后，数据库工作仍可能继续占用连接和资源。

**二次验证补充**：[BaseDapper.Async.cs:15](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:15) 开始的五个泛型命令重载没有给 `CommandExecuteAsync` 传入令牌。占住缓存信号量后，调用携带已取消令牌的 `QueryAsync<int>(CommandDefinition)`，120 毫秒后仍在等待；其他已经正确转发令牌的命令重载不应一并判为此问题。

### 10. [P2] 开启缓存后，字典参数触发反射异常

位置：[DefaultCacheKeyBuilder.cs:58](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:58)、[DefaultCacheKeyBuilder.cs:81](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:81)。

- **作用**：提取普通参数对象的属性，用于生成缓存键。
- **问题与复现**：传入 `new Dictionary<string, object> { ["id"] = 3 }` 查询 `select @id;`，缓存键生成阶段抛 `TargetParameterCountException`。代码反射到字典索引器 `Item`，却不提供索引值，也没有按参数键值对处理字典。
- **最小修复**：显式处理 Dapper 支持的字典参数，并过滤索引器属性；不要只通过过滤索引器而把所有字典参数都编码成相同键。
- **不修的影响**：本来有效的 Dapper 参数形式因为开启缓存而不可用。

### 11. [P2] 异步监控任务未完成，依赖作用域已经释放

位置：[CommandExecuteMonitor.cs:87](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Monitor/CommandExecuteMonitor.cs:87)、[CommandExecuteMonitor.cs:97](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Monitor/CommandExecuteMonitor.cs:97)。

- **作用**：为自定义慢 SQL 处理器创建独立 DI 作用域。
- **问题与复现**：处理器实现 `IDisposable`，在回调中 `await Task.Delay(50)` 后检查状态，确认续体执行前自己已被释放。`ConfigureAwait(false)` 不会等待任务，`using` 在通知方法返回时立即结束。
- **最小修复**：让作用域覆盖任务完整生命周期；异步调用等待处理器，若保留后台处理则必须由实际拥有任务生命周期的机制管理作用域并观察异常。
- **不修的影响**：处理器异步使用数据库等作用域依赖时可能访问已释放对象，监控任务失败且异常无人观察。

### 12. [P2] 释放缓存键构造器会留下已释放的线程静态 MD5

位置：[DefaultCacheKeyBuilder.cs:22](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:22)、[DefaultCacheKeyBuilder.cs:92](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:92)、[DefaultCacheKeyBuilder.cs:114](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:114)。

- **作用**：复用线程上的 MD5 实例，避免每次创建。
- **问题与复现**：同一线程上，构造器 A 生成一次键并 `Dispose`；新建构造器 B 再生成键，抛 `ObjectDisposedException`。字段是静态的，而释放属于实例，且释放后没有清空静态引用。
- **最小修复**：取消这项共享可变对象优化，使用单次调用拥有并释放的哈希实例，兼顾当前声明支持的旧目标框架。
- **不修的影响**：多个宿主/容器、重复初始化或测试宿主销毁后，同一线程后续缓存查询可能失败。

### 13. [P2] Redis 泛型注册覆盖调用方的自定义缓存键实现

位置：[DependencyInjectionExtensions.cs:50](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Redis/DependencyInjectionExtensions.cs:50)。

- **作用**：允许 `AddDapperCachingInRedis<TCacheKeyBuilder>` 使用业务自定义键策略。
- **问题与复现**：注册 `CustomKeyBuilder` 后，从 Microsoft DI 解析 `ICacheKeyBuilder`，实际得到 `DefaultCacheKeyBuilder`。泛型重载先注册自定义类型，再调用会追加默认注册的普通重载，单服务解析取到了后者。
- **最小修复**：让默认注册只在缺少实现时添加，或在完成公共注册后覆盖自定义实现；保留现有泛型 API。
- **不修的影响**：业务用于隔离租户、兼容旧键等的自定义策略完全不生效。

### 14. [P2] Autofac 内存缓存忽略自定义 KeyPrefix

位置：[AutofacExtensions.cs:15](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Memory/AutofacExtensions.cs:15)、[AutofacExtensions.cs:38](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Memory/AutofacExtensions.cs:38)。

- **作用**：把 `MemoryConfiguration` 转换为共享缓存配置。
- **问题与复现**：设置 `KeyPrefix="custom"`，解析出的 `CacheConfiguration.KeyPrefix` 仍是 `dapper_cache`。两个 Autofac 重载都漏抄该属性；Microsoft DI 版本已正确传递。
- **最小修复**：补齐两个重载的 `KeyPrefix` 映射，并验证写入、读取和移除使用同一前缀。
- **不修的影响**：指定命名空间失效，与 DI 入口行为不一致，可能干扰共享缓存中的其他数据。

### 15. [P2] 超出末页时只修正页码，没有重新查询对应数据

位置：[BaseDapper.cs:398](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:398)、[BaseDapper.cs:486](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:486)、[BaseDapper.Async.cs:730](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:730)、[BaseDapper.Async.cs:824](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:824)。

- **作用**：返回含总数和当前页码的 `PageResult`。
- **问题与复现**：表中只有 1 条记录，请求第 5 页、每页 1 条，结果返回 `Page=1, TotalCount=1, Result=[]`；数据实际按第 5 页偏移量查询，但只把返回页码改成了末页。空数据时还可能返回 `Page=0`。
- **最小修复**：最小方案是保留请求页码并允许返回空列表；若产品约定自动回到末页，则必须按实际页码重新查询。同步、异步和泛型/动态版本统一契约。
- **不修的影响**：前端拿到的页码与数据不一致，分页组件可能错误显示“第一页无数据”。

### 16. [P2] SQL 目录里的非 XML 文件也被当成 SQL XML 解析

位置：[SQLManager.cs:64](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/SQL/SQLManager.cs:64)。

- **作用**：递归扫描 SQL 配置目录。
- **问题与复现**：只放有效 `query.xml` 时初始化成功；增加 `README.txt` 后初始化抛 `InvalidOperationException`。加载器解析所有普通文件，而监听器只监听 `**/*.xml`，规则也不一致。
- **最小修复**：按扩展名筛选 XML 文件，保持初次加载、递归加载和监听规则一致。
- **不修的影响**：说明文档、编辑器备份等非 XML 文件可能导致启动失败，或使后续 XML 热更新失败。

### 17. [P1，新发现] DynamicParameters 模板没有进入缓存键，分页筛选条件被忽略

位置：[DefaultCacheKeyBuilder.cs:46](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Caching/DefaultCacheKeyBuilder.cs:46)、[BaseDapper.cs:423](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:423)、[BaseDapper.Async.cs:697](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:697)。

- **作用**：自动缓存遍历 `DynamicParameters.ParameterNames` 取参数；分页方法会先把调用方参数包装成 `DynamicParameters`。
- **问题与复现**：`new DynamicParameters(new { tenant = 11 })` 在绑定命令前 `ParameterNames` 数量为 0，值 `22` 的另一个参数对象也一样；两者生成同一个键，后者缓存查询返回 `11`，关闭缓存后返回 `22`。分页中传普通匿名对象也会触发：`tenant=31` 和 `tenant=32` 的第一页都返回 `31`；异步 `QueryPageAsync` 同样复现。
- **最小修复**：分页用原始业务参数加页码/页大小生成键，执行时再使用包装后的参数；同时消除键生成器在 `param == null` 时提前返回、跳过分页信息的分支。对于调用方传入、无法安全提取完整输入的 `DynamicParameters`，不能继续把未展开模板当成“没有参数”；应明确要求显式键或安全地跳过自动缓存，避免依赖 Dapper 私有字段。
- **不修的影响**：这是单库中的筛选条件串缓存，影响不限于多连接。SQL 中即使正确写了 `WHERE TenantId=@TenantId`，仍可能向一个租户返回另一个租户的已缓存结果。

关键调用示例：

```csharp
const string sql = "select @tenant limit @Take offset @Skip;";
dapper.QueryPlainPage<int>(sql, 1, 1, new { tenant = 31 }); // 31
dapper.QueryPlainPage<int>(sql, 1, 1, new { tenant = 32 }); // 错误：31
dapper.QueryPlainPage<int>(sql, 1, 1, new { tenant = 32 }, enableCache: false); // 32
```

本轮检查了实际依赖 Dapper 2.1.79 的实现：匿名对象先保存在内部模板列表中，`ParameterNames` 只枚举已建立的参数字典。因此排序参数名称、改哈希算法、只补连接名，都不能单独解决此问题。

### 18. [P2，新发现] Redis 的亚秒缓存期限被截断成无过期选项

位置：[RedisCacheProvider.cs:20](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Redis/RedisCacheProvider.cs:20)。

- **作用**：把可选的 `TimeSpan` 转交给 FreeRedis 保存查询结果。
- **问题与复现**：`TimeSpan.FromMilliseconds(500)` 经 `(int)expired.Value.TotalSeconds` 变成 `0`。通过实际 FreeRedis 1.5.5 拦截器观察到库生成 `SET half-second {"Value":123}`；2 秒对照生成 `EX 2`，直接调用 FreeRedis 的 TimeSpan 重载则生成 `PX 500`。
- **最小修复**：有期限时使用依赖已经提供的 TimeSpan 重载，校验零、负值和小于支持精度的期限；无期限时明确走永久缓存分支，保持与内存缓存的契约一致。
- **不修的影响**：业务想短暂保存的数据被写成不带到期时间的缓存，更新可能长期不可见。本轮确认的是命令生成错误，没有声称执行过真实 Redis TTL 测量。

### 19. [P2，新发现] 不同连接的监控配置互相覆盖

位置：[AutofacExtensions.cs:27](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/AutofacExtensions.cs:27)、[DapperProxy.cs:23](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Monitor/DapperProxy.cs:23)、[CommandExecuteMonitor.cs:96](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Monitor/CommandExecuteMonitor.cs:96)。

- **作用**：每次 `AddDapper(..., monitorBuilder)` 都接受调用方给当前连接配置的阈值、日志和处理器。
- **问题与复现**：A 注册阈值 100,000 毫秒，B 随后注册阈值 0；A 的 20 毫秒受控调用仍触发慢 SQL 通知。各注册都添加同一个未命名的 `MonitorConfiguration`，所有代理再按同一服务类型解析，实际使用了后注册的配置；处理器解析同样未按连接区分。
- **最小修复**：由注册工厂向每个代理传入该次注册的配置，并让处理器选择和作用域归属与对应连接一致。保留现有公共构造方式的兼容性，不把修复只限定为单个配置字段。
- **不修的影响**：增加一个数据库连接即可改变已有连接的监控行为，造成误报、漏报，或使用非预期的处理器。

## 5. 应该改进

### 20. 工程质量与维护改进（原报告第 17—20 项合并）

#### 测试偏重正常路径，事务回滚用例存在假通过

位置：[ExecuteTest.cs:81](E:/GitHub/Dapper.Extensions/XUnitTest/ExecuteTest.cs:81)、[QueryTest.cs:25](E:/GitHub/Dapper.Extensions/XUnitTest/QueryTest.cs:25)、[TestStartupFixture.cs:25](E:/GitHub/Dapper.Extensions/XUnitTest/TestStartupFixture.cs:25)。

- **作用**：现有 SQLite 测试验证基本查询、执行、事务和缓存重复读取。
- **问题**：回滚用例把字段更新成上个事务已经提交的同一个 `newName`，即使回滚没有效果也可能通过。缺少不同连接共用缓存、缓存与事务混用、Autofac 监控组合、取消和热更新并发测试。
- **最小改进**：回滚时改成另一个值，并断言事务内值确实改变、回滚后恢复；按上述已复现缺陷补小型回归测试。将现有 `async void` 测试改为 `async Task`，当前分析器也已给出提醒。
- **不改的影响**：测试全绿仍会遗漏数据正确性与注册路径缺陷，修复后也容易回归。

#### 默认缓存并发限制把所有不同查询也放进同一队列

位置：[BaseDapper.cs:645](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.cs:645)、[BaseDapper.Async.cs:939](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/BaseDapper.Async.cs:939)、[DependencyInjectionExtensions.cs:11](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Memory/DependencyInjectionExtensions.cs:11)。

- **作用**：默认 `maxConcurrent=1`，用单例信号量限制缓存未命中后的数据库访问。
- **问题**：按当前实现，某条慢查询占用名额时，不同连接、不同 SQL 的未命中请求也要等待；默认 5 秒未取得名额就抛 `DapperCacheException`。这是代码路径分析，没有做生产并发压测。
- **最小改进**：先说明这是全局数据库并发上限，并根据预期负载调整默认值/显式配置。只有确实需要“同一个键只回源一次”时，再引入按键合并机制，避免直接扩大实现。
- **不改的影响**：缓存刚启动或集中失效时，数据库本可并行处理的查询被串行化，长查询拖累无关请求。

#### 依赖条件和构建警告需要整理

位置：[Oracle 项目文件:31](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Oracle/Dapper.Extensions.Oracle.csproj:31)、[核心项目文件:38](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Dapper.Extensions.csproj:38)、[Example 项目文件:46](E:/GitHub/Dapper.Extensions/Example/Example.csproj:46)。

- **作用**：多个目标框架和数据库包通过项目文件选择依赖。
- **问题**：本轮干净源码副本构建有 512 条警告。Oracle 在 net5.0/net7.0 同时声明新旧两个版本的 `Oracle.ManagedDataAccess.Core`，触发 `NU1504`；部分依赖对 net5/6/7 给出不支持目标框架警告；Example 的两个传递包 `NU1901` 低严重性告警在本轮重新还原时仍然出现。
- **最小改进**：先让 Oracle 版本条件互斥；明确真正承诺运行支持的框架，并为这些框架做干净还原和最小运行测试。单独核查 Example 的依赖来源与告警修复，不盲目统一升级版本。
- **不改的影响**：依赖版本选择和兼容性边界不清楚，真实编译或运行问题容易淹没在大量警告中。这里没有把 Example 的告警推断为核心 NuGet 包存在同样漏洞。

#### 删除无引用的旧工程和未参与构建的残留代码

位置：[Redis 项目文件:20](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Redis/Dapper.Extensions.Caching.Redis.csproj:20)、[PartitionRedisCacheProvider.cs](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions.Caching.Redis/PartitionRedisCacheProvider.cs)、[DapperFactory.cs:52](E:/GitHub/Dapper.Extensions/src/Dapper.Extensions/Factory/DapperFactory.cs:52)。

- **作用**：仓库保留了旧工程备份、旧分区缓存提供器和工厂创建的额外 ServiceProvider。
- **问题**：5 个 `Dapper - Backup*.csproj` 共 161 行未被解决方案或构建脚本引用；33 行的 `PartitionRedisCacheProvider` 被显式排除编译，实际分区注册使用 `RedisCacheProvider`。工厂还创建了一个全树只写不读、未释放的额外 `ServiceProvider`。
- **最小改进**：删除已确认无引用的备份文件和排除编译的旧实现，同时移除对应排除项；删除工厂中不用的 provider 创建和存储属性。保留现有数据库适配项目和公共接口，不进行结构重写。
- **不改的影响**：维护者容易修改不参与发布的代码，工厂也保留了没有用途的容器对象。

可精简规模：仅 5 个备份工程和 1 个旧实现就有 **194 行**；未提出删除任何运行时依赖。

## 6. 建议的修复次序与验收

| 顺序 | 对应编号 | 验收标准 |
| --- | --- | --- |
| 第一批：结果正确性 | 17、1、2、4、5、10、12 | 不同筛选条件/连接/类型/时间参数不碰撞；内部与外部传入的事务均不读写共享缓存；字典参数有效；容器重建后缓存键仍可生成 |
| 第二批：监控不破坏业务 | 3、6、11、19 | 启用监控后所有注册组合可解析；不同连接配置独立；处理器生命周期完整；处理器故障不改变 SQL 本身的结果 |
| 第三批：查询与热更新边界 | 7、8、9、15、16 | 已有 SQL 热更新期间持续可读；分页参数和元数据一致；取消令牌进入实际数据库命令；忽略非 XML 文件 |
| 第四批：缓存配置一致性 | 13、14、18 | 自定义 Redis 键构造器生效；Autofac 与 Microsoft DI 的缓存前缀一致；亚秒期限正确传递 |
| 随修复落实 | 20：测试 | 每项修复有对应的小型回归用例；事务回滚测试能识别回滚失效 |
| 发布前整理 | 20：并发、依赖、精简 | 明确缓存并发策略；验证承诺的目标框架；消除重复依赖并删除无用文件 |

缓存键格式调整会影响现有键和业务手动失效逻辑，需要作为明确的兼容性变化说明。不要为了修复缓存而自动猜测 SQL 会更新哪些表；先采用清晰的事务缓存边界和显式失效约定。

## 7. 复现结果摘要

原复现集在第二轮的重新运行结果：

```text
cross_connection_cache: A=11, B_cached=11, B_actual=22
result_shape_cache: InvalidCastException
transaction_cache: read_after_write=1, rollback_cached=2, rollback_actual=1
cancel_token_ignored: already-cancelled query completed
datetime_key_collision: distinct millisecond values share key=True
dictionary_cache_key: TargetParameterCountException
page_metadata: requested=5, response_page=1, rows=0, total=1
sqlname_paging_param: direct succeeds, SQLName throws SQLiteException
monitor_scope_early_disposal: handler disposed before async continuation=True
keyed_monitored: ComponentNotRegisteredException
default_slave_monitored: ComponentNotRegisteredException
autofac_memory_prefix: requested=custom, actual=dapper_cache
sql_non_xml: InvalidOperationException
monitor_write_reports_failure: caller got monitor exception but committed_rows=1
redis_custom_key_registration: requested=CustomKeyBuilder, actual=DefaultCacheKeyBuilder
sql_reload_race: reads=109749734, missing_known_key=2881, update_seen=1
disposed_threadstatic_hash: ObjectDisposedException
Confirmed defect scenarios: 17
```

第二轮新增验证（含 1 项正常路径对照）：

```text
dynamic_parameter_templates: names_before_binding=0, key_equal=True,
  first=11, second=11, uncached=22
plain_page_filter_cache: filter31=31, filter32_cached=31, filter32_actual=32
async_page_filter_cache: filter41=41, filter42_cached=41
single_semantics_bypassed: cached_single=1, uncached_single_throws=True
dictionary_uncached_control: uncached_value=9
redis_ttl_truncated:
  half=[SET half-second {"Value":123}]
  two=[SET two-seconds {"Value":123} EX 2]
  native=[SET native-half-second control PX 500]
command_wait_ignores_cancellation: still_waiting_after_120ms=True
external_transaction_cache: cached_after_rollback=2, actual=1
monitor_configuration_crosstalk:
  A_threshold=100000ms, A_work=20ms, callback_calls=1
Checks=9, not_reproduced=0
```

二次复核明确排除或保留为边界的问题：

- SQL 测试 XML 文件已经被 Git 跟踪，干净源码副本的测试也通过；不把它误报为“依赖本机未提交文件”。
- 全局缓存信号量是可配置的限流策略，第 20 项只列出吞吐上的代价，不将它描述成必然的并发 bug。
- `CommandDefinition` 不自动继承库内事务的行为，涉及显式命令的事务契约和驱动差异；没有真实驱动证据时，不把它直接列成“写入必然逃出事务”。已确认的是第 2 项所述的事务查询缓存污染。
- 本轮 NuGet 还原和全解决方案构建成功，不能根据包的主版本号直接断言目标框架无法构建；旧框架运行兼容性仍需单独测试。

## 8. 没有验证的范围

- 未连接真实 SQL Server、MySQL、PostgreSQL、Oracle、ODBC 或 Redis 服务；SQLite 复现说明共享逻辑存在问题，不代替这些驱动的集成测试。
- 未验证真实主从复制延迟、故障转移、跨进程缓存竞争或生产吞吐；主从部分主要检查配置、注册和选择流程。
- 已执行独立源码副本的依赖还原；未验证完全没有 NuGet 缓存的机器，未做 NuGet 打包发布或旧 .NET 运行时启动测试。漏洞方面只记录还原工具输出，没有独立核查漏洞公告及实际可利用路径。
- `SQLSpliceExtension` 仅做静态浏览，未启动 Visual Studio 验证高亮行为；MiniProfiler 和雪花 ID 未做专门运行测试。
- 未对所有公共重载逐一做等价性验证；本报告按已复现结果列出缺陷，没有把未验证的推断写成已确认故障。

**建议：先修分页筛选参数串缓存、跨连接缓存、事务缓存和监控异常边界。19 项缺陷按触发条件分别成立；不表示所有未启用这些可选功能的调用都会出错。**

## 9. 授权后的修复与验证结果

2026-10-25，按“保留现有接口不变，进行修复并完善测试覆盖”的要求完成修复。第 1–8 节是修复前的审查快照，其中源码行号可能因修复发生偏移。

### 9.1 修复对应关系

| 原问题 | 修复 | 回归测试位置 |
| --- | --- | --- |
| 1 跨连接串缓存 | 缓存命名空间包含连接名、驱动、主从角色和实际解析的连接字符串摘要；连接字符串只解析一次并用于实际建连 | `CacheRegressionTests.ConnectionsDoNotShareCachedRows` |
| 2 事务污染缓存 | 库内事务和 `CommandDefinition.Transaction` 都绕过缓存读写 | `TransactionsNeitherReadNorPopulateSharedCache`、`ExternalCommandTransactionDoesNotPolluteCache` |
| 3 监控异常覆盖业务结果 | 隔离回调、日志、序列化、服务解析和作用域释放异常 | `MonitorRegressionTests.HandlerFailureCannotMaskSqlOutcome`、`LoggingSerializationAndResolutionFailuresCannotMaskWrites` |
| 4 返回类型与查询语义冲突 | 自动键区分结果类型、查询方式和命令类型；显式键命中时同样核对语义，映射查询与普通查询区分 | `CachedResultPreservesReturnTypeAndSingleSemantics`、`MappingFunctionsRequireExplicitCacheIdentity` |
| 5 日期精度丢失 | 参数按名称排序，保留 CLR 类型，以不依赖区域设置的 JSON 和往返日期格式编码 | `KeysRetainPrecisionTypesOrderingAndPagination` |
| 6 Autofac 注册遗漏 | 补齐带监控的默认从库、具名非主从库的 keyed 注册 | `RegistrationAndRedisRegressionTests.AutofacCanResolveAllRegistrationVariants`，覆盖 8 种组合及读写角色传递 |
| 7 SQL 热加载中间空窗 | 构造完整新快照后原子替换，失败时保留旧快照 | `SqlReloadRegressionTests.ReloadPublishesCompleteSnapshotAndRetainsOldSnapshotOnInvalidXml` |
| 8 SQLName 动态分页传错参数 | 转发原始 `param` | `SqlNameDynamicPagingForwardsBusinessParameters` |
| 9 取消令牌遗漏 | 字符串异步查询使用携带令牌的 `CommandDefinition`，所有命令查询的缓存等待也携带令牌 | `CancellationRegressionTests`：5 种泛型命令入口、12 种查询形态的运行中取消 |
| 10 字典参数索引器异常 | 直接枚举键值，普通对象反射排除索引器 | `DictionariesAndOpaqueDynamicTemplatesReturnCorrectRows` |
| 11 异步监控作用域过早释放 | 等待回调完成再释放作用域；同步入口等待监控任务 | `HandlerScopeLivesUntilAsyncWorkCompletes` |
| 12 共享 MD5 被释放 | 每次哈希使用独立 SHA-256 实例，保留 `Dispose()` 接口 | `BuilderDisposalDoesNotPoisonOtherInstancesOrThreads` |
| 13 Redis 自定义键生成器被覆盖 | 先注册默认服务，再注册调用者指定的生成器 | `RedisUsesRegisteredCustomKeyBuilder` |
| 14 Memory KeyPrefix 遗漏 | 两种 Autofac 注册重载均复制前缀 | `AutofacMemoryPreservesPrefix` |
| 15 分页页码与结果不符 | 四种分页实现均保留调用者请求页码 | `PagingMetadataKeepsRequestedPage`，覆盖零记录和超出总页数 |
| 16 非 XML 文件导致加载失败 | 递归读取时仅解析 XML 文件，增加监视器资源释放 | 两项 `SqlReloadRegressionTests` |
| 17 模板参数导致分页串缓存 | 分页使用原始业务参数和页坐标生成键；不透明动态参数默认跳过自动缓存 | `PagingCacheIncludesOriginalBusinessParameters`，覆盖同步／异步、泛型／动态结果及普通分页／带总数分页 |
| 18 Redis 毫秒期限变永久 | 使用 FreeRedis 的 `TimeSpan` 重载；拒绝不足 1 毫秒的非空期限 | `RedisPreservesExpiration`、`RedisRejectsUnsupportedExpirationBeforeSending` |
| 19 监控配置互相覆盖 | 每个注册捕获独立配置与处理器类型 | `AutofacRegistrationsKeepIndependentThresholdsAndHandlers`、`MicrosoftDiRegistrationsKeepIndependentMonitoring` |

补充验证：`ExplicitKeysSupportHitsForceRefreshAndRemoval` 同时通过内存对象和 JSON 序列化缓存路径验证命中、强制刷新、跨同步／异步读取与 `RemoveCache(...)`。

工程改进：原有 `async void` 测试已改为 `async Task`；回滚测试改成写入不同值，避免“回滚前后相同”的假通过；Oracle 包引用条件改为互斥；删除工厂中未使用的第二个服务容器构建。没有删除既有公开类型、缩减目标框架或批量升级依赖。

### 9.2 接口兼容性

对修复前与修复后的 net10.0 程序集进行反射对比，检查公开／受保护类型与成员、参数名称、默认值、泛型约束和成员修饰：

| 程序集 | 基线契约条目 | 删除或签名变化 |
| --- | ---: | ---: |
| Dapper.Extensions | 638 | 0 |
| Dapper.Extensions.Caching.Memory | 14 | 0 |
| Dapper.Extensions.Caching.Redis | 32 | 0 |

核心程序集唯一新增的可见契约是 `SQLManager` 实现 `IDisposable`，用于容器清理文件监视器。`IDapper`、`ICacheProvider`、`ICacheKeyBuilder`、`DapperProxy` 的公开构造器，以及 `BaseDapper` 的受保护缓存方法签名保留。

### 9.3 测试与覆盖率

- 修复前干净源码副本：15 个测试通过。
- 修复后：**81 个测试通过，0 失败，0 跳过**；新增 66 个用例。
- 完整解决方案及其声明的目标框架构建通过，0 错误；不等同于所有旧运行时或所有数据库驱动均完成运行验证。
- `git diff --check` 通过。

使用项目已有的 Coverlet 收集器实测，没有增加测试框架或包依赖；测试项目增加了已有 Redis 项目的项目引用。

| 范围 | 修复前行覆盖率 | 修复后行覆盖率 | 修复前分支覆盖率 | 修复后分支覆盖率 |
| --- | ---: | ---: | ---: | ---: |
| 核心 Dapper.Extensions | 38.08% | 50.19% | 38.50% | 58.13% |
| Memory 缓存 | 34.69% | 77.00% | 28.57% | 64.28% |
| Redis 缓存 | 未纳入原测试 | 23.85% | 未纳入原测试 | 22.72% |
| 本次收集的所有程序集 | 37.10% | 47.87% | 37.26% | 53.90% |

总覆盖率的分母包含尚未连接真实服务的驱动入口；新旧总分母也因 Redis 项目加入和源码变化而不同，应优先比较核心／Memory 的分项数据。测试集中覆盖本报告确认的问题，没有宣称所有公共重载或平台路径均已覆盖。

复验命令：

```powershell
dotnet build Dapper.Extensions.sln
dotnet test XUnitTest/XUnitTest.csproj --no-restore --collect:"XPlat Code Coverage"
git diff --check
```

### 9.4 升级时的行为变化与剩余边界

- **缓存使用新命名空间和内部值格式，旧缓存不会被读取。** 有期限的旧键等待自然过期；旧永久键需要由缓存管理流程清理。不要在生产环境无差别清空整个 Redis。直接操作库内部缓存键和值格式的外部代码需同步调整。
- 默认生成器遇到不透明 `DynamicParameters` 时不自动缓存；显式 `cacheKey` 或能完整理解参数的自定义生成器仍可使用。直接调用默认 `Generate(...)` 而不给动态参数提供显式键，会得到明确的 `NotSupportedException`。
- 多映射查询无法可靠推断委托闭包及拆分规则，自动缓存因此要求显式键。不同映射、捕获值及拆分规则应使用不同键。普通分页仍保留自动缓存。
- 显式键与失效操作按解析后的连接隔离；应在同一连接配置下调用 `RemoveCache(...)`。写入后缓存失效仍由应用原有策略负责，未新增自动清除或分布式失效协议。
- 慢 SQL 监控会等待处理器完成；同步调用同样会等待，因此处理器耗时会体现在调用延迟中。处理器应自行设置合理的耗时上限。
- Redis `null` 期限表示永久，非空期限必须至少 1 毫秒。分页超界时返回请求页码及实际空结果，不再悄悄改成最后一页。
- 尚未验证真实 Redis、SQL Server、MySQL、PostgreSQL、Oracle、ODBC 服务、真实复制延迟及旧 .NET 运行时。Redis 期限测试通过拦截器检查实际生成的命令，不连接服务器。
- 旧框架支持警告、已有驱动弃用提示和示例项目的 NuGet 安全警告仍需单独处理；全局缓存并发限制维持原配置。未清理历史备份项目文件，也未改动 MiniProfiler、雪花 ID 和编辑器插件。

## 10. Autofac 渐进弃用与微软 DI 多实例注册

2026-10-25，按后续授权增加微软 DI 注册能力，同时将 Autofac 专用入口标为弃用。旧功能继续保留，本阶段不移除 Autofac 包依赖，也不改动 `IDapper` 查询签名。

### 10.1 实现范围

- 核心 `IServiceCollection.AddDapper<TDbProvider>` 增加 `connectionName`、`serviceKey`、`enableMasterSlave`、`monitorBuilder` 重载；六种驱动均增加相应注册重载。新重载的前两个参数必填，避免破坏原来无参、`null` 和监控回调的重载选择。
- 使用微软 DI 的 keyed scoped 服务注册具名实例。主从角色在注册工厂中显式传入构造器，普通 `bool` 不作为全局服务注册。同一名称、角色在一个作用域内复用，跨作用域创建不同实例。
- 新增不包含 Autofac 类型的 `IDapperResolver`，提供默认／具名连接与主从选择。未知名称或未开启主从时请求从库会抛异常，不自动降级到其他连接。
- 具名主库使用原生字符串键，支持 `[FromKeyedServices("orders")]`；从库使用复合键，避免和 `orders_slave`、`_slave` 等合法名称冲突。业务层建议统一使用解析接口选择角色。
- 每个注册捕获独立监控配置。启用监控时，底层提供者使用独立内部键注册到容器中，代理不负责释放底层对象；由容器准确释放一次。默认连接字符串提供者和负载均衡服务使用 `TryAdd`，不会覆盖调用者已注册的实现。
- 所有 Autofac 注册扩展、`IResolveContext` / `ResolveContext` / `ResolveKeyed`、旧 `DapperFactory` / `ExtensionBuilder`、`MonitorBuilder` 的 Autofac 构造器和受保护属性均增加 `Obsolete(..., false)`。共享类与微软 DI 入口不整体弃用。
- README 增加注册、配置、解析和迁移示例。使用旧入口会产生 `CS0618` 警告；启用 warnings-as-errors 的工程需要迁移调用或明确处理该警告。

注册及解析示例：

```csharp
services.AddDapperForSQLite();
services.AddDapperForSQLite("Orders", "orders", enableMasterSlave: true);
services.AddDapperForSQLite("Reporting", "reporting");

// 在请求作用域或手动创建的作用域内解析。
var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
var defaultDb = resolver.ResolveDapper();
var master = resolver.ResolveDapper("orders");
var slave = resolver.ResolveDapper("orders", readOnly: true);
```

旧 Autofac 注册仍配合 `IResolveContext` 使用；迁移时将注册与解析一起切换。静态工厂调用可以替换为 `IServiceScopeFactory.CreateScope()` / `CreateAsyncScope()`，再在作用域内使用 `IDapperResolver`。读取角色仅选择从库，不代替数据库权限控制。

### 10.2 验证结果

| 验证 | 结果 |
| --- | --- |
| .NET 10 完整 xUnit 回归 | 104 通过，0 失败，0 跳过 |
| .NET 8 完整 xUnit 回归 | 104 通过，0 失败，0 跳过 |
| .NET 9 完整 xUnit 回归 | 104 通过，0 失败，0 跳过 |
| .NET 5.0.13 冒烟 | 默认／具名主从、双 bool 参数、SQLite 异步查询、监控、作用域释放通过 |
| .NET 6.0.36 冒烟 | 同上，通过 |
| .NET 7.0.20 冒烟 | 同上，通过 |
| 完整解决方案及声明目标框架构建 | 0 错误；588 条警告包含预期弃用警告与已有依赖／旧框架警告 |
| 公开／受保护 API 对比 | 10 个程序集、747 项迁移前契约，0 删除或签名变化；有意新增弃用属性 |
| 补充测试 | 本轮新增 23 个用例，保留上一轮全部 81 个用例 |

新增 `MicrosoftDiRegistrationTests` 验证 8 种默认／具名 × 主从 × 监控组合，以及作用域校验、默认提供者的真实配置绑定、6 种驱动创建、命名冲突、监控及缓存隔离、事务、同步／异步作用域释放、原生 keyed 构造器注入、旧重载兼容和无效输入。`AutofacDeprecationTests` 验证弃用属性确实是警告且没有误标共享 API；原有 Autofac 注册和工厂回归继续运行。

.NET 10 覆盖率实测：`DapperResolver` 行／分支覆盖率均为 **100%**；核心微软 DI 扩展类行覆盖率 **94.91%**、分支覆盖率 **100%**；新增注册闭包行／分支覆盖率均为 **100%**。核心程序集总体行覆盖率 **52.20%**、分支覆盖率 **60.13%**。这些覆盖率不代表已连接所有数据库服务。

跨运行时的完整测试需要先按目标框架显式还原，例如：

```powershell
dotnet restore XUnitTest/XUnitTest.csproj -p:TargetFramework=net8.0
dotnet test XUnitTest/XUnitTest.csproj -p:TargetFramework=net8.0 --no-restore

# net9.0 使用同样步骤；完成后恢复项目默认目标的构建资产。
dotnet build Dapper.Extensions.sln
dotnet test XUnitTest/XUnitTest.csproj --no-restore
```

### 10.3 验证边界

- .NET 5–7 仅完成上述关键路径的运行冒烟，不是完整 xUnit 套件；依赖包对旧框架的支持警告仍然存在，不能据冒烟结果宣称获得上游长期支持。
- 六种驱动均验证注册与实例创建，实际 SQL、事务和连接释放测试使用 SQLite。未连接真实 SQL Server、MySQL、PostgreSQL、Oracle、ODBC、Redis，也未验证真实主从复制或生产并发。
- NuGet 仍报告既有依赖警告，包括 Oracle 在部分目标下的传递依赖 `System.Security.Cryptography.Pkcs 8.0.1` 安全警告；本次未扩展为依赖升级或漏洞可利用性评估。
- 没有删除旧 API、发布 NuGet 包或提交 Git 提交。未来移除 Autofac 专用公开签名时仍需主版本迁移说明。

## 11. 发布前复核与文档补全

本节更新前面阶段性验证的结论：**现有 104 项回归通过，但新增边界复现确认了 4 个待修复问题，暂不建议发布。** 本轮未修改产品代码；根据后续要求补全了 README，并把迁移说明和广告分别移到后部及末尾。

### 11.1 发布前应修复的问题

| 优先级 | 位置 | 复现与影响 | 修复及回归建议 |
| --- | --- | --- | --- |
| P1 | `src/Dapper.Extensions/DependencyInjectionExtensions.cs:80–81` | 先以连接 A、键 `orders`、`enableMasterSlave: true` 注册，再用连接 B、相同键、`false` 注册。主库更新为 B，但旧的 A 从库描述符仍可解析，导致同一逻辑连接混用不同数据库。 | 明确拒绝重复键，或者以主从为一个整体替换旧注册；覆盖连接切换以及 `true → false` 的重复注册。 |
| P2 | `src/Dapper.Extensions/BaseDapper.cs:601` | 合法的自定义 `ICacheKeyBuilder` 要求 SQL 非空。查询使用显式缓存键可成功，但 `RemoveCache` 调用 `Generate(null, null, key)`，抛出 `ArgumentNullException`，缓存无法删除。 | 保留既有自定义键构建器契约，明确显式键的写入／失效规则，不依赖未约定的空 SQL 输入；覆盖自定义构建器查询、命中和删除。 |
| P2 | `src/Dapper.Extensions/Caching/CachedQueryResult.cs:12–18` | 外层配置了支持循环引用的 `IDataSerializer`，直接向 Redis 缓存提供者写入循环 DTO 成功；ORM 查询缓存包装层仍自行使用默认 `JsonConvert`，写入同样的对象失败。读取也绕过自定义转换配置。 | 让有效的序列化配置覆盖缓存载荷的读写，或保留由提供者控制的类型化序列化；覆盖自定义转换器与循环引用配置。 |
| P2 | `src/Dapper.Extensions/Monitor/CommandExecuteMonitor.cs:89` | 慢查询处理器或其作用域依赖仅实现 `IAsyncDisposable` 时，通知完成后通过同步 `using` 释放作用域，导致异常被监控隔离逻辑捕获，`DisposeAsync` 未执行。 | 在异步通知方法内使用异步作用域释放；分别验证同步查询包装路径与异步查询路径中异步资源均释放一次。 |

这些问题已通过独立程序复现，并使用打包后的 Release NuGet 包再次复现，不能由当前绿色测试结果排除。关键输出：

```text
Strict builder query result=1
CONFIRMED custom_key_invalidation: ArgumentNullException Value cannot be null. (Parameter 'sql')
Custom serializer direct control=True
CONFIRMED custom_serializer_bypassed: JsonSerializationException Error getting value from 'Json' on 'Dapper.Extensions.Caching.CachedQueryResult'.
CONFIRMED stale_slave: master=B,enabled=False; slave=A,readOnly=True
CONFIRMED async_monitor_disposal: calls=1,disposals=0
```

Redis 复现使用真实缓存提供者与 FreeRedis 拦截器，没有连接外部 Redis 服务。独立复现工程、日志及包消费工程保存在本机临时目录 `%TEMP%/dapper-release-review/`，未纳入源码或发布包。

### 11.2 本轮验证

- Release 全解决方案构建：0 错误、582 条警告；包含预期弃用及既有框架、依赖警告。
- Release .NET 10 完整回归：104 通过、0 失败、0 跳过。
- 解决方案内 10 个发布项目均成功打包，使用临时版本 `5.4.3-review.20261025`。检查目标框架程序集、README、图标、同版本核心包依赖，以及测试数据库和发布脚本未混入包。
- 独立包消费程序确认这些包能够还原和运行，也确认了上述 4 个缺陷。
- README 的 21 个 C# 代码块全部提取编译。实际执行了完整控制台示例、默认／具名主从解析、查询更新、分页、XML SQL 查询、内存缓存删除、监控注册及处理器调用、自定义连接字符串、旧 Autofac 解析与属性过滤、SQL 拼接和 Snowflake 生成；6 种驱动的文档命名空间及注册调用也通过编译检查。
- 完整 `Program.cs` 另用独立工程，仅引用 SQLite 验证包和 `Microsoft.Extensions.Hosting` 编译运行，输出 `Ada`；不依赖测试工程的其他程序集引用。
- JSON、XML 示例及 Markdown 目录锚点通过结构检查。Redis 单节点／分区示例只验证注册，不发起网络查询；SQL Server 与其他服务器数据库的 SQL 未在真实服务端执行。

### 11.3 README 调整

补齐安装与控制台示例、连接配置、多实例和主从、连接字符串扩展、查询分页、XML SQL、内存／Redis 缓存、监控、MiniProfiler、SQL 拼接及 Snowflake。修正内存缓存配置类型、数据库提供者命名空间大小写、SQL Server 分页缺少排序、SQL 名称重载选择、显式分页缓存键缺少过滤条件和页大小、旧 MD5 描述，以及只能通过 Autofac 使用多实例／主从的过时说明。

常规用法以微软 DI 为主；迁移说明和旧 Autofac 示例在后部，赞助与 Ko-fi 位于最后。正文不再包含外部文档站点的品牌、链接或来源说明。文档针对当前源码，明确新注册重载需要包含这些改动的包版本。

仍未发布 NuGet 包或修改正式版本号。修复上述问题并补回归后，再重新进行发布候选验证；当前临时包仅作为验证产物。

## 12. 依赖升级复核与 FromDapper 特性

本轮检查包含升级后的 Dapper 2.1.89、Autofac 9.3.4、Microsoft.Extensions 10.0.13、SQLite 2.0.4／原生库 3.53.4、MySqlConnector 2.6.2、Oracle 23.26.400，以及新的测试工具链。另根据用户后续要求增加了声明式主从注入。

### 12.1 新增声明式注入

`FromDapperAttribute` 位于 `Dapper.Extensions` 命名空间，继承微软 `FromKeyedServicesAttribute`，直接复用注册和解析已有的服务键规则，没有增加另一套实例注册或释放逻辑：

```csharp
public OrderService(
    [FromDapper("orders")] IDapper writer,
    [FromDapper("orders", readOnly: true)] IDapper reader)
{
    // writer 为主库，reader 为从库。
}
```

默认连接可使用 `[FromDapper]`；默认从库可使用 `[FromDapper(readOnly: true)]`。请求从库要求对应注册启用 `enableMasterSlave`。空白键按默认连接处理；缺失名称或角色会解析失败，不回退到主库。业务类型仍需正常注册到微软 DI，实例与 `IDapperResolver` 共用作用域。原生具名主库注入和旧 Autofac API 保持不变。

新增 `FromDapperAttributeTests` 的 6 个用例覆盖：默认／具名主从、类似从库后缀的独立服务名、监控开启／关闭、跨作用域隔离、每实例释放一次、与 resolver／原生 keyed 解析共享实例、`ActivatorUtilities`、空白键、具名业务类型不会意外把自己的键传给默认连接，以及缺失名称或从库时抛错。README 已补充示例，并保持迁移靠后、广告最后的布局。

### 12.2 本轮实测结果

| 检查 | 结果 |
| --- | --- |
| 升级后完整 Release 构建 | 0 错误，603 警告；仍有弃用、旧框架支持和安全公告警告 |
| 新特性定向测试 | 6 通过 |
| .NET 8 完整回归 | 110 通过，0 失败，0 跳过 |
| .NET 9 完整回归 | 110 通过，0 失败，0 跳过 |
| .NET 10 完整回归 | 110 通过，0 失败，0 跳过 |
| .NET 5.0.13／6.0.36／7.0.20 独立包消费冒烟 | 全部通过：特性选择默认／具名主从、SQLite 异步查询、监控、作用域释放 |
| 10 个 NuGet 包打包与内容检查 | 通过；包含最新 README、图标及预期目标程序集，同版本内部依赖正确 |
| README 新业务类型示例 | 从文档原样提取，参与上述独立包消费工程编译及解析验证 |
| README 结构 | JSON、XML、代码围栏、目录锚点和章节顺序通过检查 |

最终验证包使用独立临时版本 `5.4.3-dependencyreview.20261025.2`，避免复用上一轮 NuGet 缓存。产物和日志位于 `%TEMP%/dapper-dependency-review/`；正式项目版本号仍为 5.4.3。没有执行发布或提交。

### 12.3 发布结论：暂不发布

**第 11.1 节的 4 个问题全部仍可复现。** 本轮在升级后的 Release 程序集及独立 NuGet 消费工程中重新运行复现程序，依然得到自定义键删除异常、序列化设置被绕过、旧从库残留、异步监控资源未释放这 4 项结果。依赖升级未修复对应产品代码；新增特性也不改变这些路径。

另外，执行 `dotnet list Dapper.Extensions.sln package --vulnerable --include-transitive --no-restore --format json` 后确认，Oracle 发布项目在 net8.0／net9.0／net10.0 下仍解析到 `System.Security.Cryptography.Pkcs 8.0.1`，并报告高危与中危拒绝服务公告。上游 Oracle 23.26.400 的依赖声明仍指定该最低版本。微软公告列出的 8.x 修复版本是 **8.0.2**，发布前应在这些目标下提高有效依赖下限并重新验证消费图。[高危公告](https://github.com/advisories/GHSA-846f-f764-76mx)、[中危公告](https://github.com/advisories/GHSA-2fmv-rv2c-59x7)。

示例项目另解析到 `Pkcs 9.0.11`，也落在上述受影响范围内；相应 9.x 修复版本为 9.0.21。示例工具链还报告 `NuGet.Packaging`／`NuGet.Protocol 6.12.1` 的既有低危公告。示例和测试工程不会作为这 10 个库包发布，但它们自身的依赖警告仍需处理。

安全扫描确认的是存在公告覆盖的依赖版本，未证明本库的实际数据库调用会触发漏洞。未连接真实 MySQL、SQL Server、PostgreSQL、Oracle、ODBC 或 Redis 服务；.NET 5–7 仅做关键路径冒烟，上游不支持旧框架的警告仍然存在。本轮没有把这 4 个已知逻辑缺陷或安全依赖问题作为已解决项。

## 13. 缓存击穿保护：按键协调，移除全局查询限制

用户明确要求缓存锁用于防止击穿，不需要全局查库上限。本节取代前面“保留全局限流策略”的阶段性说明。

### 13.1 已修复的行为

- 同步、异步查询共用最终缓存键的锁。首次缓存命中直接返回；未命中时按键等待，获得锁后再次读取缓存，只有仍未命中的请求查库并回填。
- 不同键、不同连接无需等待全局信号量。旧 `maxConcurrent` 参数和公开 `SemaphoreSlim` 属性为兼容保留，不再控制查询并发；参数仍要求正值。
- 等待超时仍由 `acquireLockTimeoutSeconds` 控制，默认 5 秒，0 表示立即尝试，最大 2,147,483 秒。微软 DI／Autofac 的 Memory、Redis、分区 Redis 共 13 个注册入口均校验参数。异步等待支持取消。
- `RemoveCache` 为对应键更新失效版本，并与缓存写入按键排序。已开始的旧查询仍可返回自己的结果，但若其间发生失效，就不能回填旧结果。缓存实际写入已开始时，删除会等待该次写入结束再删除；不同键不受影响。
- 查询／缓存读写异常、超时和取消均释放自己的锁引用。等待者持有引用，最后一个使用者退出后才回收键状态及信号量，避免移除时仍有等待者或字典持续增长。
- 嵌套查询可读取已有缓存；未命中则直接查库、不写缓存，避免同键递归及跨键循环等待。显式 `forceUpdateCache: true` 保持逐次刷新语义，刷新操作按键串行，不合并为一次刷新。

现有查询、注册和删除缓存的公开签名均未改变。核心、Memory、Redis 三个程序集共 **692 项**改动前公开／受保护契约对比，0 删除或签名变化。README 已同步行为说明，迁移与广告的布局保持不变。

### 13.2 验证结果

- 新增 `CacheConcurrencyTests` **21 项**，并把原有 5 项缓存锁等待取消测试改为等待真实的同键查询，避免继续依赖已停用的全局信号量。
- 32 个同键并发请求只触发 **1 次查库、1 次写缓存**，旧并发参数为 1 或 8 时均成立；同步／异步请求共享同键保护。
- 验证不同键／连接并发、已命中请求不阻塞、删除发生在回填前及回填中的两种竞争、嵌套查询、强制刷新串行、超时／取消、锁内缓存读取异常、查询异常、缓存写入异常，以及异步上下文不残留。
- 32 个并发工作任务对 8 个键执行 640 次强制刷新，每个键最多只有一个加载者；全部完成后锁状态数量为 0。另验证 500 个不同键的回收。
- .NET 8、9、10 完整回归均为 **131 通过、0 失败、0 跳过**。全部声明目标框架 Release 构建通过，0 错误、609 警告，警告仍包含已有弃用、旧框架支持和安全依赖项。
- 10 个发布项目成功打包，临时验证版本为 `5.4.3-cachefix.20261025`；README、图标、目标程序集和内部依赖检查通过。
- .NET 5.0.13、6.0.36、7.0.20 的独立包消费冒烟全部通过，覆盖同键合并、不同键独立执行、失效与回填协调、嵌套查询及真实 SQLite 查询；不等同于这些旧运行时的完整回归。

验证日志与临时包保存在 `%TEMP%/dapper-cache-gate-fix/`。本次没有执行 NuGet 发布或修改正式版本号。

### 13.3 范围与剩余工作

这些协调规则仅在共享同一缓存协调器的容器内生效，不提供跨进程 Redis 分布式锁。真实外部数据库／Redis 服务和生产负载未验证。此前第 11.1 节中的自定义缓存键构建器删除异常、序列化配置绕过、重复 DI 注册残留从库、异步监控资源释放，以及第 12.3 节的安全依赖问题仍是独立待办，本轮缓存锁修复不代表已具备发布条件。

## 14. 已推送提交的最终发布检查

检查基准为 `5.5.0` 分支提交 `9754cbbf0412b042df1b17308fe8978fdd9cf0ad`。本轮未修改产品代码，也未发布。报告继续仅保留在本地 `docs/`，不提交。

重新构建所有声明目标框架：0 错误、600 警告；.NET 10 Release 完整回归：131 通过、0 失败、0 跳过。另用最新构建程序集重新运行独立复现程序，第 11.1 节的四个问题**全部仍然成立**：

1. 同键重复注册由 A 主从改为 B 单库后，实际解析结果仍为 B 主库配 A 从库。
2. 自定义缓存键构建器的查询成功，但删除同一键抛出 SQL 为空的 `ArgumentNullException`。
3. 自定义序列化器直接写入循环 DTO 成功，ORM 缓存包装层写入同一对象失败。
4. 异步监控处理器执行 1 次，但其 `DisposeAsync` 执行 0 次。

本轮 NuGet 漏洞扫描成功完成：Oracle 的 net8.0／net9.0／net10.0 消费图仍使用 `System.Security.Cryptography.Pkcs 8.0.1`，存在高危及中危公告。示例项目另有 `Pkcs 9.0.11` 和两个 NuGet 工具依赖告警；详情及已修复版本见第 12.3 节。10 个库项目的正式版本号仍为 5.4.3，分支名称不会自动改变包版本。

**结论：先处理上述四个代码问题和发布依赖告警，再进行候选版本验证；不能依据绿色测试宣称没有 bug。** 本轮没有发现需要另列的按键锁缺陷，但这不代表生产并发或跨进程 Redis 行为已通过验证。日志位于 `%TEMP%/dapper-final-review-20261025/`。

## 15. 发布阻塞问题修复与验证

本轮按用户“把 bug 先修复了”的要求，修复第 14 节的四项问题和安全依赖告警。以下结果取代前面这些问题仍未解决的阶段性结论。

### 15.1 修复内容

1. 微软 DI 新注册重载拒绝重复 serviceKey，默认键同样适用；检查发生在修改注册集合和调用监控配置回调之前。默认主从注册也不能被旧重载覆盖，避免新主库搭配旧从库。既有旧重载的普通重复注册兼容性保留。
2. 显式 cacheKey 的生成与删除共用同一规则：连接命名空间、配置前缀、显式键。自定义 ICacheKeyBuilder 仍负责自动键，但不会因 RemoveCache 收到空 SQL。跨实例删除、缓存命中和重新读取均通过验证。
3. 缓存查询载荷的序列化和反序列化均使用已注册 IDataSerializer，修复外层缓存使用自定义配置、内层却使用默认 Json.NET 设置的问题。覆盖自定义转换器与忽略循环引用，同步和异步路径均验证真实序列化往返。
4. 慢查询监控采用异步作用域释放。同步和异步查询都会等待仅实现 IAsyncDisposable 的处理器及其依赖释放；处理器和 SQL 异常时同样保证释放，监控异常不会替换数据库原始异常。

Oracle 项目 net8.0、net9.0、net10.0 的 Pkcs 依赖下限提高到 8.0.2；示例项目提高到 9.0.21，并将 NuGet.Packaging、NuGet.Protocol 提高到 6.12.5。依据仍为第 12.3 节的公告，修复版本通过实际还原和依赖扫描确认。

### 15.2 验证结果

| 项目 | 结果 |
| --- | --- |
| 新增回归用例 | 24 项：重复注册 8 项、自定义缓存 10 项、异步监控释放 6 项 |
| .NET 8 / 9 / 10 完整测试 | 每个框架 155 通过、0 失败、0 跳过 |
| 全解决方案所有声明目标 Release 构建 | 0 错误，580 警告；仍有弃用、旧框架支持及既有编译警告 |
| NuGet 已知漏洞扫描 | 13 个项目，含传递依赖，未报告漏洞项或扫描错误 |
| 10 个发布包 | 打包成功，README、图标、目标程序集、内部版本约束和排除项检查通过 |
| 公开／受保护 API 对比 | 相对已提交基准包共 760 项契约，0 删除或签名变化 |
| 独立 NuGet 消费 | 同时引用 10 个临时包；.NET 5–10 的 SQLite 与按键缓存协调冒烟全部通过 |
| Oracle 实际消费图 | net8.0、net9.0、net10.0 均解析到 Pkcs 8.0.2，与打包依赖一致 |

临时包版本为 `5.4.3-blockerfix.20261025`。日志、独立消费工程及验证包位于 `%TEMP%/dapper-release-blockers-fix/`。正式项目版本仍为 5.4.3；未提交、推送或发布，docs 继续仅保留本地。

### 15.3 兼容性及验证边界

- 显式键不再经过自定义键构建器改写。此前按自定义规则保存的显式键缓存项不会再被命中，应等待其过期，或通过缓存提供者清理原物理键；不涉及数据库数据变更。
- 新重载的重复注册会抛出 InvalidOperationException，调用方应为每个连接选择唯一 serviceKey。README 已同步这一规则。
- 本轮使用真实 SQLite；自定义缓存序列化回归通过模拟 Redis 序列化边界验证，未连接真实 Redis、MySQL、SQL Server、PostgreSQL、Oracle 或 ODBC 服务，未验证生产负载或跨进程互斥。
- .NET 5–7 只有包消费冒烟通过，依赖不支持旧框架的警告仍然存在，不能将其视为完整兼容认证。

上述已复现问题与依赖告警已经解决；本地候选包检查通过，不等于保证不存在其他 bug。正式发布前仍需选择并更新包版本，按部署所用数据库完成相应环境验证。
