# Dapper.Extensions

A Dapper extension library for .NET applications.

1. MySQL, SQL Server, PostgreSQL, SQLite, Oracle and ODBC providers.
2. In-memory and Redis query caching.
3. SQL stored in XML files, with reload support.
4. Multiple connections and explicit master/slave selection through Microsoft DI.
5. Slow-query monitoring and MiniProfiler integration.

## Documentation

This guide describes the current repository APIs. The new Microsoft DI overloads and `FromDapperAttribute` are available in this source tree and require a package release containing these changes.

- [Getting started](#getting-started)
- [Connections and Microsoft DI](#connections-and-microsoft-di)
- [Queries and paging](#queries-and-paging)
- [SQL in XML files](#sql-in-xml-files)
- [Caching](#caching)
- [Monitoring and MiniProfiler](#monitoring-and-miniprofiler)
- [SQL fragments and Snowflake IDs](#sql-fragments-and-snowflake-ids)
- [Compatibility notes](#compatibility-notes-for-the-cache-and-monitoring-fixes)
- [Autofac migration and legacy usage](#moving-from-autofac-to-microsoft-di)

## Packages & Status

| Packages|NuGet|Download|Note|
| ------------- | -----------------|--------------- |---|
| Dapper.Extensions.NetCore  | [![Dapper.Extensions.NetCore](https://img.shields.io/nuget/v/Dapper.Extensions.NetCore.svg)](https://www.nuget.org/packages/Dapper.Extensions.NetCore) | [![Dapper](https://img.shields.io/nuget/dt/Dapper.Extensions.NetCore.svg)](https://www.nuget.org/packages/Dapper.Extensions.NetCore/)||
|Dapper.Extensions.MySQL|[![Dapper.Extensions.MySQL](https://img.shields.io/nuget/v/Dapper.Extensions.MySQL.svg)](https://www.nuget.org/packages/Dapper.Extensions.MySQL/)| [![Dapper](https://img.shields.io/nuget/dt/Dapper.Extensions.MySQL.svg)](https://www.nuget.org/packages/Dapper.Extensions.MySQL/)||
| Dapper.Extensions.MSSQL          | [![Dapper.Extensions.MSSQL](https://img.shields.io/nuget/v/Dapper.Extensions.MSSQL.svg)](https://www.nuget.org/packages/Dapper.Extensions.MSSQL)                  | [![Dapper.Extensions.MSSQL](https://img.shields.io/nuget/dt/Dapper.Extensions.MSSQL.svg)](https://www.nuget.org/packages/Dapper.Extensions.MSSQL/) ||
| Dapper.Extensions.PostgreSQL     | [![Dapper.Extensions.PostgreSQL](https://img.shields.io/nuget/v/Dapper.Extensions.PostgreSQL.svg)](https://www.nuget.org/packages/Dapper.Extensions.PostgreSQL)         |[![Dapper.Extensions.PostgreSQL](https://img.shields.io/nuget/dt/Dapper.Extensions.PostgreSQL.svg)](https://www.nuget.org/packages/Dapper.Extensions.PostgreSQL/) ||
| Dapper.Extensions.ODBC           | [![Dapper.Extensions.ODBC](https://img.shields.io/nuget/v/Dapper.Extensions.ODBC.svg)](https://www.nuget.org/packages/Dapper.Extensions.ODBC)                     |[![Dapper.Extensions.ODBC](https://img.shields.io/nuget/dt/Dapper.Extensions.ODBC.svg)](https://www.nuget.org/packages/Dapper.Extensions.ODBC/) ||
| Dapper.Extensions.SQLite         | [![Dapper.Extensions.SQLite](https://img.shields.io/nuget/v/Dapper.Extensions.SQLite.svg)](https://www.nuget.org/packages/Dapper.Extensions.SQLite)                 |[![Dapper.Extensions.SQLite](https://img.shields.io/nuget/dt/Dapper.Extensions.SQLite.svg)](https://www.nuget.org/packages/Dapper.Extensions.SQLite/) ||
| ZeeLyn.Dapper.Extensions.Oracle    | [![Dapper.Extensions.Oracle](https://img.shields.io/nuget/v/ZeeLyn.Dapper.Extensions.Oracle.svg)](https://www.nuget.org/packages/ZeeLyn.Dapper.Extensions.Oracle)              |[![ZeeLyn.Dapper.Extensions.Oracle](https://img.shields.io/nuget/dt/ZeeLyn.Dapper.Extensions.Oracle.svg)](https://www.nuget.org/packages/ZeeLyn.Dapper.Extensions.Oracle/) |The Oracle provider uses the ZeeLyn package prefix.|
| Dapper.Extensions.Caching.Redis  | [![Dapper.Extensions.Caching.Redis](https://img.shields.io/nuget/v/Dapper.Extensions.Caching.Redis.svg)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Redis)   |[![Dapper.Extensions.Caching.Redis](https://img.shields.io/nuget/dt/Dapper.Extensions.Caching.Redis.svg)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Redis/) ||
| Dapper.Extensions.Caching.Memory | [![Dapper.Extensions.Caching.Memory](https://img.shields.io/nuget/v/Dapper.Extensions.Caching.Memory.svg)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Memory) |[![Dapper.Extensions.Caching.Memory](https://img.shields.io/nuget/dt/Dapper.Extensions.Caching.Memory.svg)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Memory/) ||
| Dapper.Extensions.MiniProfiler   | [![Dapper.Extensions.NetCore](https://img.shields.io/nuget/v/Dapper.Extensions.MiniProfiler.svg)](https://www.nuget.org/packages/Dapper.Extensions.MiniProfiler)     |[![Dapper.Extensions.MiniProfiler](https://img.shields.io/nuget/dt/Dapper.Extensions.MiniProfiler.svg)](https://www.nuget.org/packages/Dapper.Extensions.MiniProfiler/) || 

## Getting started

Install the provider for your database. It brings in `Dapper.Extensions.NetCore`; cache and profiling packages are optional. For a console example using SQLite and Microsoft DI:

```shell
dotnet new console -n DapperDemo
cd DapperDemo
dotnet add package Dapper.Extensions.SQLite
dotnet add package Microsoft.Extensions.Hosting
```

This complete `Program.cs` uses an in-memory SQLite database, creates its sample table and queries it. It needs no database server or configuration file.

```csharp
using System;
using System.Collections.Generic;
using Dapper.Extensions;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

var services = new ServiceCollection();
IConfiguration configuration = new ConfigurationBuilder()
    .AddInMemoryCollection(new Dictionary<string, string>
    {
        ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:"
    })
    .Build();
services.AddSingleton(configuration);
services.AddLogging();
services.AddDapperForSQLite();

await using var provider = services.BuildServiceProvider(
    new ServiceProviderOptions { ValidateScopes = true });
await using var scope = provider.CreateAsyncScope();
var db = scope.ServiceProvider.GetRequiredService<IDapper>();

await db.ExecuteAsync("CREATE TABLE Contact (Id INTEGER PRIMARY KEY, Name TEXT NOT NULL)");
await db.ExecuteAsync("INSERT INTO Contact (Id, Name) VALUES (@Id, @Name)",
    new { Id = 1, Name = "Ada" });
var contacts = await db.QueryAsync<Contact>(
    "SELECT Id, Name FROM Contact WHERE Id = @Id", new { Id = 1 });
Console.WriteLine(contacts[0].Name);

public sealed class Contact
{
    public long Id { get; set; }
    public string Name { get; set; } = "";
}
```

The following snippets use `services` for the application's `IServiceCollection`, `db` for a scoped `IDapper`, and the `Contact` model above. In ASP.NET Core, use `builder.Services`; the host already registers configuration and logging. Inject `IDapper` into your controller or service constructor for the default connection. The console example uses modern C# top-level statements.

## Connections and Microsoft DI

### Connection strings

The parameterless registration reads `ConnectionStrings:DefaultConnection`. For a host that loads `appsettings.json`, a default database, a separate reporting database and a master/slave group can be configured as follows:

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=default.db",
    "Reporting": "Data Source=reporting.db",
    "Orders": {
      "Master": "Data Source=orders.db",
      "Slaves": [
        { "ConnectionString": "Data Source=orders-read-1.db", "Weight": 4 },
        { "ConnectionString": "Data Source=orders-read-2.db", "Weight": 6 }
      ]
    }
  }
}
```

These file paths illustrate routing only. The library does not create schemas or replicate data between these files. Use connection strings and a replication setup appropriate to your database provider.

### Registering providers and named connections

Import the namespace matching the installed provider and choose its extension:

| Database | Namespace | Registration |
| --- | --- | --- |
| SQLite | `Dapper.Extensions.SQLite` | `AddDapperForSQLite()` |
| SQL Server | `Dapper.Extensions.MSSQL` | `AddDapperForMSSQL()` |
| MySQL | `Dapper.Extensions.MySql` | `AddDapperForMySQL()` |
| PostgreSQL | `Dapper.Extensions.PostgreSql` | `AddDapperForPostgreSQL()` |
| Oracle | `Dapper.Extensions.Oracle` | `AddDapperForOracle()` |
| ODBC | `Dapper.Extensions.Odbc` | `AddDapperForODBC()` |

Register one default provider, then use distinct service keys for additional connections:

```csharp
using Dapper.Extensions;
using Dapper.Extensions.SQLite;

services.AddDapperForSQLite();
services.AddDapperForSQLite("Reporting", "reporting");
services.AddDapperForSQLite("Orders", "orders", enableMasterSlave: true);
```

`connectionName` selects a configuration entry; `serviceKey` identifies the registration. The new overload requires both arguments. Pass `serviceKey: null` to configure an unnamed default connection with additional options. The same overload is available for all six providers. Register each service key only once. The new overload rejects duplicate keys (including the default key) before changing any registration or invoking the monitoring callback. A default master/slave registration also cannot be overwritten by the legacy overload.

### Selecting the master or a slave

Resolve named instances through `IDapperResolver` inside a request scope or an explicitly created scope:

```csharp
var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
var defaultDb = resolver.ResolveDapper();
var reporting = resolver.ResolveDapper("reporting");
var writer = resolver.ResolveDapper("orders");
var reader = resolver.ResolveDapper("orders", readOnly: true);
```

Inject `IDapperResolver` into application services when you need this selection. `enableMasterSlave: true` registers both roles; `readOnly: true` selects a slave using weighted polling. Role selection is explicit: executing a `SELECT` on `writer` still uses the master. A missing key or a slave request for a registration without master/slave support throws. `readOnly` does not enforce database permissions. For a read immediately following a write, use the writer when replication lag is unacceptable.

Instances are scoped and owned by the container. The same key and role reuse an instance within a scope; a slave's connection is selected lazily and retained by that instance. Do not manually dispose an injected instance, retain it after its scope ends, or use its connection/transaction concurrently. Background jobs and console applications must create a scope.

### Constructor injection with FromDapper

Use `FromDapperAttribute` when the connection and role are known at construction time. With the `orders` master/slave registration above:

```csharp
using Dapper.Extensions;

public sealed class OrderService
{
    public IDapper Writer { get; }
    public IDapper Reader { get; }

    public OrderService(
        [FromDapper("orders")] IDapper writer,
        [FromDapper("orders", readOnly: true)] IDapper reader)
    {
        Writer = writer;
        Reader = reader;
    }
}
```

Register the consumer with Microsoft DI:

```csharp
services.AddScoped<OrderService>();
```

| Constructor parameter | Instance |
| --- | --- |
| `[FromDapper] IDapper db` | Default master/standalone connection |
| `[FromDapper(readOnly: true)] IDapper db` | Default slave |
| `[FromDapper("orders")] IDapper db` | Named master/standalone connection |
| `[FromDapper("orders", readOnly: true)] IDapper db` | Named slave |

The selected registration must have `enableMasterSlave: true` when requesting a slave, including the default registration. A missing name or role fails resolution; it does not fall back to the master. Null or whitespace service keys select the default connection. The attribute uses the same instances and lifetime as `IDapperResolver`; it does not register extra instances or require an additional registration extension. It works with Microsoft DI constructor injection and `ActivatorUtilities`.

Named masters also support `[FromKeyedServices("orders")] IDapper`. Use `FromDapper` for declarative role selection, or `IDapperResolver` when choosing a connection at runtime. This attribute is for Microsoft DI registrations; legacy Autofac consumers continue to use `DependencyDapperAttribute` during migration.

### Custom connection-string sources

Implement `IConnectionStringProvider` to obtain connection strings from another source, then register it with `AddDapperConnectionStringProvider<T>()`. For example, this provider reads a single connection from an environment variable and explicitly rejects unsupported names or routing options:

```csharp
using System;
using Dapper.Extensions;

public sealed class EnvironmentConnectionStringProvider : IConnectionStringProvider
{
    public string GetConnectionString(string connectionName,
        bool enableMasterSlave = false, bool readOnly = false)
    {
        if (connectionName != "DefaultConnection" || enableMasterSlave || readOnly)
            throw new NotSupportedException("Only the default standalone connection is configured.");

        var value = Environment.GetEnvironmentVariable("DAPPER_CONNECTION_STRING");
        return !string.IsNullOrWhiteSpace(value)
            ? value
            : throw new InvalidOperationException("DAPPER_CONNECTION_STRING is missing.");
    }
}
```

```csharp
services.AddDapperConnectionStringProvider<EnvironmentConnectionStringProvider>();
```

Custom providers are singletons. Keep any dependencies compatible with that lifetime. The library still requires `IConfiguration` and logging to be registered, as shown in the console example.

## Queries and paging

Use SQL parameters for values. The wrapper exposes typed and dynamic queries, single-row queries, commands and asynchronous variants through `IDapper`. Async overloads accepting `cancellationToken` forward it to the database operation. `Query<T>` and `QueryAsync<T>` return materialized lists.

```csharp
var contact = await db.QuerySingleOrDefaultAsync<Contact>(
    "SELECT Id, Name FROM Contact WHERE Id = @Id", new { Id = 1 });
var affected = await db.ExecuteAsync(
    "UPDATE Contact SET Name = @Name WHERE Id = @Id",
    new { Id = 1, Name = "Grace" });
```

### Paging

`QueryPageAsync<T>` accepts separate count and data SQL. Page numbers start at 1; use a positive page size. Apply the same filters to both queries, and use a deterministic `ORDER BY` in the data query. For SQLite, PostgreSQL or MySQL:

```csharp
var page = await db.QueryPageAsync<Contact>(
    countSql: "SELECT COUNT(*) FROM Contact WHERE Id >= @MinId",
    dataSql: "SELECT Id, Name FROM Contact WHERE Id >= @MinId ORDER BY Id LIMIT @Take OFFSET @Skip",
    pageindex: 1,
    pageSize: 20,
    param: new { MinId = 1 });
```

`PageResult<T>` contains `Page`, `PageSize`, `TotalCount`, `TotalPage` and `Result`. A request beyond the last page retains the requested page number and normally returns an empty `Result`. The library supplies these SQL parameters; do not reuse their names for business filters:

| Parameter | Value |
| --- | --- |
| `@Skip` | `(pageindex - 1) * pageSize` |
| `@Take` | `pageSize` |
| `@TakeStart` | `(pageindex - 1) * pageSize + 1` |
| `@TakeEnd` | `pageindex * pageSize` |

Alternative data SQL for MySQL:

```sql
SELECT Id, Name FROM Contact WHERE Id >= @MinId ORDER BY Id LIMIT @Skip, @Take;
```

For SQL Server 2012 and later, `OFFSET` requires `ORDER BY`:

```sql
SELECT Id, Name FROM Contact WHERE Id >= @MinId
ORDER BY Id OFFSET @Skip ROWS FETCH NEXT @Take ROWS ONLY;
```

For SQL Server versions supporting `ROW_NUMBER()`:

```sql
SELECT Id, Name FROM (
    SELECT Id, Name, ROW_NUMBER() OVER (ORDER BY Id) AS RowNumber
    FROM Contact WHERE Id >= @MinId
) AS NumberedContacts
WHERE RowNumber BETWEEN @TakeStart AND @TakeEnd
ORDER BY RowNumber;
```

## SQL in XML files

Register an existing absolute directory containing your `.xml` files:

```csharp
using System;
using System.IO;
using Dapper.Extensions;

services.AddSQLSeparationForDapper(Path.Combine(AppContext.BaseDirectory, "sql"));
```

For example, save `sql/contacts.xml` with globally unique command names:

```xml
<?xml version="1.0" encoding="utf-8"?>
<sql-set>
  <sql name="Contact.byId"><![CDATA[
    SELECT Id, Name FROM Contact WHERE Id = @Id
  ]]></sql>
  <paging-sql name="Contact.page">
    <count><![CDATA[SELECT COUNT(*) FROM Contact WHERE Id >= @MinId]]></count>
    <query><![CDATA[
      SELECT Id, Name FROM Contact WHERE Id >= @MinId
      ORDER BY Id LIMIT @Take OFFSET @Skip
    ]]></query>
  </paging-sql>
</sql-set>
```

Copy these files to build and publish output in an SDK-style project:

```xml
<ItemGroup>
  <None Update="sql/**/*.xml" CopyToOutputDirectory="PreserveNewest" CopyToPublishDirectory="PreserveNewest" />
</ItemGroup>
```

Select the SQL-name overload explicitly. A bare positional string would select the raw-SQL overload:

```csharp
using Dapper.Extensions.SQL;

var contactsByName = await db.QueryAsync<Contact>(
    new SQLName("Contact.byId"), param: new { Id = 1 });
var pageByName = await db.QueryPageAsync<Contact>(
    new SQLName("Contact.page"), pageindex: 1, pageSize: 20, param: new { MinId = 1 });
```

This is external SQL storage, not a dynamic XML templating language. The directory is watched for XML changes; a valid reload replaces the SQL snapshot, while an invalid reload is logged and retains the previous snapshot. Initial loading errors fail resolution. Reloading SQL does not automatically invalidate application-selected cache keys.

## Caching

Install either `Dapper.Extensions.Caching.Memory` or `Dapper.Extensions.Caching.Redis`, and register one cache backend for the container. The following examples opt into caching per query. `AllMethodsEnableCache` actually defaults to `true`; `Expire` defaults to `null` (no configured expiration), and `KeyPrefix` defaults to `dapper_cache`.

### In-memory cache

```csharp
using Dapper.Extensions.Caching.Memory;

services.AddDapperCachingInMemory(new MemoryConfiguration
{
    AllMethodsEnableCache = false,
    Expire = TimeSpan.FromMinutes(1)
});
```

### Redis cache

```csharp
using Dapper.Extensions.Caching.Redis;

services.AddDapperCachingInRedis(new RedisConfiguration
{
    AllMethodsEnableCache = false,
    ConnectionString = "localhost:6379,defaultDatabase=0",
    Expire = TimeSpan.FromMinutes(1)
});
```

Load production Redis credentials from your application's configuration. If using partitioned Redis instead, configure at least two connections; this replaces the single Redis registration:

```csharp
services.AddDapperCachingInPartitionRedis(new PartitionRedisConfiguration
{
    AllMethodsEnableCache = false,
    Connections = new[]
    {
        "localhost:6379,defaultDatabase=1",
        "localhost:6379,defaultDatabase=2"
    },
    Expire = TimeSpan.FromMinutes(1)
});
```

The `CacheConfiguration, RedisClient` overload accepts an existing FreeRedis client. Generic cache registration overloads accept an `ICacheKeyBuilder` implementation for automatic keys. An explicit `cacheKey` uses the connection namespace, `KeyPrefix`, and the supplied key directly, so queries and `RemoveCache` agree without passing a null SQL statement to a custom builder. Existing entries whose explicit keys were transformed by a custom builder will no longer be read; let them expire or remove them using the cache provider. Redis uses the registered `IDataSerializer` for both query payloads and cache envelopes, including configured converters and reference-loop handling.

### Query keys, expiration and invalidation

```csharp
var pageIndex = 1;
var pageSize = 20;
var minId = 1;
var cacheKey = $"contact-page:min:{minId}:page:{pageIndex}:size:{pageSize}";
var cachedPage = await db.QueryPageAsync<Contact>(
    "SELECT COUNT(*) FROM Contact WHERE Id >= @MinId",
    "SELECT Id, Name FROM Contact WHERE Id >= @MinId ORDER BY Id LIMIT @Take OFFSET @Skip",
    pageIndex, pageSize, param: new { MinId = minId },
    enableCache: true, cacheKey: cacheKey, cacheExpire: TimeSpan.FromSeconds(100));

// Invalidate this entry after a relevant write has committed.
db.RemoveCache(cacheKey);
```

The explicit key must distinguish every input affecting the result: filters, page number, page size, tenant and mapping options where applicable. Give different queries distinct keys. `RemoveCache` accepts the same application key, without adding the library's prefix or connection namespace. Writes do not invalidate cached queries automatically; invalidate all affected entries after commit.

Without an explicit key, the default builder uses SHA-256 over SQL and normalized parameters, including page coordinates for paging. Results are also isolated by connection, result type and query operation. `enableCache: false` bypasses caching for one call; `forceUpdateCache: true` refreshes an enabled cache entry. `cacheExpire` overrides the configured `Expire`; when both are `null`, no expiration is configured. Redis TTL values must be at least one millisecond.

Opaque `DynamicParameters` bypass automatic caching with the default builder; provide an explicit key if needed. Multi-mapping also requires an explicit key to cache results, including the mapping function's relevant inputs in that key. Transactional queries bypass the shared cache. See the compatibility notes below for upgrade details.

Cache misses are coordinated by the final cache key within the container. One caller loads and stores a missing entry; waiting callers check the cache again after acquiring that key's lock. Different keys and connections have no global query limit. The legacy `maxConcurrent` argument and `CacheSemaphoreSlim.SemaphoreSlim` property remain for source/binary compatibility, but no longer control query concurrency; the argument still requires a positive value. `acquireLockTimeoutSeconds` controls waiting for the same key and defaults to 5 seconds. It accepts 0 (do not wait) through 2,147,483 seconds; a timeout throws `DapperCacheException`, and async waits honor cancellation.

`RemoveCache` also invalidates pending fills for that key: a query already in progress can return its result to its caller, but cannot store it after invalidation. Writes and deletion are ordered per key. Idle key locks are removed after all holders and waiters exit. A nested query inside a cache load uses existing cache hits, but bypasses cache loading and storage on a miss to avoid recursive or cross-key deadlocks. Explicit `forceUpdateCache: true` requests still refresh sequentially under the key lock; they are not combined into a single refresh. These guarantees apply within the same container, not across application processes sharing Redis.

## Monitoring and MiniProfiler

### Slow-query monitoring

Supply the monitoring callback when registering the provider (instead of registering it a second time):

```csharp
using Dapper.Extensions.SQLite;

services.AddDapperForSQLite(monitor =>
{
    monitor.Threshold = 200;
    monitor.EnableLog = true;
    monitor.AddMonitorHandler<SlowQueryHandler>();
});
```

`Threshold` is measured in milliseconds and defaults to 200. An execution exceeding it triggers monitoring. `EnableLog` defaults to `true`; a handler is optional. The named registration overload accepts the same callback through `monitorBuilder`.

```csharp
using System.Threading.Tasks;
using Dapper.Extensions.Monitor;
using Microsoft.Extensions.Logging;

public sealed class SlowQueryHandler : IMonitorHandler
{
    private readonly ILogger<SlowQueryHandler> logger;

    public SlowQueryHandler(ILogger<SlowQueryHandler> logger) => this.logger = logger;

    public Task OnSlowSqlCommandAsync(string methodName, string sqlOrSqlName,
        object param, long duration)
    {
        logger.LogWarning("Slow database operation {Method}: {Duration} ms", methodName, duration);
        return Task.CompletedTask;
    }
}
```

Handlers run in a separate scope. The query waits for the handler and asynchronous scope disposal to complete, including resources that implement only `IAsyncDisposable`, so keep both bounded. Monitoring failures do not replace the query result or its original exception. Built-in logging includes SQL and parameters; disable it and use a redacting handler when those values are sensitive.

### MiniProfiler

Install `Dapper.Extensions.MiniProfiler` and register its connection wrapper:

```csharp
using Dapper.Extensions.MiniProfiler;

services.AddMiniProfilerForDapper();
```

Configure MiniProfiler and its profiling session in the host application as well. This extension alone does not configure ASP.NET Core middleware or a results UI.

## SQL fragments and Snowflake IDs

### Conditional SQL fragments

`Splice` consumes brace-delimited fragments in order. `{fragment}` keeps its SQL when the corresponding condition is true; `{whenTrue:whenFalse}` chooses one branch. Use it for SQL structure and keep data in parameters:

```csharp
using Dapper.Extensions;

var id = 1;
var descending = false;
var sql = "SELECT Id, Name FROM Contact WHERE 1 = 1 {AND Id = @Id} ORDER BY Id {DESC:ASC}"
    .Splice(id > 0, descending);
var filtered = await db.QueryAsync<Contact>(sql, new { Id = id });
```

This produces `SELECT Id, Name FROM Contact WHERE 1 = 1 AND Id = @Id ORDER BY Id ASC`. Supply one condition per fragment; nested braces and literal braces in SQL are not handled by a SQL parser. An overload accepts a custom branch separator instead of `:`.

### Snowflake IDs

```csharp
using Dapper.Extensions;

// Initialize once at application startup.
SnowflakeUtils.Initialization(workerId: 1, datacenterId: 1);
var generatedId = SnowflakeUtils.GenerateId();
```

Both IDs must be between 0 and 31. Assign each concurrently running generator a distinct worker/datacenter pair; the library does not coordinate this across machines. Initialize before generating IDs and do not repeatedly reset the generator. Clock rollback can cause generation to throw.

## Compatibility notes for the cache and monitoring fixes

Public method signatures, optional parameters and the existing `cacheKey` / `RemoveCache(...)` call pattern are preserved. Cache entries now use a versioned namespace isolated by the resolved connection, result type and query operation. Old entries are not read; allow them to expire, or remove old permanent entries through your cache administration process. Direct access to library-generated cache keys or serialized entries must account for the new format.

- Queries inside a library transaction or an explicit `CommandDefinition.Transaction` bypass the shared cache.
- The default key builder cannot inspect unbound `DynamicParameters` templates reliably. These queries bypass automatic caching; provide an explicit `cacheKey` or an `ICacheKeyBuilder` that understands your parameters when caching is required. Calling the default builder directly with opaque dynamic parameters and no custom key throws `NotSupportedException`.
- Multi-mapping queries require an explicit `cacheKey` to cache mapped results. Choose different keys for different mapping functions, captured values and split rules. Ordinary paging retains automatic caching, including the original business filters and page coordinates.
- Explicit keys belong to the resolved connection namespace. Use the same connection configuration when invalidating them with `RemoveCache(...)`. Writes still require the application's existing cache invalidation policy.
- Slow-query handlers complete before their scope is disposed. Async queries await the handler; synchronous queries wait for it. Monitoring failures are logged when possible and do not replace SQL results or database exceptions. Keep handlers bounded to avoid delaying slow-query callers.
- Redis expirations must be at least one millisecond; `null` means no expiration. Out-of-range pagination returns the requested page number with its actual results, including an empty result.

Cache-miss coordination now uses per-key locks instead of a global query limit. Existing registration signatures and their defaults are preserved; `maxConcurrent` is a compatibility argument, while `acquireLockTimeoutSeconds` still controls how long a caller waits. Applications needing database-wide throttling must configure that separately.

## Moving from Autofac to Microsoft DI

Autofac-specific registration extensions, `IResolveContext` / `ResolveContext` / `ResolveKeyed`, the static `DapperFactory` / `ExtensionBuilder`, and the Autofac members of `MonitorBuilder` are now marked `[Obsolete(..., false)]`. They remain functional and produce migration warnings. `IDapper`, the Microsoft DI constructor of `MonitorBuilder`, and the query APIs are not deprecated. The Autofac packages remain dependencies for compatibility until a future major version removes the old public signatures. Projects treating warnings as errors must migrate the affected calls or explicitly handle `CS0618` during the transition.

Use the Microsoft DI registrations and `IDapperResolver` described above. Migrate registration and resolution together: legacy Autofac registrations use `IResolveContext`, while Microsoft DI registrations use `IDapperResolver`. `DapperFactory` callbacks can be replaced with `IServiceScopeFactory.CreateScope()` / `CreateAsyncScope()` and resolution inside that scope, as in the console example.

Existing `AddDapperForSQLite(null)` and `AddDapperForSQLite(monitor => ...)` calls remain valid. The new overload requires both `connectionName` and `serviceKey` to avoid ambiguity with the old callback overload. Generic `AddDapper<TDbProvider>` supports the same options: custom providers must accept configuration arguments in the order `string connectionName`, `bool enableMasterSlave`, `bool readOnly`; DI supplies other constructor dependencies. Shared registration preserves an existing custom connection-string provider.

### Legacy Autofac registration

For applications that still use Autofac, the following retains the old registration and resolution pattern. It intentionally produces deprecation warnings. `services` must already contain configuration and logging, as in the console example. When a host already integrates Autofac, place the registrations in its container configuration instead of building a second container.

```csharp
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Extensions;
using Dapper.Extensions.SQLite;

var containerBuilder = new ContainerBuilder();
containerBuilder.Populate(services);
containerBuilder.AddDapperForSQLite("Reporting", "reporting");
containerBuilder.AddDapperForSQLite("Orders", "orders", enableMasterSlave: true);
using var container = containerBuilder.Build();
using var lifetime = container.BeginLifetimeScope();
var context = lifetime.Resolve<IResolveContext>();
var legacyWriter = context.ResolveDapper("orders");
var legacyReader = context.ResolveDapper("orders", readOnly: true);
```

### Legacy constructor filters

`DependencyDapperAttribute` is an Autofac constructor filter. Register each consumer with `WithAttributeFiltering()`; it is not a Microsoft DI attribute. For example:

```csharp
using Dapper.Extensions;

public sealed class LegacyOrderService
{
    public IDapper Writer { get; }
    public IDapper Reader { get; }

    public LegacyOrderService(
        [DependencyDapper("orders")] IDapper writer,
        [DependencyDapper("orders", readOnly: true)] IDapper reader)
    {
        Writer = writer;
        Reader = reader;
    }
}
```

Before `containerBuilder.Build()`, register the consumer:

```csharp
using Autofac;
using Autofac.Features.AttributeFilters;

containerBuilder.RegisterType<LegacyOrderService>()
    .WithAttributeFiltering().InstancePerLifetimeScope();
```

## ZZZ Projects - Sponsorship

[Dapper Plus](https://dapper-plus.net/) and [Entity Framework Extensions](https://entityframework-extensions.net/) are major sponsors and are proud to contribute to the development of Dapper.Extensions

[![Dapper Plus - Sponsor](https://raw.githubusercontent.com/ZeeLyn/Dapper.Extensions/master/dapper-plus-sponsor.png)](https://dapper-plus.net/bulk-insert)

[![Entity Framework Extensions - Sponsor](https://raw.githubusercontent.com/ZeeLyn/Dapper.Extensions/master/entity-framework-extensions-sponsor.png)](https://entityframework-extensions.net/bulk-insert)

[![ko-fi](https://ko-fi.com/img/githubbutton_sm.svg)](https://ko-fi.com/O5O31DZRRR)
