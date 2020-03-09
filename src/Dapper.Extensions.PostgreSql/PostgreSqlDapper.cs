using System;
using Npgsql;

namespace Dapper.Extensions.PostgreSql
{
    public class PostgreSqlDapper : BaseDapper<NpgsqlConnection>
    {
        public PostgreSqlDapper(IServiceProvider service, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave, readOnly)
        {
        }
    }
}
