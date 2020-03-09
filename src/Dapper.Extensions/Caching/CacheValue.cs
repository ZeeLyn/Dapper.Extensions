using System;
using Newtonsoft.Json;

namespace Dapper.Extensions.Caching
{
    [Serializable]
    public class CacheValue<TValue>
    {
        public CacheValue()
        {
        }
        public CacheValue(TValue value)
        {
            Value = value;
        }

        public CacheValue(TValue value, bool hasKey)
        {
            Value = value;
            ExistKey = hasKey;
        }


        public TValue Value { get; set; }

        [JsonIgnore]
        public bool ExistKey { get; set; } = true;

    }
}
