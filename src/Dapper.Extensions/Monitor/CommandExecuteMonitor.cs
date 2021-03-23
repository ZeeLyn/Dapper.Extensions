using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace Dapper.Extensions.Monitor
{
    public sealed partial class DapperProxy
    {
        private TResult SyncCommandExecuteMonitor<TResult>(string methodName, string sql, object param, Func<TResult> func)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return func();
            }
            finally
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > MonitorConfiguration.SlowCriticalValue)
                {
                    SlowCommandNotification(methodName, sql, param, sw.ElapsedMilliseconds);
                }
            }
        }

        private async Task<TResult> AsyncCommandExecuteMonitor<TResult>(string methodName, string sql, object param, Func<Task<TResult>> func)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                return await func();
            }
            finally
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > MonitorConfiguration.SlowCriticalValue)
                {
                    SlowCommandNotification(methodName, sql, param, sw.ElapsedMilliseconds);
                }
            }
        }


        private void SyncCommandExecuteMonitor(string methodName, string sql, object param, Action action)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                action();
            }
            finally
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > MonitorConfiguration.SlowCriticalValue)
                {
                    SlowCommandNotification(methodName, sql, param, sw.ElapsedMilliseconds);
                }
            }
        }

        private async Task AsyncCommandExecuteMonitor(string methodName, string sql, object param, Func<Task> action)
        {
            var sw = Stopwatch.StartNew();
            try
            {
                await action();
            }
            finally
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > MonitorConfiguration.SlowCriticalValue)
                {
                    SlowCommandNotification(methodName, sql, param, sw.ElapsedMilliseconds);
                }
            }
        }

        private void SlowCommandNotification(string methodName, string sql, object param, long duration)
        {
            if (MonitorConfiguration.EnableLog || MonitorConfiguration.HasCustomMonitorHandler)
            {
                using var scope = Service.CreateScope();
                if (MonitorConfiguration.EnableLog)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DapperProxy>>();
                    logger.LogWarning("Slow SQL command  ---->\r\nMethod:{0}\r\nSQL/SQLName:{1}\r\nParam:{2}\r\nDuration:{3}ms", methodName, sql, JsonConvert.SerializeObject(param), duration);
                }

                if (MonitorConfiguration.HasCustomMonitorHandler)
                {
                    var handler = scope.ServiceProvider.GetRequiredService<IMonitorHandler>();
                    handler.OnSlowSqlCommandAsync(methodName, sql, param, duration).ConfigureAwait(false);
                }
            }

        }
    }
}
