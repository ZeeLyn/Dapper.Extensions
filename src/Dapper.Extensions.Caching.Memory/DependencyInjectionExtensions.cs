using System;
using System.Threading;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Caching.Memory
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperCachingInMemory(this IServiceCollection service,
            MemoryConfiguration config, int maxConcurrent = 1, int acquireLockTimeoutSeconds = 5)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            });
            service.AddMemoryCache();
            service.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            service.AddSingleton(new CacheConcurrencyConfig
            {
                MaxConcurrent = maxConcurrent,
                AcquireLockTimeout = acquireLockTimeoutSeconds
            });
            service.AddSingleton(new CacheSemaphoreSlim(maxConcurrent));
            return service;
        }

        public static IServiceCollection AddDapperCachingInMemory<TCacheKeyBuilder>(this IServiceCollection service,
            MemoryConfiguration config, int maxConcurrent = 1, int acquireLockTimeoutSeconds = 5)
            where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            });
            service.AddMemoryCache();
            service.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            service.AddSingleton(new CacheConcurrencyConfig
            {
                MaxConcurrent = maxConcurrent,
                AcquireLockTimeout = acquireLockTimeoutSeconds
            });
            service.AddSingleton(new CacheSemaphoreSlim(maxConcurrent));
            return service;
        }
    }
}