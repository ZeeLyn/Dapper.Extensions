using System;
using System.Data;
using System.Data.SqlClient;
using Microsoft.Extensions.Configuration;

namespace Dapper.Extensions
{
    public class MsSqlDapper : DbDapper
    {

        public MsSqlDapper(IConfiguration configuration, IServiceProvider service, string connectionName = "DefaultConnection") : base(configuration, service, connectionName)
        {
        }
        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            IDbConnection conn = SqlClientFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return conn;
        }
    }
}
