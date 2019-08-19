using System;

namespace Dapper.Extensions.Caching.Redis
{
    public class RedisConfiguration : CacheConfiguration
    {
        public string ConnectionString { get; set; }
    }

    public class PartitionRedisConfiguration : CacheConfiguration
    {
        public string[] Connections { get; set; }

        public Func<string, string[], string> PartitionPolicy { get; set; }
    }
}
