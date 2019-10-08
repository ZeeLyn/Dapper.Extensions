using System;
using System.Data;
using System.Data.SQLite;

namespace Dapper.Extensions.SQLite
{
    public class SQLiteDapper : BaseDapper
    {

        public SQLiteDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = GetConnectionString(connectionName);
            var conn = SQLiteFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return PackMiniProfilerConnection(conn);
        }
    }
}
