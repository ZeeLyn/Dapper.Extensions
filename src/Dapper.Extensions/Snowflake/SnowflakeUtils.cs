using Dapper.Extensions.Snowflake;
using System;


namespace Dapper.Extensions
{
    public class SnowflakeUtils
    {
        private static IdWorker Worker { get; set; }

        public static void Initialize(long workerId, long datacenterId, long sequence = 0L)
        {
            Worker = new IdWorker(workerId, datacenterId, sequence);
        }

        public static long GenerateId()
        {
            if (Worker == null)
                throw new InvalidOperationException("Please call the 'Initialize' method to initialize.");
            return Worker.NextId();
        }
    }
}
