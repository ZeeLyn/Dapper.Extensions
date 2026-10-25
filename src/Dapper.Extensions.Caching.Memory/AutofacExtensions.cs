using System;
using Autofac;
using Microsoft.Extensions.Caching.Memory;

namespace Dapper.Extensions.Caching.Memory
{
    public static class AutofacExtensions
    {
        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddDapperCachingInMemory(this ContainerBuilder service,
            MemoryConfiguration config, int maxConcurrent = 1, int acquireLockTimeoutSeconds = 5)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (maxConcurrent <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrent));
            if (acquireLockTimeoutSeconds < 0 || acquireLockTimeoutSeconds > int.MaxValue / 1000)
                throw new ArgumentOutOfRangeException(nameof(acquireLockTimeoutSeconds));
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
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

        [Obsolete("Autofac integration is deprecated and will be removed in a future major version. Use IServiceCollection registration and IDapperResolver instead.", false)]
        public static ContainerBuilder AddDapperCachingInMemory<TCacheKeyBuilder>(this ContainerBuilder service,
            MemoryConfiguration config, int maxConcurrent = 1, int acquireLockTimeoutSeconds = 5)
            where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (maxConcurrent <= 0)
                throw new ArgumentOutOfRangeException(nameof(maxConcurrent));
            if (acquireLockTimeoutSeconds < 0 || acquireLockTimeoutSeconds > int.MaxValue / 1000)
                throw new ArgumentOutOfRangeException(nameof(acquireLockTimeoutSeconds));
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
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