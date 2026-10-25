using System;
using Dapper.Extensions.MasterSlave;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
                var configuration = new MonitorConfiguration
                {
                    SlowCriticalValue = builder.Threshold,
                    EnableLog = builder.EnableLog,
                    HasCustomMonitorHandler = builder.HasCustomMonitorHandler,
                    HandlerType = builder.HandlerType
                };
                services.AddSingleton(configuration);
                services.AddScoped(typeof(TDbProvider))
                    .AddScoped<IDapper>(sc => new DapperProxy(sc.GetRequiredService<TDbProvider>(), sc, configuration));
            }
            else
                services.AddScoped(typeof(IDapper), typeof(TDbProvider));

            return AddSharedServices(services);
        }

        /// <summary>
        /// 注册独立连接。serviceKey 为 null 或空白时注册默认连接；主从实例分别按作用域管理。
        /// </summary>
        /// <remarks>提供者构造器中的配置参数顺序为 string connectionName、bool enableMasterSlave、bool readOnly；其他依赖由 DI 提供。</remarks>
        public static IServiceCollection AddDapper<TDbProvider>(this IServiceCollection services,
            string connectionName, string serviceKey, bool enableMasterSlave = false,
            Action<MonitorBuilder> monitorBuilder = null) where TDbProvider : IDapper
        {
            if (services == null) throw new ArgumentNullException(nameof(services));
            if (string.IsNullOrWhiteSpace(connectionName)) throw new ArgumentException("A connection name is required.", nameof(connectionName));
            serviceKey = DapperResolver.NormalizeKey(serviceKey);
            var factory = ActivatorUtilities.CreateFactory(typeof(TDbProvider),
                new[] { typeof(string), typeof(bool), typeof(bool) });
            MonitorConfiguration configuration = null;
            if (monitorBuilder != null)
            {
                var builder = new MonitorBuilder(services);
                monitorBuilder(builder);
                configuration = new MonitorConfiguration
                {
                    SlowCriticalValue = builder.Threshold,
                    EnableLog = builder.EnableLog,
                    HasCustomMonitorHandler = builder.HasCustomMonitorHandler,
                    HandlerType = builder.HandlerType
                };
                services.AddSingleton(configuration);
            }

            void Register(bool readOnly)
            {
                var key = DapperResolver.GetKey(serviceKey, readOnly);
                if (configuration == null)
                    services.AddKeyedScoped(typeof(IDapper), key,
                        (provider, _) => factory(provider, new object[] { connectionName, enableMasterSlave, readOnly }));
                else
                {
                    // 代理不拥有底层连接；独立注册让容器负责释放，且避免同类型多连接互相覆盖。
                    var providerKey = new object();
                    services.AddKeyedScoped(typeof(TDbProvider), providerKey,
                        (provider, _) => factory(provider, new object[] { connectionName, enableMasterSlave, readOnly }));
                    services.AddKeyedScoped(typeof(IDapper), key, (provider, _) => new DapperProxy(
                        (IDapper)provider.GetRequiredKeyedService(typeof(TDbProvider), providerKey), provider, configuration));
                }
            }

            Register(false);
            if (enableMasterSlave) Register(true);
            return AddSharedServices(services);
        }

        private static IServiceCollection AddSharedServices(IServiceCollection services)
        {
            services.TryAddSingleton<IConnectionStringProvider, DefaultConnectionStringProvider>();
            services.TryAddSingleton<ILoadBalancing, WeightedPolling>();
            services.TryAddScoped<IDapperResolver, DapperResolver>();
            return services;
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
