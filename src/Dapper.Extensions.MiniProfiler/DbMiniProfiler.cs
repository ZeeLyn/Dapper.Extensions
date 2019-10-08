using System.Data;
using System.Data.Common;
using StackExchange.Profiling.Data;

namespace Dapper.Extensions.MiniProfiler
{
    public class DbMiniProfiler : IDbMiniProfiler
    {
        public IDbConnection CreateConnection(DbConnection connection)
        {
            return new ProfiledDbConnection(connection, StackExchange.Profiling.MiniProfiler.Current);
        }
    }
}
