using Autofac;

namespace Dapper.Extensions.MySql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null)
        {
            container.AddDapper<MySqlDapper>(connectionName, serviceKey);
            return container;
        }
    }
}
