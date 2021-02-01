using System;
using FreeRedis;

namespace Dapper.Extensions.Caching.Redis
{
    public class RedisCacheProvider : ICacheProvider
    {
        private IDataSerializer Serializer { get; }

        private RedisClient Client { get; }

        public RedisCacheProvider(IDataSerializer serializer, RedisClient client)
        {
            Serializer = serializer;
            Client = client;
        }

        public bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null)
        {
            Client.Set(key, new CacheValue<TResult>(result), expired.HasValue ? (int)expired.Value.TotalSeconds : 0);
            return true;
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            var val = Client.Get(key);
            if (string.IsNullOrWhiteSpace(val))
                return new CacheValue<TResult>(default, false);
            return Serializer.Deserialize<CacheValue<TResult>>(val);
        }
    }
}
