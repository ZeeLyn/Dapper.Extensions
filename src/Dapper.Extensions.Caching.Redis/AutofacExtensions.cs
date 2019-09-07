using System.Linq;
using Autofac;
using CSRedis;

namespace Dapper.Extensions.Caching.Redis
{
    public static class AutofacExtensions
    {
        public static ContainerBuilder AddDapperCachingForRedis(this ContainerBuilder service, RedisConfiguration config)
        {
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                Enable = config.Enable,
                Expire = config.Expire
            }).SingleInstance();
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInRedis<TCacheKeyBuilder>(this ContainerBuilder service, RedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(new CacheConfiguration
            {
                Enable = config.Enable,
                Expire = config.Expire
            }).SingleInstance();
            RedisHelper.Initialization(new CSRedisClient(config.ConnectionString));
            service.RegisterType<RedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInPartitionRedis(this ContainerBuilder service, PartitionRedisConfiguration config)
        {
            service.RegisterType<DefaultCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(config).SingleInstance();
            RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()), config.Connections.ToArray()));
            service.RegisterType<PartitionRedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }

        public static ContainerBuilder AddDapperCachingInPartitionRedis<TCacheKeyBuilder>(this ContainerBuilder service, PartitionRedisConfiguration config) where TCacheKeyBuilder : ICacheKeyBuilder
        {
            service.RegisterType<TCacheKeyBuilder>().As<ICacheKeyBuilder>().SingleInstance();
            service.RegisterInstance(config).SingleInstance();
            RedisHelper.Initialization(new CSRedisClient(key => config.PartitionPolicy(key, config.Connections.ToArray()), config.Connections.ToArray()));
            service.RegisterType<PartitionRedisCacheProvider>().As<ICacheProvider>().SingleInstance();
            service.RegisterType<DataSerializer>().As<IDataSerializer>().SingleInstance();
            return service;
        }
    }
}
