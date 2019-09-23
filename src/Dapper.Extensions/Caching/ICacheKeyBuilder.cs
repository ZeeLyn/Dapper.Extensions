namespace Dapper.Extensions.Caching
{
    public interface ICacheKeyBuilder
    {
        string Generate(string sql, object param, string customKey, int? pageIndex = default, int? pageSize = default);
    }
}
