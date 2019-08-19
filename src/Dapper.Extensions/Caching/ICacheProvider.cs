using System;

namespace Dapper.Extensions.Caching
{
    public interface ICacheProvider
    {
        bool TrySet<TResult>(string key, TResult result, TimeSpan? expired = null);

        CacheValue<TResult> TryGet<TResult>(string key);
    }
}
