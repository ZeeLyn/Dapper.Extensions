using System;
using System.Data.Odbc;

namespace Dapper.Extensions.Odbc
{
    public class OdbcDapper : BaseDapper<OdbcConnection>
    {
        public OdbcDapper(IServiceProvider service, string connectionName = "DefaultConnection",
            bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave,
            readOnly)
        {
        }
    }
}