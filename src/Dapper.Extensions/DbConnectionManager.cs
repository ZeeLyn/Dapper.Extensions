using System;
using System.Linq;

namespace Dapper.Extensions
{
	public class DbConnectionManager
	{
		public static string GetConnectionString(string connectionName)
		{
			if (string.IsNullOrWhiteSpace(connectionName))
				throw new ArgumentNullException(nameof(connectionName), "The connectionName cannot be null.");
			var connconfig = DbConnectionConfig.Get.DbConnections.FirstOrDefault(p => string.Equals(p.Name, connectionName, StringComparison.CurrentCultureIgnoreCase));
			var connstring = connconfig?.ConnectionString;
			if (string.IsNullOrWhiteSpace(connstring))
				throw new ArgumentNullException(nameof(connconfig), "The config of " + connectionName + " annot be null.");
			return connstring;
		}
	}
}
