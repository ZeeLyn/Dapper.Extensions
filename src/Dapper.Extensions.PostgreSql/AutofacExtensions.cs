using Autofac;

namespace Dapper.Extensions.PostgreSql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForPostgreSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null)
        {
            container.AddDapper<PostgreSqlDapper>(connectionName, serviceKey);
            return container;
        }
    }
}
