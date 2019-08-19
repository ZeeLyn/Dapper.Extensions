using System;

namespace Dapper.Extensions.Caching
{
    public class CacheConfiguration
    {
        /// <summary>
        /// If this option is enabled, all query methods will enable caching.
        /// </summary>
        public bool EnableGlobalCache { get; set; }

        public TimeSpan? Expire { get; set; }
    }
}
