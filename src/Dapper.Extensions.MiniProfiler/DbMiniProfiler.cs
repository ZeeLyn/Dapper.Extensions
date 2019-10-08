using System.Data;
using System.Data.Common;

namespace Dapper.Extensions.MiniProfiler
{
    public class DbMiniProfiler : IDbMiniProfiler
    {
        public IDbConnection CreateConnection(DbConnection connection)
        {
            return new StackExchange.Profiling.Data.ProfiledDbConnection(connection, StackExchange.Profiling.MiniProfiler.Current);
        }
    }
}
