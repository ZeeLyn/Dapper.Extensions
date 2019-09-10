using Autofac;

namespace Dapper.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapper<TDbProvider>(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null) where TDbProvider : IDapper
        {
            container.RegisterType<ResolveKeyed>().As<IResolveKeyed>().IfNotRegistered(typeof(IResolveKeyed)).InstancePerLifetimeScope();
            if (serviceKey == null)
                container.RegisterType<TDbProvider>().As<IDapper>().WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
            else
                container.RegisterType<TDbProvider>().Keyed<IDapper>(serviceKey).WithParameter("connectionName", connectionName).InstancePerLifetimeScope();
            return container;
        }

        public static ContainerBuilder AddDapperForMSSQL(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null)
        {
            container.AddDapper<MsSqlDapper>(connectionName, serviceKey);
            return container;
        }
    }
}
