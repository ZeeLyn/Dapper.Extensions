using Microsoft.Extensions.Caching.Distributed;
using System;

namespace Dapper.Extensions.Caching.Redis
{
    public class PartitionRedisCacheProvider : ICacheProvider
    {
        private IDistributedCache Cache { get; }
        public PartitionRedisCacheProvider(IDistributedCache cache)
        {
            Cache = cache;
        }

        public bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null)
        {
            try
            {
                if (expired.HasValue)
                    Cache.SetObject(key, result, new DistributedCacheEntryOptions { AbsoluteExpirationRelativeToNow = expired.Value });
                else
                    Cache.SetObject(key, result);
                return true;
            }
            catch
            {
                return false;
            }
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            return Cache.GetObject<CacheValue<TResult>>(key);
        }
    }
}
