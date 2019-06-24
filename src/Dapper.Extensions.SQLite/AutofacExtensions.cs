using Autofac;

namespace Dapper.Extensions.SQLite
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForSQLite(this ContainerBuilder container, string connectionName = "DefaultConnection", string name = "default")
        {
            container.AddDapper<SQLiteDapper>(connectionName, name);
            return container;
        }
    }
}
