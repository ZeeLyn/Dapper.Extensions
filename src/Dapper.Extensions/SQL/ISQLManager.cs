namespace Dapper.Extensions.SQL
{
    public interface ISQLManager
    {
        string GetSQL(string name);

        (string CountSQL, string QuerySQL) GetPagingSQL(string name);
    }
}
