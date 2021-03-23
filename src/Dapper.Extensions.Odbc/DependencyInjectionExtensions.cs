using System;
using Dapper.Extensions.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Odbc
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForODBC(this IServiceCollection services, Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<OdbcDapper>(monitorBuilder);
        }
    }
}
