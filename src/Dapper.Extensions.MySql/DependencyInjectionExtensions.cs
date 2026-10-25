using System;
using Dapper.Extensions.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.MySql
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForMySQL(this IServiceCollection services, Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<MySqlDapper>(monitorBuilder);
        }
        /// <summary>注册默认或具名连接；serviceKey 为 null 时注册默认连接。</summary>
        public static IServiceCollection AddDapperForMySQL(this IServiceCollection services,
            string connectionName, string serviceKey, bool enableMasterSlave = false,
            Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<MySqlDapper>(connectionName, serviceKey, enableMasterSlave, monitorBuilder);
        }
    }
}
