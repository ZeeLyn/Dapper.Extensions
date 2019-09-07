using Newtonsoft.Json;

namespace Dapper.Extensions.Caching
{
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
            HasKey = hasKey;
        }


        public TValue Value { get; set; }

        [JsonIgnore]
        public bool HasKey { get; set; } = true;

    }
}
