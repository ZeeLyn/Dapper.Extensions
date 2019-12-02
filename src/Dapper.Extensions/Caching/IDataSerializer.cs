namespace Dapper.Extensions.Caching
{
    public interface IDataSerializer
    {
        string Serialize(object data);

        T Deserialize<T>(string value);
    }
}
