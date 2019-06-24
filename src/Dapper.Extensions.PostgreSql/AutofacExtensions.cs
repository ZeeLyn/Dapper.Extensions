using Autofac;

namespace Dapper.Extensions.PostgreSql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForPostgreSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string name = "default")
        {
            container.AddDapper<PostgreSqlDapper>(connectionName, name);
            return container;
        }
    }
}
