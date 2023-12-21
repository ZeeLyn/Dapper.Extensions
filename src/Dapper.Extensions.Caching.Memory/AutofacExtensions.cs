using System;
using Autofac;
using Microsoft.Extensions.Caching.Memory;

namespace Dapper.Extensions.Caching.Memory
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperCachingInMemory(this ContainerBuilder service,
            MemoryConfiguration config, int maxConcurrent = 1, int acquireLockTimeoutSeconds = 5)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            service.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
            service.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterInstance(new CacheConcurrencyConfig
            {
                MaxConcurrent = maxConcurrent,
                AcquireLockTimeout = acquireLockTimeoutSeconds
            }).SingleInstance();
            service.RegisterInstance(new CacheSemaphoreSlim(maxConcurrent)).SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInMemory<TCacheKeyBuilder>(this ContainerBuilder service,
            MemoryConfiguration config, int maxConcurrent = 1, int acquireLockTimeoutSeconds = 5)
            where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            service.RegisterType<MemoryCache>().As<IMemoryCache>().SingleInstance();
            service.RegisterType<MemoryCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterInstance(new CacheConcurrencyConfig
            {
                MaxConcurrent = maxConcurrent,
                AcquireLockTimeout = acquireLockTimeoutSeconds
            }).SingleInstance();
            service.RegisterInstance(new CacheSemaphoreSlim(maxConcurrent)).SingleInstance();
            return service;
        }
    }
}