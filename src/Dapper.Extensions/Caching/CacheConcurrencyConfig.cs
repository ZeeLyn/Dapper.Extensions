using System;
using System.Collections.Generic;
using System.Text;

namespace Dapper.Extensions.Caching
{
    public class CacheConcurrencyConfig
    {
        /// <summary>保留用于兼容旧配置；缓存查询不再使用全局并发上限。</summary>
        public int MaxConcurrent { get; set; }

        /// <summary>等待同一缓存键的锁超时秒数；0 表示不等待。</summary>
        public int AcquireLockTimeout { get; set; }
    }
}
