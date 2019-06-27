using Autofac;

namespace Dapper.Extensions.SQLite
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForSQLite(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null)
        {
            container.AddDapper<SQLiteDapper>(connectionName, serviceKey);
            return container;
        }
    }
}
