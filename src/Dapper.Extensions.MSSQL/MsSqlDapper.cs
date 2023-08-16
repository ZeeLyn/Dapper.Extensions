using System;
using System.Data.SqlClient;

namespace Dapper.Extensions.MSSQL
{
    public class MsSqlDapper : BaseDapper<SqlConnection>
    {
        public MsSqlDapper(IServiceProvider service, string connectionName = "DefaultConnection",
            bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave,
            readOnly)
        {
        }
    }
}