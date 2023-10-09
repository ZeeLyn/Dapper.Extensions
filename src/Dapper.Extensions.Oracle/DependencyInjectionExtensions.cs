using System;
using Dapper.Extensions.Monitor;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Oracle
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperForOracle(this IServiceCollection services,
            Action<MonitorBuilder> monitorBuilder = null)
        {
            return services.AddDapper<OracleDapper>(monitorBuilder);
        }
    }
}