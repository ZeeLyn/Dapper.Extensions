using Oracle.ManagedDataAccess.Client;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace Dapper.Extensions.Oracle
{
    public class OracleDapper : BaseDapper<OracleConnection>
    {
        public OracleDapper(IServiceProvider service, string connectionName = "DefaultConnection",
            bool enableMasterSlave = false, bool readOnly = false) : base(service, connectionName, enableMasterSlave,
            readOnly)
        {
        }
    }
}