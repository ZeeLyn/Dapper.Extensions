using System;

namespace Dapper.Extensions.Caching
{
    public class CacheConfiguration
    {
        /// <summary>
        /// If this option is enabled, all query methods will enable caching, the default is enabled.
        /// </summary>
        public bool AllMethodsEnableCache { get; set; } = true;

        public TimeSpan? Expire { get; set; }

        public string KeyPrefix { get; set; } = "dapper_cache";
    }
}
