using System;
using System.Linq;
using Autofac;
using FreeRedis;

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
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            }).SingleInstance();

            service.RegisterInstance(new RedisClient(config.ConnectionString)).SingleInstance();
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
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            }).SingleInstance();
            service.RegisterInstance(new RedisClient(config.ConnectionString)).SingleInstance();
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
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            }).SingleInstance();

            service.RegisterInstance(config.PartitionPolicy != null
                ? new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), config.PartitionPolicy)
                : new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), null)).SingleInstance();

            service.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
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
                Expire = config.Expire,
                KeyPrefix = config.KeyPrefix
            }).SingleInstance();
            service.RegisterInstance(config.PartitionPolicy != null
                ? new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), config.PartitionPolicy)
                : new RedisClient(config.Connections.Select(ConnectionStringBuilder.Parse).ToArray(), null)).SingleInstance();
            service.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }
    }
}
