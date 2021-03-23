using System;
using Dapper.Extensions.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.MSSQL
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForMSSQL(this IServiceCollection services, Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<MsSqlDapper>(monitorBuilder);
        }
    }
}
