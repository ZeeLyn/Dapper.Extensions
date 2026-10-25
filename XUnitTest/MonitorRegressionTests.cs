using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Extensions;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Xunit;

namespace XUnitTest
{
    public class MonitorRegressionTests
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(true, false, false)]
        [InlineData(false, true, false)]
        [InlineData(true, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, true, true)]
        public async Task AsyncOnlyMonitorResourcesAreAwaitedEvenWhenHandlerOrSqlFails(bool async, bool handlerFails, bool sqlFails)
        {
            var state = new MonitorState { Throw = handlerFails };
            using var env = new RegressionEnvironment(s =>
            {
                s.AddSingleton(state);
                s.AddScoped<AsyncOnlyDependency>();
                s.AddScoped<IMonitorHandler, AsyncOnlyHandler>();
                s.AddSingleton(new MonitorConfiguration { SlowCriticalValue = -1, HasCustomMonitorHandler = true, EnableLog = false });
            });
            var proxy = new DapperProxy(env.Db, env.Services);
            var sql = sqlFails ? "select * from missing_table" : "select 1";
            var pending = async ? proxy.QueryFirstOrDefaultAsync<int>(sql)
                : Task.Run(() => proxy.QueryFirstOrDefault<int>(sql));
            try
            {
                await state.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.False(pending.IsCompleted);
                Assert.Equal(0, state.Disposed);
            }
            finally { state.Release.TrySetResult(true); }
            if (sqlFails)
                Assert.Contains("missing_table", (await Assert.ThrowsAsync<System.Data.SQLite.SQLiteException>(
                    async () => await pending.WaitAsync(TimeSpan.FromSeconds(5)))).Message);
            else Assert.Equal(1, await pending.WaitAsync(TimeSpan.FromSeconds(5)));
            Assert.Equal(1, state.Calls);
            Assert.Equal(2, state.Disposed);
        }

        public sealed class AsyncOnlyDependency : IAsyncDisposable
        {
            private readonly MonitorState state;
            public AsyncOnlyDependency(MonitorState state) => this.state = state;
            public async ValueTask DisposeAsync() { await Task.Yield(); state.Disposed++; }
        }

        public sealed class AsyncOnlyHandler : IMonitorHandler, IAsyncDisposable
        {
            private readonly MonitorState state;
            public AsyncOnlyHandler(MonitorState state, AsyncOnlyDependency dependency) => this.state = state;
            public async Task OnSlowSqlCommandAsync(string methodName, string sql, object param, long duration)
            {
                state.Calls++;
                await Task.Yield();
                if (state.Throw) throw new InvalidOperationException("monitor failure");
            }
            public async ValueTask DisposeAsync()
            {
                state.Started.TrySetResult(true);
                await state.Release.Task;
                state.Disposed++;
            }
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(2)]
        public void LoggingSerializationAndResolutionFailuresCannotMaskWrites(int failure)
        {
            using var env = new RegressionEnvironment(s =>
            {
                s.AddSingleton(new MonitorConfiguration { SlowCriticalValue = -1, EnableLog = failure != 2, HasCustomMonitorHandler = failure == 2 });
                if (failure == 1) s.AddSingleton<ILogger<DapperProxy>, ThrowingLogger>();
                if (failure == 2) s.AddScoped<IMonitorHandler>(_ => throw new InvalidOperationException("resolution failure"));
            });
            var proxy = new DapperProxy(env.Db, env.Services);
            Assert.Equal(1, proxy.Execute("create table item(value integer);insert into item values(1)", failure == 0 ? new Cycle() : null));
            Assert.Equal(1, env.Db.ExecuteScalar<int>("select count(*) from item"));
        }

        [Theory]
        [InlineData(false, false)]
        [InlineData(false, true)]
        [InlineData(true, false)]
        [InlineData(true, true)]
        public async Task HandlerFailureCannotMaskSqlOutcome(bool async, bool throwAfterAwait)
        {
            var state = new MonitorState { Throw = true, ThrowAfterAwait = throwAfterAwait };
            using var env = new RegressionEnvironment(s =>
            {
                s.AddSingleton(state);
                s.AddScoped<IMonitorHandler, RecordingHandler>();
                s.AddSingleton(new MonitorConfiguration { SlowCriticalValue = -1, HasCustomMonitorHandler = true });
            });
            var proxy = new DapperProxy(env.Db, env.Services);
            if (async) Assert.Equal(1, await proxy.ExecuteAsync("create table item(value integer);insert into item values(1)"));
            else Assert.Equal(1, proxy.Execute("create table item(value integer);insert into item values(1)"));
            Assert.Equal(1, env.Db.ExecuteScalar<int>("select count(*) from item"));
            if (async) await Assert.ThrowsAsync<System.Data.SQLite.SQLiteException>(() => proxy.ExecuteAsync("invalid sql"));
            else Assert.Throws<System.Data.SQLite.SQLiteException>(() => proxy.Execute("invalid sql"));
            Assert.Equal(2, state.Calls);
            Assert.Equal(2, state.Disposed);
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public async Task HandlerScopeLivesUntilAsyncWorkCompletes(bool async)
        {
            var state = new MonitorState { Wait = true };
            using var env = new RegressionEnvironment(s =>
            {
                s.AddSingleton(state);
                s.AddScoped<IMonitorHandler, RecordingHandler>();
                s.AddSingleton(new MonitorConfiguration { SlowCriticalValue = -1, HasCustomMonitorHandler = true });
            });
            var proxy = new DapperProxy(env.Db, env.Services);
            var pending = async ? proxy.ExecuteAsync("create table item(value integer)") : Task.Run(() => proxy.Execute("create table item(value integer)"));
            try
            {
                await state.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                Assert.False(pending.IsCompleted);
                Assert.Equal(0, state.Disposed);
            }
            finally { state.Release.TrySetResult(true); }
            await pending.WaitAsync(TimeSpan.FromSeconds(5));
            Assert.True(state.Completed);
            Assert.Equal(1, state.Disposed);
        }

        [Fact]
        public void AutofacRegistrationsKeepIndependentThresholdsAndHandlers()
        {
            var services = RegressionEnvironment.CreateServices();
            var state = new MonitorState();
            services.AddSingleton(state);
            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.AddDapper<SQLiteDapper>("A", "quiet", monitorBuilder: m => { m.Threshold = int.MaxValue; m.EnableLog = false; m.AddMonitorHandler<RecordingHandler>(); });
            builder.AddDapper<SQLiteDapper>("A", "a", monitorBuilder: m => { m.Threshold = -1; m.EnableLog = false; m.AddMonitorHandler<RecordingHandler>(); });
            builder.AddDapper<SQLiteDapper>("B", "b", monitorBuilder: m => { m.Threshold = -1; m.EnableLog = false; m.AddMonitorHandler<OtherHandler>(); });
            using var container = builder.Build();
            using var scope = container.BeginLifetimeScope();
            scope.ResolveKeyed<IDapper>("quiet").ExecuteScalar<int>("select 1");
            Assert.Equal(0, state.Calls);
            scope.ResolveKeyed<IDapper>("a").ExecuteScalar<int>("select 1");
            Assert.Equal(1, state.Calls);
            Assert.Equal(0, state.OtherCalls);
            scope.ResolveKeyed<IDapper>("b").ExecuteScalar<int>("select 1");
            Assert.Equal(1, state.OtherCalls);
        }

        [Fact]
        public void MicrosoftDiRegistrationsKeepIndependentMonitoring()
        {
            var services = RegressionEnvironment.CreateServices();
            var state = new MonitorState();
            services.AddSingleton(state);
            services.AddDapper<SQLiteDapper>(m => { m.Threshold = int.MaxValue; m.EnableLog = false; m.AddMonitorHandler<RecordingHandler>(); });
            services.AddDapper<SQLiteDapper>(m => { m.Threshold = -1; m.EnableLog = false; m.AddMonitorHandler<OtherHandler>(); });
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var registrations = scope.ServiceProvider.GetServices<IDapper>().ToArray();
            registrations[1].ExecuteScalar<int>("select 1");
            Assert.Equal(0, state.OtherCalls);
            registrations[2].ExecuteScalar<int>("select 1");
            Assert.Equal(0, state.Calls);
            Assert.Equal(1, state.OtherCalls);
        }

        public sealed class MonitorState
        {
            public int Calls, OtherCalls, Disposed;
            public bool Throw, ThrowAfterAwait, Wait, Completed;
            public readonly TaskCompletionSource<bool> Started = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
            public readonly TaskCompletionSource<bool> Release = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        }

        public sealed class RecordingHandler : IMonitorHandler, IDisposable
        {
            private readonly MonitorState state;
            private bool disposed;
            public RecordingHandler(MonitorState state) { this.state = state; }
            public Task OnSlowSqlCommandAsync(string methodName, string sql, object param, long duration)
            {
                state.Calls++;
                if (state.Throw && !state.ThrowAfterAwait) throw new InvalidOperationException("monitor failure");
                return Complete();
            }
            private async Task Complete()
            {
                state.Started.TrySetResult(true);
                if (state.Wait) await state.Release.Task;
                else await Task.Yield();
                Assert.False(disposed);
                if (state.Throw) throw new InvalidOperationException("async monitor failure");
                state.Completed = true;
            }
            public void Dispose() { disposed = true; state.Disposed++; }
        }

        public sealed class OtherHandler : IMonitorHandler
        {
            private readonly MonitorState state;
            public OtherHandler(MonitorState state) { this.state = state; }
            public Task OnSlowSqlCommandAsync(string methodName, string sql, object param, long duration) { state.OtherCalls++; return Task.CompletedTask; }
        }

        public sealed class Cycle { public Cycle Self => this; }

        public sealed class ThrowingLogger : ILogger<DapperProxy>
        {
            public IDisposable BeginScope<TState>(TState state) => null;
            public bool IsEnabled(LogLevel level) => true;
            public void Log<TState>(LogLevel level, EventId eventId, TState state, Exception exception, Func<TState, Exception, string> formatter)
                => throw new InvalidOperationException("logging failure");
        }
    }
}
