using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;

namespace Dapper.Extensions.Caching.Redis
{
    public static class DependencyInjectionExtensions
    {
        private static bool Exist<TServiceType, TImplementationType>(this IServiceCollection service)
        {
            return service.Any(p =>
                p.ServiceType == typeof(TServiceType) && p.ImplementationType == typeof(TImplementationType));
        }

        public static IServiceCollection AddDapperCachingInRedis(this IServiceCollection service, RedisConfiguration config)
        {
            if (!service.Exist<ICacheKeyBuilder, DefaultCacheKeyBuilder>())
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                service.AddSingleton(new CacheConfiguration
                {
                    Enable = config.Enable,
                    Expire = config.Expire
                });
                RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            }
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInRedis<TCacheKeyBuilder>(this IServiceCollection service, RedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (!service.Exist<ICacheKeyBuilder, TCacheKeyBuilder>())
            {
                service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
                service.AddSingleton(new CacheConfiguration
                {
                    Enable = config.Enable,
                    Expire = config.Expire
                });
                RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            }
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis(this IServiceCollection service, PartitionRedisConfiguration config)
        {
            if (!service.Exist<ICacheKeyBuilder, DefaultCacheKeyBuilder>())
            {
                service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
                service.AddSingleton(config);
                RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()), config.Connections.ToArray()));
            }
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis<TCacheKeyBuilder>(this IServiceCollection service, PartitionRedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (!service.Exist<ICacheKeyBuilder, TCacheKeyBuilder>())
            {
                service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
                service.AddSingleton(config);
                RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()), config.Connections.ToArray()));
            }
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }
    }
}
