using System.IO;
using Autofac;
using Dapper.Extensions.SQL;

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

        public static ContainerBuilder AddSQLSeparateForDapper(this ContainerBuilder services, string xmlRootDir)
        {
            if (!Directory.Exists(xmlRootDir))
                throw new FileNotFoundException($"Directory not found {xmlRootDir}.");
            services.RegisterInstance(new SQLManager(xmlRootDir)).As<ISQLManager>().SingleInstance();
            return services;
        }
    }
}
