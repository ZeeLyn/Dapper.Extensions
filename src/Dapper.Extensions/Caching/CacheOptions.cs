using System;

namespace Dapper.Extensions.Caching
{
    public class CacheOptions
    {
        public CacheOptions(string key = "", TimeSpan? expire = null)
        {
            Key = key;
            Expire = expire;
        }

        /// <summary>
        /// Cache key
        /// </summary>
        public string Key { get; set; }

        public TimeSpan? Expire { get; set; }
    }
}
