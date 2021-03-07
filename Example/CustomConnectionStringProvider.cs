using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions;

namespace Example
{
    public class CustomConnectionStringProvider:IConnectionStringProvider
    {
        public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
        {
            throw new NotImplementedException();
        }
    }
}
