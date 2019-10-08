using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

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
