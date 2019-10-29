using System;
using Autofac;

namespace Dapper.Extensions.PostgreSql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForPostgreSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<PostgreSqlDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }

        public static ContainerBuilder AddDapperForPostgreSQL(this ContainerBuilder container, Action<DapperBuilder> builder)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<PostgreSqlDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave);
            return container;
        }
    }
}
