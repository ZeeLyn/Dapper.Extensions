using Autofac;

namespace Dapper.Extensions.Autofac
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapper<TDbProvider>(this ContainerBuilder container,string connectionName= "DefaultConnection", string named="default") where TDbProvider : IDapper
        {
            container.RegisterType<TDbProvider>().Named<IDapper>(named).WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
            return container;
        }
    }
}
