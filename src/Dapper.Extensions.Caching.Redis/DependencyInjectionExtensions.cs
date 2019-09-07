using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Dapper.Extensions.Caching.Redis
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperCachingInRedis(this IServiceCollection service, RedisConfiguration config)
        {
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                Enable = config.Enable,
                Expire = config.Expire
            });
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInRedis<TCacheKeyBuilder>(this IServiceCollection service, RedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(new CacheConfiguration
            {
                Enable = config.Enable,
                Expire = config.Expire
            });
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis(this IServiceCollection service, PartitionRedisConfiguration config)
        {
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(config);
            RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()), config.Connections.ToArray()));
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis<TCacheKeyBuilder>(this IServiceCollection service, PartitionRedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(config);
            RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()), config.Connections.ToArray()));
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }
    }
}
