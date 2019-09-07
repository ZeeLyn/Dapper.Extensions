using System.Linq;
using Microsoft.Extensions.DependencyInjection;

namespace Dapper.Extensions.Caching.Memory
{
    public static class DependencyInjectionExtensions
    {
        private static bool Exist<TServiceType, TImplementationType>(this IServiceCollection service)
        {
            return service.Any(p =>
                p.ServiceType == typeof(TServiceType) && p.ImplementationType == typeof(TImplementationType));
        }

        public static IServiceCollection AddDapperCachingInMemory(this IServiceCollection service, MemoryConfiguration config)
        {
            if (!service.Exist<ICacheKeyBuilder, DefaultCacheKeyBuilder>())
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                service.AddSingleton(new CacheConfiguration
                {
                    Enable = config.Enable,
                    Expire = config.Expire
                });
                service.AddMemoryCache();
            }
            service.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInMemory<TCacheKeyBuilder>(this IServiceCollection service, MemoryConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (!service.Exist<ICacheKeyBuilder, TCacheKeyBuilder>())
            {
                service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
                service.AddSingleton(new CacheConfiguration
                {
                    Enable = config.Enable,
                    Expire = config.Expire
                });
                service.AddMemoryCache();
            }
            service.AddSingleton<ICacheProvider, MemoryCacheProvider>();
            return service;
        }
    }
}
