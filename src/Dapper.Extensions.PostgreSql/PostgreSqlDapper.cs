using System;
using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;

namespace Dapper.Extensions.PostgreSql
{
    public class PostgreSqlDapper : DbDapper
    {
        public PostgreSqlDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            IDbConnection conn = NpgsqlFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return conn;
        }
    }
}
