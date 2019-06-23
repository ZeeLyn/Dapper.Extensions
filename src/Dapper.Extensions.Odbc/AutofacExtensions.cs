using Autofac;

namespace Dapper.Extensions.Odbc
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForODBC(this ContainerBuilder container,string connectionName= "DefaultConnection", string name="default")
        {
            container.AddDapper<OdbcDapper>(connectionName, name);
            return container;
        }
    }
}
