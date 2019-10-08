using System;
using System.Data;
using System.Data.SQLite;

namespace Dapper.Extensions.SQLite
{
    public class SQLiteDapper : BaseDapper
    {

        public SQLiteDapper(IServiceProvider service, string connectionName = "DefaultConnection") : base(service, connectionName)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            return GetConnection(connectionName, SQLiteFactory.Instance);
        }
    }
}
