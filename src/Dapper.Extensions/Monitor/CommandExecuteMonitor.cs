using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;

namespace Dapper.Extensions.Monitor
{
    public sealed partial class DapperProxy
    {
        private TResult SyncCommandExecuteMonitor<TResult>(string methodName, string sql, object param, Func<TResult> func)
        {
            var sw = new Stopwatch();
            sw.Start();
            try
            {
                return func();
            }
            catch (Exception e)
            {
                throw;
            }
            finally
            {
                sw.Stop();
                if (sw.ElapsedMilliseconds > 200)
                    SlowCommandNotification(methodName, sql, param);
            }
        }

        private void SyncCommandExecuteMonitor(Action action)
        {
            action();
        }

        private void SlowCommandNotification(string methodName, string sql, object param)
        {

        }
    }
}
