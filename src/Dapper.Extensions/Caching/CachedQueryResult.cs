using Newtonsoft.Json;

namespace Dapper.Extensions.Caching
{
    // 自定义键仍是一个可删除的缓存槽，读取时验证结果类型及查询语义。
    internal sealed class CachedQueryResult
    {
        public string Identity { get; set; }
        [JsonIgnore]
        public object Value { get; set; }
        private string json;
        public string Json
        {
            get => json ??= JsonConvert.SerializeObject(Value);
            set => json = value;
        }

        public T Read<T>() => Value is T value ? value : JsonConvert.DeserializeObject<T>(Json);
    }
}
