using System;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.MySql
{
    public class MySqlDapper : DbDapper
    {
        public MySqlDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }
        protected override IDbConnection CreateConnection(string connectionName)
        {
            var connString = Configuration.GetConnectionString(connectionName);
            if (string.IsNullOrWhiteSpace(connString))
                throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
            IDbConnection conn = MySqlClientFactory.Instance.CreateConnection();
            if (conn == null)
                throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
            conn.ConnectionString = connString;
            conn.Open();
            return conn;
        }
    }
}