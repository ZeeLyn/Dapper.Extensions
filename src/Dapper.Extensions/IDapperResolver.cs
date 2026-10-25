namespace Dapper.Extensions
{
    /// <summary>按连接注册名称和读写角色获取当前作用域的 Dapper 实例，不暴露容器类型。</summary>
    public interface IDapperResolver
    {
        /// <summary>获取默认连接；readOnly 为 true 时要求该注册启用了主从。</summary>
        IDapper ResolveDapper(bool readOnly = false);

        /// <summary>获取具名连接；同一名称及角色在当前作用域内复用，实例由容器释放。</summary>
        IDapper ResolveDapper(string serviceKey, bool readOnly = false);
    }
}
