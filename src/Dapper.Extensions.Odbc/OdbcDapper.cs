using System;
using System.Data;
using System.Data.Odbc;

namespace Dapper.Extensions.Odbc
{
	public class OdbcDapper : DbDapper
	{
		public OdbcDapper(string connectionName) : base(connectionName)
		{
		}

		protected override IDbConnection CreateConnection(string connectionName)
		{
			var connstring = DbConnectionManager.GetConnectionString(connectionName);
			IDbConnection conn = OdbcFactory.Instance.CreateConnection();
			if (conn == null)
				throw new ArgumentNullException(nameof(IDbConnection), "Failed to get database connection object");
			conn.ConnectionString = connstring;
			conn.Open();
			return conn;
		}
	}
}
