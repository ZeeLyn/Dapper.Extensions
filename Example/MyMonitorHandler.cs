using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions.Monitor;

namespace Example
{
    public class MyMonitorHandler : IMonitorHandler
    {
        public async Task OnSlowSqlCommandAsync(string methodName, string sqlOrSqlName, object param, long duration)
        {
            throw new Exception("error");
            Console.WriteLine("#######################");
            await Task.Delay(5 * 1000);
            Console.WriteLine("@@@@@@@@@@@@@@@@@@@@@@@");
        }
    }
}
