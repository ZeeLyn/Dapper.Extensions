using System;

namespace Dapper.Extensions.Caching.Redis
{
    public class RedisCacheProvider : ICacheProvider
    {
        private IDataSerializer Serializer { get; }

        public RedisCacheProvider(IDataSerializer serializer)
        {
            Serializer = serializer;
        }

        public bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null)
        {
            return RedisHelper.Set(key, Serializer.Serialize(new CacheValue<TResult>(result)), expired.HasValue ? (int)expired.Value.TotalSeconds : -1);
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            var val = RedisHelper.Get(key);
            if (string.IsNullOrWhiteSpace(val))
                return new CacheValue<TResult>(default, false);
            return Serializer.Deserialize<CacheValue<TResult>>(val);
        }
    }
}
