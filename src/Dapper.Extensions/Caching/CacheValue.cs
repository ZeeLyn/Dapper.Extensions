namespace Dapper.Extensions.Caching
{
    public class CacheValue<TValue>
    {
        public CacheValue(TValue value)
        {
            Value = value;
        }

        public TValue Value { get; set; }
    }
}
