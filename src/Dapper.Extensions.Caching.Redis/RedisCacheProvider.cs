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
            if (expired.HasValue && expired.Value < TimeSpan.FromMilliseconds(1))
                throw new ArgumentOutOfRangeException(nameof(expired), "Expiration must be at least one millisecond, or null for no expiration.");
            Client.Set(key, Serializer.Serialize(new CacheValue<TResult>(result)), expired ?? TimeSpan.Zero);
            return true;
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            var val = Client.Get(key);
            if (string.IsNullOrWhiteSpace(val))
                return new CacheValue<TResult>(default, false);
            return Serializer.Deserialize<CacheValue<TResult>>(val);
        }

        public void Remove(params string[] keys)
        {
            Client.Del(keys);
        }
    }
}
