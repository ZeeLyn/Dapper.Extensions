using System;
using System.Data;
using System.Data.SQLite;

namespace Dapper.Extensions.SQLite
{
    public class SQLiteDapper : BaseDapper
    {

        public SQLiteDapper(IServiceProvider service, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave, readOnly)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            return GetConnection(connectionName, SQLiteFactory.Instance);
        }
    }
}
