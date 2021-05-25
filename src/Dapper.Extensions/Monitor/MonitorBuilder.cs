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

        protected ContainerBuilder ContainerBuilder { get; }

        /// <summary>
        /// Slow SQL command execution time critical value, greater than this value will trigger the monitoring event, default 200, unit millisecond
        /// </summary>
        public int Threshold { get; set; } = 200;


        public MonitorBuilder(IServiceCollection service)
        {
            Service = service;

        }
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
            Service?.AddScoped(typeof(IMonitorHandler), typeof(TMonitorHandler));
            ContainerBuilder?.RegisterType<TMonitorHandler>().As<IMonitorHandler>().InstancePerLifetimeScope();
            HasCustomMonitorHandler = true;
        }

        protected internal bool HasCustomMonitorHandler { get; set; }


        /// <summary>
        /// Enable the log, the default value is open
        /// </summary>
        public bool EnableLog { get; set; } = true;
    }

    public class MonitorConfiguration
    {
        public int SlowCriticalValue { get; set; }

        public bool EnableLog { get; set; }

        public bool HasCustomMonitorHandler { get; set; }
    }
}
