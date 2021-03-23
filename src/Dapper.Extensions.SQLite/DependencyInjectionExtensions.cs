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
    }
}
