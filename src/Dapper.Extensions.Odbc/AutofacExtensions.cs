using Autofac;

namespace Dapper.Extensions.Odbc
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForODBC(this ContainerBuilder container,string connectionName= "DefaultConnection", string named="default")
        {
            container.AddDapper<OdbcDapper>(connectionName, named);
            return container;
        }
    }
}
