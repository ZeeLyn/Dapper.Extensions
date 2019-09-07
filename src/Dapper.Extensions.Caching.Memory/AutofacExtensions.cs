using Autofac;
using Microsoft.Extensions.Caching.Memory;

namespace Dapper.Extensions.Caching.Memory
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperCachingInMemory(this ContainerBuilder service, MemoryConfiguration config)
        {
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            service.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
            service.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInMemory<TCacheKeyBuilder>(this ContainerBuilder service, MemoryConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            service.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
            service.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();
            return service;
        }
    }
}
