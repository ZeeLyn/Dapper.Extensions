using Autofac;

namespace Dapper.Extensions.SQLite
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForSQLite(this ContainerBuilder container,string connectionName= "DefaultConnection", string named="default")
        {
            container.AddDapper<SQLiteDapper>(connectionName, named);
            return container;
        }
    }
}
