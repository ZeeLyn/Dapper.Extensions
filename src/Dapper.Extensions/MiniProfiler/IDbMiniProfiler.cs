using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Text;

namespace Dapper.Extensions.MiniProfiler
{
    public interface IDbMiniProfiler
    {
        IDbConnection CreateConnection(DbConnection connection);
    }
}
