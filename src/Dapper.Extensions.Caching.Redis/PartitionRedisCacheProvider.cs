
using System;

namespace Dapper.Extensions.Caching.Redis
{
    public class PartitionRedisCacheProvider : ICacheProvider
    {
        private R Cache { get; }
        private IDataSerializer Serializer { get; }
        public PartitionRedisCacheProvider(IDistributedCache cache, IDataSerializer serializer)
        {
            Cache = cache;
            Serializer = serializer;
        }

        public bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null)
        {
            if (expired.HasValue)
                Cache.SetString(key, Serializer.Serialize(new CacheValue<TResult>(result)), new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expired.Value });
            else
                Cache.SetString(key, Serializer.Serialize(new CacheValue<TResult>(result)));
            return true;
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            var val = Cache.GetString(key);
            if (string.IsNullOrWhiteSpace(val))
                return new CacheValue<TResult>(default, false);
            return Serializer.Deserialize<CacheValue<TResult>>(val);
        }
    }
}
