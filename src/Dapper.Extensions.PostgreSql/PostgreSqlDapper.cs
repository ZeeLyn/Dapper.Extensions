using System;
using System.Data;
using Npgsql;

namespace Dapper.Extensions.PostgreSql
{
    public class PostgreSqlDapper : BaseDapper
    {
        public PostgreSqlDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = GetConnectionString(connectionName);
            var conn = NpgsqlFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return PackMiniProfilerConnection(conn);
        }
    }
}
