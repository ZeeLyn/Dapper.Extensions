using System;
using System.Collections.Generic;
using System.Text;
using Autofac;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Monitor
{
    public class MonitorBuilder
    {
        private IServiceCollection Service { get; }

        [Obsolete("Use IServiceCollection-based monitoring registration instead.", false)]
        protected ContainerBuilder ContainerBuilder { get; }

        /// <summary>
        /// Slow SQL command execution time critical value, greater than this value will trigger the monitoring event, default 200, unit millisecond
        /// </summary>
        public int Threshold { get; set; } = 200;


        public MonitorBuilder(IServiceCollection service)
        {
            Service = service;

        }
        [Obsolete("Use MonitorBuilder(IServiceCollection) instead. Autofac support will be removed in a future major version.", false)]
        public MonitorBuilder(ContainerBuilder containerBuilder)
        {
            ContainerBuilder = containerBuilder;

        }
        /// <summary>
        /// Add a custom handler
        /// </summary>
        /// <typeparam name="TMonitorHandler"></typeparam>
        public void AddMonitorHandler<TMonitorHandler>() where TMonitorHandler : IMonitorHandler
        {
            Service?.AddScoped(typeof(TMonitorHandler));
            Service?.AddScoped(typeof(IMonitorHandler), provider => provider.GetRequiredService<TMonitorHandler>());
            HandlerType = typeof(TMonitorHandler);
            ContainerBuilder?.RegisterType<TMonitorHandler>().AsSelf().As<IMonitorHandler>().InstancePerLifetimeScope();
            HasCustomMonitorHandler = true;
        }

        internal Type HandlerType { get; private set; }

        protected internal bool HasCustomMonitorHandler { get; set; }


        /// <summary>
        /// Enable the log, the default value is open
        /// </summary>
        public bool EnableLog { get; set; } = true;
    }

    public class MonitorConfiguration
    {
        internal Type HandlerType { get; set; }
        public int SlowCriticalValue { get; set; }

        public bool EnableLog { get; set; }

        public bool HasCustomMonitorHandler { get; set; }
    }
}
