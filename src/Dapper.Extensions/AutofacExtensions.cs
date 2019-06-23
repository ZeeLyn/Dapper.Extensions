using Autofac;

namespace Dapper.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapper<TDbProvider>(this ContainerBuilder container,string connectionName= "DefaultConnection", string named="default") where TDbProvider : IDapper
        {
            container.RegisterType<ResolveNamed>().As<IResolveNamed>().IfNotRegistered(typeof(ResolveNamed)).InstancePerLifetimeScope();
            container.RegisterType<TDbProvider>().Named<IDapper>(named).WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
            return container;
        }

        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", string named = "default")
        {
            container.AddDapper<MsSqlDapper>(connectionName, named);
            return container;
        }
    }
}
