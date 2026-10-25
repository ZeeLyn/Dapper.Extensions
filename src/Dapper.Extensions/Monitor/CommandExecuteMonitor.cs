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
                    Task.Run(() => SlowCommandNotificationAsync(methodName, sql, param, sw.ElapsedMilliseconds)).GetAwaiter().GetResult();
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
                    await SlowCommandNotificationAsync(methodName, sql, param, sw.ElapsedMilliseconds).ConfigureAwait(false);
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
                    Task.Run(() => SlowCommandNotificationAsync(methodName, sql, param, sw.ElapsedMilliseconds)).GetAwaiter().GetResult();
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
                    await SlowCommandNotificationAsync(methodName, sql, param, sw.ElapsedMilliseconds).ConfigureAwait(false);
                }
            }
        }

        private async Task SlowCommandNotificationAsync(string methodName, string sql, object param, long duration)
        {
            try
            {
                if (!MonitorConfiguration.EnableLog && !MonitorConfiguration.HasCustomMonitorHandler)
                    return;
                using var scope = Service.CreateScope();
                if (MonitorConfiguration.EnableLog)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<DapperProxy>>();
                    logger.LogWarning("Slow SQL command: Method:{Method} SQL/SQLName:{Sql} Param:{Param} Duration:{Duration}ms",
                        methodName, sql, JsonConvert.SerializeObject(param), duration);
                }
                if (MonitorConfiguration.HasCustomMonitorHandler)
                {
                    var handler = (IMonitorHandler)scope.ServiceProvider.GetRequiredService(
                        MonitorConfiguration.HandlerType ?? typeof(IMonitorHandler));
                    await handler.OnSlowSqlCommandAsync(methodName, sql, param, duration).ConfigureAwait(false);
                }
            }
            catch (Exception exception)
            {
                // 监控及其日志故障不能覆盖业务执行结果或原始数据库异常。
                try { Service.GetService<ILogger<DapperProxy>>()?.LogError(exception, "Slow SQL monitoring failed."); }
                catch { }
            }
        }
    }
}
