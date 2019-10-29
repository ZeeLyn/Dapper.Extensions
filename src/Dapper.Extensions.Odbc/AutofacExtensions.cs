using System;
using Autofac;

namespace Dapper.Extensions.Odbc
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForODBC(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<OdbcDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }

        public static ContainerBuilder AddDapperForODBC(this ContainerBuilder container, Action<DapperBuilder> builder)
        {
            var configure = new DapperBuilder();
            builder?.Invoke(configure);
            container.AddDapper<OdbcDapper>(configure.ConnectionName, configure.ServiceKey, configure.EnableMasterSlave);
            return container;
        }
    }
}
