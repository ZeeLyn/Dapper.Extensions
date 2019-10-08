using System;
using System.Data;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.MySql
{
    public class MySqlDapper : BaseDapper
    {
        public MySqlDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }
        protected override IDbConnection CreateConnection(string connectionName)
        {
            return GetConnection(connectionName, MySqlClientFactory.Instance);
        }
    }
}