using System;
using Autofac;

namespace Dapper.Extensions.MySql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false, bool enableMonitor = false)
        {
            container.AddDapper<MySqlDapper>(connectionName, serviceKey, enableMasterSlave, enableMonitor);
            return container;
        }


        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, Action<DapperBuilder> builder, bool enableMonitor = false)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<MySqlDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave, enableMonitor);
            return container;
        }
    }
}
