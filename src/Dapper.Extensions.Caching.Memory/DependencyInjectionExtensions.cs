using System;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Caching.Memory
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperCachingInMemory(this IServiceCollection service, MemoryConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            });
            service.AddMemoryCache();
            service.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInMemory<TCacheKeyBuilder>(this IServiceCollection service, MemoryConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            });
            service.AddMemoryCache();
            service.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            return service;
        }
    }
}
