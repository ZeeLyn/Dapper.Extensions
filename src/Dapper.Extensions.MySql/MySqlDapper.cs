using System;
using MySql.Data.MySqlClient;

namespace Dapper.Extensions.MySql
{
    public class MySqlDapper : BaseDapper<MySqlConnection>
    {
        public MySqlDapper(IServiceProvider service, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave, readOnly)
        {
        }
    }
}