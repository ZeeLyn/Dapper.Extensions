using System;
using Autofac;
using Dapper.Extensions.Monitor;

namespace Dapper.Extensions.Oracle
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForOracle(this ContainerBuilder container,
            string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false,
            Action<MonitorBuilder> monitorBuilder = null)
        {
            container.AddDapper<OracleDapper>(connectionName, serviceKey, enableMasterSlave, monitorBuilder);
            return container;
        }

        public static ContainerBuilder AddDapperForOracle(this ContainerBuilder container,
            Action<DapperBuilder> builder,
            Action<MonitorBuilder> monitorBuilder = null)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<OracleDapper>(configure.ConnectionName, configure.ServiceKey,
                configure.EnableMasterSlave,
                monitorBuilder);
            return container;
        }
    }
}