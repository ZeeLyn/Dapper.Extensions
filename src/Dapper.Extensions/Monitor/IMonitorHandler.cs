using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace Dapper.Extensions.Monitor
{
    public interface IMonitorHandler
    {
        Task OnSlowSqlCommandAsync(string methodName, string sqlOrSqlName, object param, long duration);
    }
}
