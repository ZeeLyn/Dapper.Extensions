using System.Collections.Generic;
using AppSettings.Loader;

namespace Dapper.Extensions
{
	public class DbConnectionConfig : AppSettingLoader<DbConnectionConfig>
	{
		public List<ConnectionConfig> DbConnections { get; set; }
	}
	public class ConnectionConfig
	{
		/// <summary>
		/// 连接名称
		/// </summary>
		public string Name { get; set; }

		/// <summary>
		/// 连接字符串
		/// </summary>
		public string ConnectionString { get; set; }
	}
}
