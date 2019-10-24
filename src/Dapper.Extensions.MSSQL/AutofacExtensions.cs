using Autofac;

namespace Dapper.Extensions.MSSQL
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null, bool enableMasterSlave = false)
        {
            container.AddDapper<MsSqlDapper>(connectionName, serviceKey, enableMasterSlave);
            return container;
        }
    }
}
