using System;
using System.Runtime.Versioning;
using Autofac;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapper<TDbProvider>(this IServiceCollection services, Action<MonitorBuilder> monitorBuilder = null) where TDbProvider : IDapper
        {
            if (monitorBuilder != null)
            {
                var builder = new MonitorBuilder(services);
                monitorBuilder.Invoke(builder);
                services.AddSingleton(new MonitorConfiguration
                {
                    SlowCriticalValue = builder.Threshold,
                    EnableLog = builder.EnableLog,
                    HasCustomMonitorHandler = builder.HasCustomMonitorHandler
                });
                services.AddScoped(typeof(TDbProvider))
                    .AddScoped<IDapper>(sc => new DapperProxy(sc.GetRequiredService<TDbProvider>(), sc));
            }
            else
                services.AddScoped(typeof(IDapper), typeof(TDbProvider));

            return services.AddSingleton<IConnectionStringProvider, DefaultConnectionStringProvider>();
        }


        public static IServiceCollection AddDapperConnectionStringProvider<TConnectionStringProvider>(this IServiceCollection services) where TConnectionStringProvider : IConnectionStringProvider
        {
            return services.AddSingleton(typeof(IConnectionStringProvider), typeof(TConnectionStringProvider));
        }


        /// <summary>
        /// Enable SQL separation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="xmlRootDir">The root directory of the xml file</param>
        /// <returns></returns>
        public static IServiceCollection AddSQLSeparationForDapper(this IServiceCollection services, string xmlRootDir)
        {
            services.AddSingleton(new SQLSeparateConfigure
            {
                RootDir = xmlRootDir
            });
            return services.AddSingleton<ISQLManager, SQLManager>();
        }
    }
}
