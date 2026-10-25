using System;
using System.Collections.Generic;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Extensions;
using Dapper.Extensions.Caching;
using Dapper.Extensions.Caching.Memory;
using Dapper.Extensions.Caching.Redis;
using Dapper.Extensions.SQLite;
using FreeRedis;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace XUnitTest
{
    public class RegistrationAndRedisRegressionTests
    {
        [Theory]
        [InlineData(false, false, false)]
        [InlineData(false, false, true)]
        [InlineData(false, true, false)]
        [InlineData(false, true, true)]
        [InlineData(true, false, false)]
        [InlineData(true, false, true)]
        [InlineData(true, true, false)]
        [InlineData(true, true, true)]
        public void AutofacCanResolveAllRegistrationVariants(bool named, bool masterSlave, bool monitored)
        {
            var builder = new ContainerBuilder();
            builder.Populate(RegressionEnvironment.CreateServices());
            builder.AddDapper<SQLiteDapper>(serviceKey: named ? "db" : null, enableMasterSlave: masterSlave,
                monitorBuilder: monitored ? m => m.EnableLog = false : (Action<Dapper.Extensions.Monitor.MonitorBuilder>)null);
            builder.AddDapperConnectionStringProvider<RouteProvider>();
            using var container = builder.Build();
            using var scope = container.BeginLifetimeScope();
            var context = scope.Resolve<IResolveContext>();
            var master = named ? context.ResolveDapper("db") : context.ResolveDapper();
            Assert.NotNull(master);
            Assert.Equal(1, master.ExecuteScalar<int>("select 1"));
            var routes = scope.Resolve<IConnectionStringProvider>() as RouteProvider;
            Assert.Equal(new[] { false }, routes.ReadOnlyCalls);
            if (masterSlave)
            {
                var slave = named ? context.ResolveDapper("db", true) : context.ResolveDapper(true);
                Assert.NotNull(slave);
                Assert.NotSame(master, slave);
                Assert.Equal(1, slave.ExecuteScalar<int>("select 1"));
                Assert.Equal(new[] { false, true }, routes.ReadOnlyCalls);
            }
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void AutofacMemoryPreservesPrefix(bool custom)
        {
            var builder = new ContainerBuilder();
            builder.Populate(RegressionEnvironment.CreateServices());
            var config = new MemoryConfiguration { KeyPrefix = "tenant" };
            if (custom) builder.AddDapperCachingInMemory<DefaultCacheKeyBuilder>(config);
            else builder.AddDapperCachingInMemory(config);
            using var container = builder.Build();
            Assert.Equal("tenant:key", container.Resolve<ICacheKeyBuilder>().Generate(null, null, "key"));
        }

        [Fact]
        public void RedisUsesRegisteredCustomKeyBuilder()
        {
            var services = new ServiceCollection();
            services.AddDapperCachingInRedis<CustomKeyBuilder>(new RedisConfiguration { ConnectionString = "127.0.0.1:1" });
            using var provider = services.BuildServiceProvider();
            Assert.IsType<CustomKeyBuilder>(provider.GetRequiredService<ICacheKeyBuilder>());
        }

        [Theory]
        [InlineData(500, "PX 500")]
        [InlineData(1, "PX 1")]
        [InlineData(2000, "EX 2")]
        public void RedisPreservesExpiration(double milliseconds, string expected)
        {
            using var client = new RedisClient("127.0.0.1:1,connectTimeout=10");
            var capture = new CaptureInterceptor();
            client.Interceptors.Add(() => capture);
            var provider = new RedisCacheProvider(new DataSerializer(), client);
            Assert.True(provider.TrySet("key", 123, TimeSpan.FromMilliseconds(milliseconds)));
            Assert.Contains(expected, Assert.Single(capture.Commands));
            capture.Commands.Clear();
            provider.TrySet("forever", 123);
            var command = Assert.Single(capture.Commands);
            Assert.DoesNotContain("EX ", command);
            Assert.DoesNotContain("PX ", command);
        }

        [Theory]
        [InlineData(-1)]
        [InlineData(0)]
        [InlineData(0.5)]
        public void RedisRejectsUnsupportedExpirationBeforeSending(double milliseconds)
        {
            using var client = new RedisClient("127.0.0.1:1");
            var capture = new CaptureInterceptor();
            client.Interceptors.Add(() => capture);
            var provider = new RedisCacheProvider(new DataSerializer(), client);
            Assert.Throws<ArgumentOutOfRangeException>(() => provider.TrySet("key", 1, TimeSpan.FromMilliseconds(milliseconds)));
            Assert.Empty(capture.Commands);
        }

        public sealed class CustomKeyBuilder : ICacheKeyBuilder
        {
            public string Generate(string sql, object param, string customKey, int? pageIndex = null, int? pageSize = null) => customKey ?? "custom";
        }

        public sealed class RouteProvider : IConnectionStringProvider
        {
            public List<bool> ReadOnlyCalls { get; } = new List<bool>();
            public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
            {
                ReadOnlyCalls.Add(readOnly);
                return "Data Source=:memory:";
            }
        }

        private sealed class CaptureInterceptor : IInterceptor
        {
            public List<string> Commands { get; } = new List<string>();
            public void Before(InterceptorBeforeEventArgs args) { Commands.Add(args.Command.ToString()); args.Value = "OK"; }
            public void After(InterceptorAfterEventArgs args) { }
        }
    }
}
