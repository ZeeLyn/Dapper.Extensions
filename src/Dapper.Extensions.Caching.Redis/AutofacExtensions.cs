using System;
using System.Linq;
using Autofac;
using CSRedis;
using Microsoft.Extensions.Caching.Distributed;
using Microsoft.Extensions.Caching.Redis;

namespace Dapper.Extensions.Caching.Redis
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperCachingForRedis(this ContainerBuilder service, RedisConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInRedis<TCacheKeyBuilder>(this ContainerBuilder service, RedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInPartitionRedis(this ContainerBuilder service, PartitionRedisConfiguration config)
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Connections.Count() < 2)
                throw new ArgumentException("Need at least 2 redis nodes.", nameof(config.Connections));
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            RedisHelper.Initialization(config.PartitionPolicy != null
                ? new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()),
                    config.Connections.ToArray())
                : new CSRedisClient(null, config.Connections.ToArray()));
            service.RegisterInstance(new CSRedisCache(RedisHelper.Instance)).As<IDistributedCache>().SingleInstance();
            service.RegisterType<PartitionRedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInPartitionRedis<TCacheKeyBuilder>(this ContainerBuilder service, PartitionRedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            if (config == null)
                throw new ArgumentNullException(nameof(config));
            if (config.Connections.Count() < 2)
                throw new ArgumentException("Need at least 2 redis nodes.", nameof(config.Connections));
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                AllMethodsEnableCache = config.AllMethodsEnableCache,
                Expire = config.Expire
            }).SingleInstance();
            RedisHelper.Initialization(config.PartitionPolicy != null
                ? new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()),
                    config.Connections.ToArray())
                : new CSRedisClient(null, config.Connections.ToArray()));
            service.RegisterInstance(new CSRedisCache(RedisHelper.Instance)).As<IDistributedCache>().SingleInstance();
            service.RegisterType<PartitionRedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }
    }
}
