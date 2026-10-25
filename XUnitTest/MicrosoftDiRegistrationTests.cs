using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions;
using Dapper.Extensions.Caching.Memory;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.MSSQL;
using Dapper.Extensions.MySql;
using Dapper.Extensions.Odbc;
using Dapper.Extensions.Oracle;
using Dapper.Extensions.PostgreSql;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace XUnitTest
{
    public class MicrosoftDiRegistrationTests
    {
        private static ServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string> { ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:" }).Build());
            return services;
        }

        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void RegistrationMatrixPreservesArgumentsScopesAndDisposal(bool named, bool masterSlave, bool monitored)
        {
            var services = CreateServices();
            var state = new TrackingState();
            var routes = new RouteProvider();
            services.AddSingleton(state);
            services.AddSingleton<IConnectionStringProvider>(routes);
            services.AddDapper<TrackingDapper>("Orders", named ? "orders" : null, masterSlave,
                monitored ? m => m.EnableLog = false : (Action<MonitorBuilder>)null);
            using var provider = services.BuildServiceProvider(new ServiceProviderOptions { ValidateScopes = true, ValidateOnBuild = true });
            Assert.Same(routes, provider.GetRequiredService<IConnectionStringProvider>());
            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<IDapperResolver>());
            for (var iteration = 0; iteration < 2; iteration++)
            {
                using var scope = provider.CreateScope();
                var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
                var master = named ? resolver.ResolveDapper("orders") : resolver.ResolveDapper();
                Assert.Same(master, resolver.ResolveDapper(named ? "orders" : null));
                Assert.Same(master, named ? scope.ServiceProvider.GetRequiredKeyedService<IDapper>("orders") : scope.ServiceProvider.GetRequiredService<IDapper>());
                Assert.Equal(1, master.ExecuteScalar<int>("select 1"));
                if (monitored) Assert.IsType<DapperProxy>(master);
                else Assert.IsType<TrackingDapper>(master);
                if (masterSlave)
                {
                    var slave = named ? resolver.ResolveDapper("orders", true) : resolver.ResolveDapper(true);
                    Assert.NotSame(master, slave);
                    Assert.Same(slave, scope.ServiceProvider.GetRequiredKeyedService<IDapper>((named ? "orders" : (string)null, true)));
                    Assert.Equal(1, slave.ExecuteScalar<int>("select 1"));
                }
                else Assert.Throws<InvalidOperationException>(() => resolver.ResolveDapper(named ? "orders" : null, true));
            }
            Assert.Equal(masterSlave ? 4 : 2, state.Instances.Count);
            Assert.Equal(state.Instances.Count, routes.Calls.Count);
            Assert.All(state.Instances, instance =>
            {
                Assert.Equal("Orders", instance.ConnectionName);
                Assert.Equal(masterSlave, instance.MasterSlave);
                Assert.Equal(1, instance.DisposeCalls);
                Assert.Throws<ObjectDisposedException>(() => instance.Conn.Value.Open());
            });
            Assert.Equal(masterSlave ? 2 : 0, state.Instances.Count(x => x.ReadOnlyRole));
            Assert.All(routes.Calls, call => Assert.Equal(("Orders", masterSlave), (call.Name, call.MasterSlave)));
        }

        [Fact]
        public async Task NamedConnectionsKeepTheirDataCacheAndMonitorConfiguration()
        {
            var services = CreateServices();
            var monitors = new MonitorRegressionTests.MonitorState();
            services.AddSingleton(monitors);
            services.AddSingleton<IConnectionStringProvider, RouteProvider>();
            services.AddDapperCachingInMemory(new MemoryConfiguration { AllMethodsEnableCache = true });
            services.AddDapperForSQLite("A", "a", monitorBuilder: m =>
            {
                m.Threshold = -1; m.EnableLog = false;
                m.AddMonitorHandler<MonitorRegressionTests.RecordingHandler>();
            });
            services.AddDapperForSQLite("B", "b", monitorBuilder: m =>
            {
                m.Threshold = -1; m.EnableLog = false;
                m.AddMonitorHandler<MonitorRegressionTests.OtherHandler>();
            });
            services.AddDapperForSQLite("Quiet", "quiet", monitorBuilder: m =>
            {
                m.Threshold = int.MaxValue; m.EnableLog = false;
                m.AddMonitorHandler<MonitorRegressionTests.RecordingHandler>();
            });
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
            var a = resolver.ResolveDapper("a");
            var b = resolver.ResolveDapper("b");
            a.Execute("create table item(value integer);insert into item values(11)");
            b.Execute("create table item(value integer);insert into item values(22)");
            Assert.Equal(11, await a.QueryFirstOrDefaultAsync<int>("select value from item"));
            Assert.Equal(22, await b.QueryFirstOrDefaultAsync<int>("select value from item"));
            Assert.Equal(2, monitors.Calls);
            Assert.Equal(2, monitors.OtherCalls);
            resolver.ResolveDapper("quiet").ExecuteScalar<int>("select 1");
            Assert.Equal(2, monitors.Calls);
            a.BeginTransaction();
            a.Execute("update item set value=33");
            Assert.Equal(33, a.QueryFirstOrDefault<int>("select value from item"));
            Assert.Equal(22, b.QueryFirstOrDefault<int>("select value from item"));
            a.RollbackTransaction();
            Assert.Equal(11, a.QueryFirstOrDefault<int>("select value from item"));
        }

        [Fact]
        public void DefaultConnectionAndSlaveLikeNamesDoNotCollide()
        {
            var services = CreateServices();
            services.AddSingleton<IConnectionStringProvider, RouteProvider>();
            services.AddDapperForSQLite("DefaultConnection", null, true);
            services.AddDapperForSQLite("A", "orders", true);
            services.AddDapperForSQLite("B", "orders_slave");
            services.AddDapperForSQLite("C", "_slave");
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
            var instances = new[] { resolver.ResolveDapper(), resolver.ResolveDapper(true), resolver.ResolveDapper("orders"),
                resolver.ResolveDapper("orders", true), resolver.ResolveDapper("orders_slave"), resolver.ResolveDapper("_slave") };
            Assert.Equal(6, instances.Distinct().Count());
            Assert.Same(instances[0], resolver.ResolveDapper(" "));
            Assert.Throws<InvalidOperationException>(() => resolver.ResolveDapper("missing"));
            Assert.Throws<InvalidOperationException>(() => resolver.ResolveDapper("orders_slave", true));
        }

        [Fact]
        public void DefaultConnectionProviderSupportsMasterSlaveConfigurationWithoutAutofac()
        {
            var services = CreateServices();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(new Dictionary<string, string>
            {
                ["ConnectionStrings:Orders:Master"] = "Data Source=:memory:;Version=3",
                ["ConnectionStrings:Orders:Slaves:0:ConnectionString"] = "Data Source=:memory:;Version=3;Cache Size=1234",
                ["ConnectionStrings:Orders:Slaves:0:Weight"] = "1"
            }).Build());
            services.AddDapperForSQLite("Orders", "orders", true);
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
            var master = resolver.ResolveDapper("orders");
            var slave = resolver.ResolveDapper("orders", true);
            Assert.Equal(1, master.ExecuteScalar<int>("select 1"));
            Assert.Equal(1, slave.ExecuteScalar<int>("select 1"));
            Assert.DoesNotContain("1234", master.Conn.Value.ConnectionString);
            Assert.Contains("1234", slave.Conn.Value.ConnectionString);
        }

        [Theory]
        [InlineData("sqlite")]
        [InlineData("mssql")]
        [InlineData("mysql")]
        [InlineData("postgres")]
        [InlineData("oracle")]
        [InlineData("odbc")]
        public void EveryDriverExposesNamedMicrosoftDiRegistration(string driver)
        {
            var services = CreateServices();
            Type expected;
            switch (driver)
            {
                case "sqlite": services.AddDapperForSQLite("Orders", driver, true); expected = typeof(SQLiteDapper); break;
                case "mssql": services.AddDapperForMSSQL("Orders", driver, true); expected = typeof(MsSqlDapper); break;
                case "mysql": services.AddDapperForMySQL("Orders", driver, true); expected = typeof(MySqlDapper); break;
                case "postgres": services.AddDapperForPostgreSQL("Orders", driver, true); expected = typeof(PostgreSqlDapper); break;
                case "oracle": services.AddDapperForOracle("Orders", driver, true); expected = typeof(OracleDapper); break;
                default: services.AddDapperForODBC("Orders", driver, true); expected = typeof(OdbcDapper); break;
            }
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
            var master = resolver.ResolveDapper(driver);
            var slave = resolver.ResolveDapper(driver, true);
            Assert.IsType(expected, master);
            Assert.IsType(expected, slave);
            Assert.NotSame(master, slave);
            Assert.False(master.Conn.IsValueCreated);
            Assert.False(slave.Conn.IsValueCreated);
        }

        [Fact]
        public void ExistingNullAndCallbackOverloadsStillCompileAndResolve()
        {
            var services = CreateServices();
            services.AddDapperForSQLite();
            services.AddDapperForSQLite(null);
            services.AddDapperForSQLite(m => m.EnableLog = false);
            services.AddDapper<SQLiteDapper>(null);
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            Assert.Equal(4, scope.ServiceProvider.GetServices<IDapper>().Count());
            Assert.IsType<SQLiteDapper>(scope.ServiceProvider.GetRequiredService<IDapperResolver>().ResolveDapper());
        }

        [Fact]
        public void InvalidConnectionNameIsRejectedBeforeRegistration()
        {
            var services = CreateServices();
            var count = services.Count;
            Assert.Throws<ArgumentException>(() => services.AddDapperForSQLite(" ", "orders"));
            Assert.Equal(count, services.Count);
            Assert.Throws<ArgumentNullException>(() => Dapper.Extensions.DependencyInjectionExtensions.AddDapper<SQLiteDapper>(null, "Orders", "orders"));
        }

        [Fact]
        public void NativeKeyedConstructorInjectionResolvesTheSameScopedInstance()
        {
            var services = CreateServices();
            services.AddDapperForSQLite("DefaultConnection", "orders");
            services.AddScoped<NamedConsumer>();
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredService<NamedConsumer>();
            Assert.Same(scope.ServiceProvider.GetRequiredService<IDapperResolver>().ResolveDapper("orders"), consumer.Db);
            Assert.Equal(1, consumer.Db.ExecuteScalar<int>("select 1"));
        }

        [Fact]
        public async Task AsyncMonitoringAndAsyncScopeDisposalKeepOwnershipCorrect()
        {
            var services = CreateServices();
            var tracking = new TrackingState();
            var monitoring = new MonitorRegressionTests.MonitorState { Wait = true };
            services.AddSingleton(tracking);
            services.AddSingleton(monitoring);
            services.AddDapper<TrackingDapper>("DefaultConnection", "orders", monitorBuilder: m =>
            {
                m.Threshold = -1; m.EnableLog = false;
                m.AddMonitorHandler<MonitorRegressionTests.RecordingHandler>();
            });
            await using var provider = services.BuildServiceProvider();
            await using (var scope = provider.CreateAsyncScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<IDapperResolver>().ResolveDapper("orders");
                var pending = db.QueryAsync<int>("select 1");
                try
                {
                    await monitoring.Started.Task.WaitAsync(TimeSpan.FromSeconds(5));
                    Assert.False(pending.IsCompleted);
                    Assert.Equal(0, monitoring.Disposed);
                    Assert.Equal(0, Assert.Single(tracking.Instances).DisposeCalls);
                }
                finally { monitoring.Release.TrySetResult(true); }
                Assert.Equal(new[] { 1 }, await pending.WaitAsync(TimeSpan.FromSeconds(5)));
                Assert.True(monitoring.Completed);
                Assert.Equal(1, monitoring.Disposed);
            }
            Assert.Equal(1, Assert.Single(tracking.Instances).DisposeCalls);
        }

        public sealed class NamedConsumer
        {
            public IDapper Db { get; }
            public NamedConsumer([FromKeyedServices("orders")] IDapper db) => Db = db;
        }

        public sealed class TrackingState { public List<TrackingDapper> Instances { get; } = new List<TrackingDapper>(); }

        public sealed class TrackingDapper : SQLiteDapper
        {
            public string ConnectionName { get; }
            public bool MasterSlave { get; }
            public bool ReadOnlyRole { get; }
            public int DisposeCalls { get; private set; }
            public TrackingDapper(IServiceProvider services, string connectionName = "DefaultConnection", bool enableMasterSlave = false, bool readOnly = false)
                : base(services, connectionName, enableMasterSlave, readOnly)
            {
                ConnectionName = connectionName; MasterSlave = enableMasterSlave; ReadOnlyRole = readOnly;
                services.GetRequiredService<TrackingState>().Instances.Add(this);
            }
            public override void Dispose() { DisposeCalls++; base.Dispose(); }
        }

        public sealed class RouteProvider : IConnectionStringProvider
        {
            public List<(string Name, bool MasterSlave, bool ReadOnly)> Calls { get; } = new List<(string, bool, bool)>();
            public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
            {
                Calls.Add((connectionName, enableMasterSlave, readOnly));
                return "Data Source=:memory:";
            }
        }
    }
}
