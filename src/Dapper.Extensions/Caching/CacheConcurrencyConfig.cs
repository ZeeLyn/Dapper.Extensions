using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Caching
{
    public class CacheConcurrencyConfig
    {
        public int MaxConcurrent { get; set; }

        public int AcquireLockTimeout { get; set; }
    }
}