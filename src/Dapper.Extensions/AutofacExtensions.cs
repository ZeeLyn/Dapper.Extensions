using Autofac;
using Dapper.Extensions.MasterSlave;
using Dapper.Extensions.SQL;

namespace Dapper.Extensions
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapper<TDbProvider>(this ContainerBuilder container, string connectionName = "DefaultConnection", object serviceKey = null, bool enableMasterSlave = false) where TDbProvider : IDapper
        {
            container.RegisterType<ResolveKeyed>().As<IResolveKeyed>().IfNotRegistered(typeof(IResolveKeyed)).InstancePerLifetimeScope();
            container.RegisterType<ConnectionConfigureManager>().IfNotRegistered(typeof(ConnectionConfigureManager)).SingleInstance();
            container.RegisterType<WeightedPolling>().As<ILoadBalancing>().IfNotRegistered(typeof(ILoadBalancing)).SingleInstance();
            var builder = serviceKey == null ? container.RegisterType<TDbProvider>().As<IDapper>().WithParameter("connectionName", connectionName).InstancePerLifetimeScope() : container.RegisterType<TDbProvider>().Keyed<IDapper>(serviceKey).WithParameter("connectionName", connectionName).InstancePerLifetimeScope();

            if (enableMasterSlave)
                builder.WithParameter("enableMasterSlave", true);
            return container;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="services"></param>
        /// <param name="xmlRootDir">The root directory of the xml file</param>
        /// <returns></returns>
        public static ContainerBuilder AddSQLSeparateForDapper(this ContainerBuilder services, string xmlRootDir)
        {
            services.RegisterInstance(new SQLSeparateConfigure
            {
                RootDir = xmlRootDir
            });
            services.RegisterType<SQLManager>().As<ISQLManager>().SingleInstance();
            return services;
        }
    }
}
