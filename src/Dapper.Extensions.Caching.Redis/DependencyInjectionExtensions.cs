using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Dapper.Extensions.Caching.Redis
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperCachingForRedis(this IServiceCollection service, RedisConfiguration config)
        {
            if (!service.Any(p => p.ServiceType == typeof(ICacheKeyBuilder) && p.ImplementationType == typeof(DefaultCacheKeyBuilder)))
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            }
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingForPartitionRedis(this IServiceCollection service, PartitionRedisConfiguration config)
        {
            if (!service.Any(p => p.ServiceType == typeof(ICacheKeyBuilder) && p.ImplementationType == typeof(DefaultCacheKeyBuilder)))
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections), config.Connections));
            }
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }
    }
}
