using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Dapper.Extensions.SQL;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace XUnitTest
{
    public class SqlReloadRegressionTests
    {
        [Fact]
        public void SqlNameDynamicPagingForwardsBusinessParameters()
        {
            var directory = Path.Combine(Path.GetTempPath(), "dapper-sql-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            try
            {
                File.WriteAllText(Path.Combine(directory, "README.txt"), "Not an XML file");
                File.WriteAllText(Path.Combine(directory, "queries.xml"), "<sql-set><sql name=\"page\">select @tenant as value limit @Take offset @Skip</sql></sql-set>");
                using var env = new RegressionEnvironment(s => Dapper.Extensions.DependencyInjectionExtensions.AddSQLSeparationForDapper(s, directory));
                Assert.Equal(42L, (long)env.Db.QueryPlainPage(new SQLName("page"), 1, 10, new { tenant = 42 })[0].value);
            }
            finally { Directory.Delete(directory, true); }
        }

        [Fact]
        public async Task ReloadPublishesCompleteSnapshotAndRetainsOldSnapshotOnInvalidXml()
        {
            var directory = Path.Combine(Path.GetTempPath(), "dapper-reload-" + Guid.NewGuid().ToString("N"));
            Directory.CreateDirectory(directory);
            var path = Path.Combine(directory, "queries.xml");
            string Xml(int version) => "<sql-set>" + string.Concat(Enumerable.Range(0, 400).Select(i => $"<sql name=\"q{i}\">select {version}</sql>")) + "</sql-set>";
            var logger = new ReloadLogger();
            try
            {
                File.WriteAllText(path, Xml(1));
                var manager = new SQLManager(new SQLSeparateConfigure { RootDir = directory }, logger);
                using var owner = (IDisposable)manager;
                using var stop = new CancellationTokenSource();
                var reader = Task.Run(() =>
                {
                    while (!stop.IsCancellationRequested)
                    {
                        for (var i = 0; i < 400; i++) Assert.StartsWith("select ", manager.GetSQL("q" + i));
                        Thread.Yield();
                    }
                });
                try
                {
                    File.WriteAllText(path, Xml(2));
                    await WaitUntil(() => manager.GetSQL("q399") == "select 2");
                    File.WriteAllText(path, "<invalid");
                    await logger.Error.Task.WaitAsync(TimeSpan.FromSeconds(10));
                    Assert.Equal("select 2", manager.GetSQL("q399"));
                    File.WriteAllText(Path.Combine(directory, "notes.txt"), "ignored on reload too");
                    File.WriteAllText(path, Xml(3));
                    await WaitUntil(() => manager.GetSQL("q399") == "select 3");
                }
                finally { stop.Cancel(); await reader.WaitAsync(TimeSpan.FromSeconds(5)); }
            }
            finally { Directory.Delete(directory, true); }
        }

        private static async Task WaitUntil(Func<bool> predicate)
        {
            using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            while (!predicate()) await Task.Delay(25, timeout.Token);
        }

        private sealed class ReloadLogger : ILogger<SQLManager>
        {
            public readonly TaskCompletionSource<bool> Error = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel level) => true;
            public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
            { if (level >= LogLevel.Error) Error.TrySetResult(true); }
        }
    }
}
