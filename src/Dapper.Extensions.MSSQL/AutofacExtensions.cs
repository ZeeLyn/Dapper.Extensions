using System;
using Autofac;

namespace Dapper.Extensions.MSSQL
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<MsSqlDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }

        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, Action<DapperBuilder> builder)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<MsSqlDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave);
            return container;
        }
    }
}
