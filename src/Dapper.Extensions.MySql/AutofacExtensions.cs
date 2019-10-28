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

        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, string connectionName = "DefaultConnection", bool enableMasterSlave = false)
        {
            container.AddDapper<MySqlDapper>(connectionName, null, enableMasterSlave);
            return container;
        }
    }
}
