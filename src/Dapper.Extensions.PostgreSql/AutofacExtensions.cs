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
    }
}
