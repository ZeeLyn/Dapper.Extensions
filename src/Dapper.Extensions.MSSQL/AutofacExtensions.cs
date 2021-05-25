using System;
using Autofac;
using Dapper.Extensions.Monitor;

namespace Dapper.Extensions.MSSQL
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false, Action<MonitorBuilder> monitorBuilder = null)
        {
            container.AddDapper<MsSqlDapper>(connectionName, serviceKey, enableMasterSlave, monitorBuilder);
            return container;
        }

        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, Action<DapperBuilder> builder, Action<MonitorBuilder> monitorBuilder = null)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<MsSqlDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave, monitorBuilder);
            return container;
        }
    }
}
