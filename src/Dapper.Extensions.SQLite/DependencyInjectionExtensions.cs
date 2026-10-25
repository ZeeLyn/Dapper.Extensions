using System;
using Dapper.Extensions.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.SQLite
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForSQLite(this IServiceCollection services, Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<SQLiteDapper>(monitorBuilder);
        }
        /// <summary>注册默认或具名连接；serviceKey 为 null 时注册默认连接。</summary>
        public static IServiceCollection AddDapperForSQLite(this IServiceCollection services,
            string connectionName, string serviceKey, bool enableMasterSlave = false,
            Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<SQLiteDapper>(connectionName, serviceKey, enableMasterSlave, monitorBuilder);
        }
    }
}
