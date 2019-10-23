using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.MySql
{
    public class MySqlDapper : BaseDapper
    {
        public MySqlDapper(IServiceProvider service, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave, readOnly)
        {
        }
        protected override IDbConnection CreateConnection(string connectionName)
        {
            return GetConnection(connectionName, MySqlClientFactory.Instance);
        }
    }
}