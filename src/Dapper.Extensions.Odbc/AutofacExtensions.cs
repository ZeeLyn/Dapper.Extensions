using Autofac;

namespace Dapper.Extensions.Odbc
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperForODBC(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null)
        {
            container.AddDapper<OdbcDapper>(connectionName, serviceKey);
            return container;
        }
    }
}
