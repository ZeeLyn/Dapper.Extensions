using Autofac;

namespace Dapper.Extensions.SQLite
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForSQLite(this ContainerBuilder container, string connectionName = "DefaultConnection", string serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<SQLiteDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }
    }
}
