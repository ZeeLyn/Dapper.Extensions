using System;
using Autofac;

namespace Dapper.Extensions.MiniProfiler
{
    public static class AutofacExtensions
    {
        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddMiniProfilerForDapper(this ContainerBuilder services)
        {
            services.RegisterType<DbMiniProfiler>().As<IDbMiniProfiler>().SingleInstance();
            return services;
        }
    }
}
