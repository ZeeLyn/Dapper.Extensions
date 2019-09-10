using System;
using CSRedis;
using Microsoft.Extensions.DependencyInjection;
using System.Linq;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;

namespace Dapper.Extensions.Caching.Redis
{
    public static class DependencyInjectionExtensions
    {
        public static IServiceCollection AddDapperCachingInRedis(this IServiceCollection service, RedisConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            });
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInRedis<TCacheKeyBuilder>(this IServiceCollection service, RedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            });
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.AddSingleton<ICacheProvider, RedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis(this IServiceCollection service, PartitionRedisConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Connections.Count() < 2)
                throw new ArgumentException("Need at least 2 redis nodes.", nameof(config.Connections));
            service.AddSingleton<ICacheKeyBuilder, DefaultCacheKeyBuilder>();
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            });
            RedisHelper.Initialization(config.PartitionPolicy != null
                ? new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()),
                    config.Connections.ToArray())
                : new CSRedisClient(null, config.Connections.ToArray()));
            service.AddSingleton<IDistributedCache>(new CSRedisCache(RedisHelper.Instance));
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }

        public static IServiceCollection AddDapperCachingInPartitionRedis<TCacheKeyBuilder>(this IServiceCollection service, PartitionRedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Connections.Count() < 2)
                throw new ArgumentException("Need at least 2 redis nodes.", nameof(config.Connections));
            service.AddSingleton(typeof(ICacheKeyBuilder), typeof(TCacheKeyBuilder));
            service.AddSingleton(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            });
            RedisHelper.Initialization(config.PartitionPolicy != null
                ? new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()),
                    config.Connections.ToArray())
                : new CSRedisClient(null, config.Connections.ToArray()));
            service.AddSingleton<IDistributedCache>(new CSRedisCache(RedisHelper.Instance));
            service.AddSingleton<ICacheProvider, PartitionRedisCacheProvider>();
            service.AddSingleton<IDataSerializer, DataSerializer>();
            return service;
        }
    }
}
