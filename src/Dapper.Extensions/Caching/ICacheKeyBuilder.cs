namespace Dapper.Extensions.Caching
{
    public interface ICacheKeyBuilder
    {
        string Generate(string sql, object param, bool shotKey = true, int? pageIndex = default, int? pageSize = default);
    }
}
