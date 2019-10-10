using System.Data;
using System.Data.Common;

namespace Dapper.Extensions.MiniProfiler
{
    public interface IDbMiniProfiler
    {
        IDbConnection CreateConnection(DbConnection connection);
    }
}
