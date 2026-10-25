using System;
using Autofac;
using Dapper.Extensions.Monitor;

namespace Dapper.Extensions.MySql
{
    public static class AutofacExtensions
    {
        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false, Action<MonitorBuilder> monitorBuilder = null)
        {
            container.AddDapper<MySqlDapper>(connectionName, serviceKey, enableMasterSlave, monitorBuilder);
            return container;
        }


        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, Action<DapperBuilder> builder, Action<MonitorBuilder> monitorBuilder = null)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<MySqlDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave, monitorBuilder);
            return container;
        }
    }
}
