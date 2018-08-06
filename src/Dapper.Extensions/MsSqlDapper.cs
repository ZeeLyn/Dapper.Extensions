using System;
using System.Data;
using System.Data.SqlClient;

namespace Dapper.Extensions
{
	public class MsSqlDapper : DbDapper
	{

		public MsSqlDapper(string connectionName) : base(connectionName)
		{
		}
		protected override IDbConnection CreateConnection(string connectionName)
		{
			var connstring = DbConnectionManager.GetConnectionString(connectionName);
			IDbConnection conn = SqlClientFactory.Instance.CreateConnection();
			if (conn == null)
				throw new ArgumentNullException(nameof(IDbConnection), "获取数据库连接对象失败");
			conn.ConnectionString = connstring;
			conn.Open();
			return conn;
		}
	}
}
