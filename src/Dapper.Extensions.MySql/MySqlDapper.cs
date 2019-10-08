using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.MySql
{
    public class MySqlDapper : BaseDapper
    {
        public MySqlDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }
        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = GetConnectionString(connectionName);
            var conn = MySqlClientFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return PackMiniProfilerConnection(conn);
        }
    }
}