using System;
using System.Reflection;
using Autofac;
using Autofac.Core;
using Autofac.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using Dapper.Extensions.MasterSlave;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.Hosting;

namespace Dapper.Extensions
{
    public static class AutofacExtensions
    {
        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddDapper<TDbProvider>(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false, Action<MonitorBuilder> monitorBuilder = null) where TDbProvider : IDapper
        {
            container.RegisterType<ResolveContext>().As<IResolveContext>().IfNotRegistered(typeof(IResolveContext)).InstancePerLifetimeScope();
            //container.RegisterType<ResolveKeyed>().As<IResolveKeyed>().IfNotRegistered(typeof(IResolveKeyed)).InstancePerLifetimeScope();
            container.RegisterType<DefaultConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
            container.RegisterType<WeightedPolling>().As<ILoadBalancing>().SingleInstance();

            MonitorConfiguration configuration = null;
            if (monitorBuilder != null)
            {
                var builder = new MonitorBuilder(container);
                monitorBuilder.Invoke(builder);
                configuration = new MonitorConfiguration
                {
                    SlowCriticalValue = builder.Threshold,
                    EnableLog = builder.EnableLog,
                    HasCustomMonitorHandler = builder.HasCustomMonitorHandler,
                    HandlerType = builder.HandlerType
                };
                container.RegisterInstance(configuration).SingleInstance();
            }

            if (string.IsNullOrWhiteSpace(serviceKey))
            {
                if (enableMasterSlave)
                {
                    if (monitorBuilder != null)
                    {
                        container.RegisterType<TDbProvider>().WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                        container.Register<IDapper>((ctx, @params) => new DapperProxy(ctx.Resolve<TDbProvider>(@params), ctx.Resolve<IServiceProvider>(), configuration)).InstancePerLifetimeScope();



                        container.RegisterType<TDbProvider>().Keyed<TDbProvider>("_slave").WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                        container.Register<IDapper>((ctx, @params) => new DapperProxy(ctx.ResolveKeyed<TDbProvider>("_slave", @params), ctx.Resolve<IServiceProvider>(), configuration))
                            .Keyed<IDapper>("_slave").InstancePerLifetimeScope();

                    }
                    else
                    {
                        container.RegisterType<TDbProvider>().As<IDapper>().WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                        container.RegisterType<TDbProvider>().Keyed<IDapper>("_slave").WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                    }
                }
                else
                {
                    if (monitorBuilder != null)
                    {
                        container.RegisterType<TDbProvider>().WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
                        container.Register<IDapper>((ctx, @params) => new DapperProxy(ctx.Resolve<TDbProvider>(@params), ctx.Resolve<IServiceProvider>(), configuration)).InstancePerLifetimeScope();
                    }
                    else
                        container.RegisterType<TDbProvider>().As<IDapper>().WithParameter("connectionName", connectionName)
                            .InstancePerLifetimeScope();
                }
            }
            else
            {
                if (enableMasterSlave)
                {
                    if (monitorBuilder != null)
                    {
                        container.RegisterType<TDbProvider>().Keyed<TDbProvider>(serviceKey).WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                        container.Register<IDapper>((ctx, @params) => new DapperProxy(ctx.ResolveKeyed<TDbProvider>(serviceKey, @params), ctx.Resolve<IServiceProvider>(), configuration)).Keyed<IDapper>(serviceKey).InstancePerLifetimeScope();

                        container.RegisterType<TDbProvider>().Keyed<TDbProvider>($"{serviceKey}_slave").WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                        container.Register<IDapper>((ctx, @params) => new DapperProxy(ctx.ResolveKeyed<TDbProvider>($"{serviceKey}_slave", @params), ctx.Resolve<IServiceProvider>(), configuration)).Keyed<IDapper>($"{serviceKey}_slave").InstancePerLifetimeScope();
                    }
                    else
                    {
                        container.RegisterType<TDbProvider>().Keyed<IDapper>(serviceKey).WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                        container.RegisterType<TDbProvider>().Keyed<IDapper>($"{serviceKey}_slave").WithParameters(new[]
                        {
                            new NamedParameter("connectionName", connectionName),
                            new NamedParameter("enableMasterSlave", true)
                        }).InstancePerLifetimeScope();
                    }
                }
                else
                {
                    if (monitorBuilder != null)
                    {
                        container.RegisterType<TDbProvider>().Keyed<TDbProvider>(serviceKey)
                            .WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
                        container.Register<IDapper>((ctx, @params) =>
                            new DapperProxy(ctx.ResolveKeyed<TDbProvider>(serviceKey, @params), ctx.Resolve<IServiceProvider>(), configuration)).Keyed<IDapper>(serviceKey).InstancePerLifetimeScope();
                    }
                    else
                        container.RegisterType<TDbProvider>().Keyed<IDapper>(serviceKey)
                            .WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
                }
            }


            return container;
        }

        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddDapperConnectionStringProvider<TConnectionStringProvider>(this ContainerBuilder container) where TConnectionStringProvider : IConnectionStringProvider
        {
            container.RegisterType<TConnectionStringProvider>().As<IConnectionStringProvider>().SingleInstance();
            return container;
        }

        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddAllControllers(this ContainerBuilder container)
        {
            container.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .Where(t => t.Name.EndsWith("Controller"))
                .PropertiesAutowired().WithAttributeFiltering().InstancePerLifetimeScope();
            return container;
        }

        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static IHostBuilder UseAutofac(this IHostBuilder builder)
        {
            return builder.UseServiceProviderFactory(new AutofacServiceProviderFactory());
        }

        /// <summary>
        /// Enable SQL separation
        /// </summary>
        /// <param name="services"></param>
        /// <param name="xmlRootDir">The root directory of the xml file</param>
        /// <returns></returns>
        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddSQLSeparationForDapper(this ContainerBuilder services, string xmlRootDir)
        {
            services.RegisterInstance(new SQLSeparateConfigure
            {
                RootDir = xmlRootDir
            });
            services.RegisterType<SQLManager>().As<ISQLManager>().SingleInstance();
            return services;
        }
    }
}
