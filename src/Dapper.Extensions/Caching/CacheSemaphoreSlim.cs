using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Dapper.Extensions.Caching
{
    public class CacheSemaphoreSlim
    {
        public SemaphoreSlim SemaphoreSlim { get; }

        public CacheSemaphoreSlim(int concurrency)
        {
            SemaphoreSlim = new SemaphoreSlim(concurrency);
        }
    }
}