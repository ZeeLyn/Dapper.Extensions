using Autofac;

namespace Dapper.Extensions.MySql
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForMySQL(this ContainerBuilder container,string connectionName= "DefaultConnection", string named="default")
        {
            container.AddDapper<MySqlDapper>(connectionName, named);
            return container;
        }
    }
}
