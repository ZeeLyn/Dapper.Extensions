using Autofac;

namespace Dapper.Extensions.PostgreSql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForPostgre(this ContainerBuilder container,string connectionName= "DefaultConnection", string named="default")
        {
            container.AddDapper<PostgreSqlDapper>(connectionName, named);
            return container;
        }
    }
}
