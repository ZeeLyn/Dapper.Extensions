using System;
using System.Data;
using Npgsql;

namespace Dapper.Extensions.PostgreSql
{
    public class PostgreSqlDapper : BaseDapper
    {
        public PostgreSqlDapper(IServiceProvider service, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave, readOnly)
        {
        }

        protected override IDbConnection CreateConnection(string connectionName)
        {
            return GetConnection(connectionName, NpgsqlFactory.Instance);
        }
    }
}
