using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.Extensions.Hosting;

namespace Dapper.Extensions.MiniProfiler
{
    public class MiniProfilerHostedService : IHostedService
    {
        public async Task StartAsync(CancellationToken cancellationToken)
        {
            StackExchange.Profiling.MiniProfiler.StartNew("Test");
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            await StackExchange.Profiling.MiniProfiler.Current.StopAsync();
        }
    }
}
