using System;

namespace Dapper.Extensions.Caching
{
    public class CacheOptions
    {
        public bool Enable { get; set; }

        /// <summary>
        /// Cache key
        /// </summary>
        public string Key { get; set; }

        public TimeSpan? Expire { get; set; }
    }
}
