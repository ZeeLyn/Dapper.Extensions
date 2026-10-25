using System;
using System.Data;
using System.Data.Common;
using System.Data.SQLite;
using System.Threading;
using System.Threading.Tasks;
using Dapper;
using Dapper.Extensions;
using Dapper.Extensions.Caching;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace XUnitTest
{
    public class CancellationRegressionTests
    {
        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        public async Task GenericCommandOverloadsCancelWhileCacheGateIsBusy(int shape)
        {
            using var env = new RegressionEnvironment();
            using var cancellation = new CancellationTokenSource();
            using var holder = new BlockingDapper(env.Services);
            var started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            var loading = holder.Hold(started, release);
            await started.Task.WaitAsync(TimeSpan.FromSeconds(2));
            try
            {
                var command = new CommandDefinition("select 1", cancellationToken: cancellation.Token);
                Task pending = shape switch
                {
                    0 => env.Db.QueryAsync<int>(command, cacheKey: "busy"),
                    1 => env.Db.QueryFirstAsync<int>(command, cacheKey: "busy"),
                    2 => env.Db.QueryFirstOrDefaultAsync<int>(command, cacheKey: "busy"),
                    3 => env.Db.QuerySingleAsync<int>(command, cacheKey: "busy"),
                    _ => env.Db.QuerySingleOrDefaultAsync<int>(command, cacheKey: "busy")
                };
                cancellation.Cancel();
                await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(2)));
            }
            finally { release.TrySetResult(true); await loading; }
        }

        private sealed class BlockingDapper : SQLiteDapper
        {
            public BlockingDapper(IServiceProvider services) : base(services) { }
            public Task<int> Hold(TaskCompletionSource<bool> started, TaskCompletionSource<bool> release)
                => CommandExecuteAsync(true, async () =>
                {
                    started.SetResult(true);
                    await release.Task;
                    return 1;
                }, "select 1", null, "busy", null, false);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        [InlineData(3)]
        [InlineData(4)]
        [InlineData(5)]
        [InlineData(6)]
        [InlineData(7)]
        [InlineData(8)]
        [InlineData(9)]
        [InlineData(10)]
        [InlineData(11)]
        public async Task StringQueriesForwardLiveCancellationToDatabase(int shape)
        {
            using var env = new RegressionEnvironment();
            using var db = new ProbeDapper(env.Services);
            var connection = (ProbeConnection)db.Conn.Value;
            using var cancellation = new CancellationTokenSource();
            var token = cancellation.Token;
            Task pending = shape switch
            {
                0 => db.QueryAsync<int>("select 1", enableCache: false, cancellationToken: token),
                1 => db.QueryAsync("select 1", enableCache: false, cancellationToken: token),
                2 => db.QueryFirstOrDefaultAsync<int>("select 1", enableCache: false, cancellationToken: token),
                3 => db.QuerySingleOrDefaultAsync<int>("select 1", enableCache: false, cancellationToken: token),
                4 => db.QueryFirstOrDefaultAsync("select 1", enableCache: false, cancellationToken: token),
                5 => db.QuerySingleOrDefaultAsync("select 1", enableCache: false, cancellationToken: token),
                6 => db.QueryAsync<int, int, int>("select 1,2", (a, b) => a + b, enableCache: false, cancellationToken: token),
                7 => db.QueryMultipleAsync<int, int>("select 1;select 2", enableCache: false, cancellationToken: token),
                8 => db.QueryPlainPageAsync<int>("select 1", 1, 10, enableCache: false, cancellationToken: token),
                9 => db.QueryPlainPageAsync("select 1", 1, 10, enableCache: false, cancellationToken: token),
                10 => db.QueryPageAsync<int>("select 1", "select 1", 1, 10, enableCache: false, cancellationToken: token),
                _ => db.QueryPageAsync("select 1", "select 1", 1, 10, enableCache: false, cancellationToken: token)
            };
            await connection.Started.Task.WaitAsync(TimeSpan.FromSeconds(2));
            Assert.Equal(token, connection.ReceivedToken);
            cancellation.Cancel();
            await Assert.ThrowsAnyAsync<OperationCanceledException>(() => pending.WaitAsync(TimeSpan.FromSeconds(2)));
        }

        private sealed class ProbeDapper : BaseDapper<ProbeConnection>
        {
            public ProbeDapper(IServiceProvider services) : base(services) { }
        }

        public sealed class ProbeConnection : DbConnection
        {
            private ConnectionState state;
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public CancellationToken ReceivedToken;
            public override string ConnectionString { get; set; }
            public override string Database => "probe";
            public override string DataSource => "probe";
            public override string ServerVersion => "1";
            public override ConnectionState State => state;
            public override void Open() => state = ConnectionState.Open;
            public override void Close() => state = ConnectionState.Closed;
            public override void ChangeDatabase(string databaseName) => throw new NotSupportedException();
            protected override DbTransaction BeginDbTransaction(IsolationLevel isolationLevel) => throw new NotSupportedException();
            protected override DbCommand CreateDbCommand() => new ProbeCommand(this);
        }

        private sealed class ProbeCommand : DbCommand
        {
            private readonly ProbeConnection owner;
            private readonly SQLiteCommand parameters = new SQLiteCommand();
            public ProbeCommand(ProbeConnection owner) { this.owner = owner; DbConnection = owner; }
            public override string CommandText { get; set; }
            public override int CommandTimeout { get; set; }
            public override CommandType CommandType { get; set; }
            public override bool DesignTimeVisible { get; set; }
            public override UpdateRowSource UpdatedRowSource { get; set; }
            protected override DbConnection DbConnection { get; set; }
            protected override DbTransaction DbTransaction { get; set; }
            protected override DbParameterCollection DbParameterCollection => parameters.Parameters;
            protected override DbParameter CreateDbParameter() => new SQLiteParameter();
            public override void Cancel() { }
            public override void Prepare() { }
            public override int ExecuteNonQuery() => throw new NotSupportedException();
            public override object ExecuteScalar() => throw new NotSupportedException();
            protected override DbDataReader ExecuteDbDataReader(CommandBehavior behavior) => throw new NotSupportedException();
            protected override async Task<DbDataReader> ExecuteDbDataReaderAsync(CommandBehavior behavior, CancellationToken cancellationToken)
            {
                owner.ReceivedToken = cancellationToken;
                owner.Started.TrySetResult(true);
                await Task.Delay(Timeout.Infinite, cancellationToken);
                throw new InvalidOperationException("Expected cancellation");
            }
            protected override void Dispose(bool disposing) { if (disposing) parameters.Dispose(); base.Dispose(disposing); }
        }
    }
}
