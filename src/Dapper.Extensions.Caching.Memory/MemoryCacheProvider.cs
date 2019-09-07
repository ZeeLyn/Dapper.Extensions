using System;
using Microsoft.Extensions.Caching.Memory;

namespace Dapper.Extensions.Caching.Memory
{
    public class MemoryCacheProvider : ICacheProvider
    {
        private IMemoryCache Cache { get; }

        public MemoryCacheProvider(IMemoryCache cache)
        {
            Cache = cache;
        }

        public CacheValue<TResult> TryGet<TResult>(string key)
        {
            if (Cache.TryGetValue(key, out var val))
                return new CacheValue<TResult>((TResult)val);
            return new CacheValue<TResult>(default, false);
        }

        public bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null)
        {
            if (expired.HasValue)
                Cache.Set(key, result, expired.Value);
            else
                Cache.Set(key, result);
            return true;
        }
    }
}
