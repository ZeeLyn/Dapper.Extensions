using Newtonsoft.Json;

namespace Dapper.Extensions.Caching.Redis
{
    public class DataSerializer : IDataSerializer
    {
        public string Serialize(object data)
        {
            return JsonConvert.SerializeObject(data);
        }

        public T Deserialize<T>(string value)
        {
            return JsonConvert.DeserializeObject<T>(value);
        }
    }
}
