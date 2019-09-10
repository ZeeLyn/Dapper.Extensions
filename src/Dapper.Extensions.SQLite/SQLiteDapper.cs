using System;
using System.Data;
using System.Data.SQLite;
using Microsoft.Extensions.Configuration;

namespace Dapper.Extensions.SQLite
{
    public class SQLiteDapper : DbDapper
    {
        public SQLiteDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            IDbConnection conn = SQLiteFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return conn;
        }
    }
}
