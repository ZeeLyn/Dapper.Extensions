using Autofac;

namespace Dapper.Extensions.Odbc
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForODBC(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<OdbcDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }
    }
}
