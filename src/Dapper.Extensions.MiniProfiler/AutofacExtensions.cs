using Autofac;

namespace Dapper.Extensions.MiniProfiler
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddMiniProfilerForDapper(this ContainerBuilder services)
        {
            services.RegisterType<DbMiniProfiler>().As<IDbMiniProfiler>().SingleInstance();
            return services;
        }
    }
}
