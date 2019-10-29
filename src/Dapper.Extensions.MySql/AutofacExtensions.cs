using System;
using Autofac;

namespace Dapper.Extensions.MySql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<MySqlDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }


        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, Action<DapperBuilder> builder)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<MySqlDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave);
            return container;
        }
    }
}
