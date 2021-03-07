using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions
{
    public interface IConnectionStringProvider
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="connectionName"></param>
        /// <param name="enableMasterSlave"></param>
        /// <param name="readOnly"></param>
        /// <returns></returns>
        string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false);
    }
}
