using System;
using System.Data;
using System.Data.Odbc;
using Microsoft.Extensions.Configuration;

namespace Dapper.Extensions.Odbc
{
	public class OdbcDapper : DbDapper
	{
		public OdbcDapper(string connectionName) : base(connectionName)
		{
		}

		protected override IDbConnection CreateConnection(string connectionName)
		{
			var connString = Configuration.GetConnectionString(connectionName);
			if (string.IsNullOrWhiteSpace(connString))
				throw new ArgumentNullException(nameof(connString), "The config of " + connectionName + " cannot be null.");
			IDbConnection conn = OdbcFactory.Instance.CreateConnection();
			if (conn == null)
				throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
			conn.ConnectionString = connString;
			conn.Open();
			return conn;
		}
	}
}
