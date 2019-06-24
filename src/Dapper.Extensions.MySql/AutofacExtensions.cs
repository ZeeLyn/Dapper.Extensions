using Autofac;

namespace Dapper.Extensions.MySql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string name = "default")
        {
            container.AddDapper<MySqlDapper>(connectionName, name);
            return container;
        }
    }
}
