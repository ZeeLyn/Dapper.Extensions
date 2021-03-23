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
    }
}
