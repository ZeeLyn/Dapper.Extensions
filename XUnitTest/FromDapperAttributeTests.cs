using System;
using System.Linq;
using Dapper.Extensions;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Xunit;
using static XUnitTest.MicrosoftDiRegistrationTests;

namespace XUnitTest
{
    public class FromDapperAttributeTests
    {
        private static ServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().Build());
            services.AddSingleton<IConnectionStringProvider, RouteProvider>();
            services.AddSingleton<TrackingState>();
            return services;
        }

        [Theory]
        [InlineData(false)]
        [InlineData(true)]
        public void ConstructorSelectionPreservesRolesScopesAndSingleDisposal(bool monitored)
        {
            var services = CreateServices();
            Action<MonitorBuilder> monitor = monitored ? m => m.EnableLog = false : (Action<MonitorBuilder>)null;
            services.AddDapper<TrackingDapper>("Default", null, true, monitor);
            services.AddDapper<TrackingDapper>("Orders", "orders", true, monitor);
            services.AddDapper<TrackingDapper>("Independent", "orders_slave", false, monitor);
            services.AddScoped<SelectionConsumer>();
            using var provider = services.BuildServiceProvider(new ServiceProviderOptions
            {
                ValidateScopes = true, ValidateOnBuild = true
            });
            Assert.Throws<InvalidOperationException>(() => provider.GetRequiredService<SelectionConsumer>());
            SelectionConsumer previous = null;
            for (var i = 0; i < 2; i++)
            {
                using var scope = provider.CreateScope();
                var consumer = scope.ServiceProvider.GetRequiredService<SelectionConsumer>();
                var resolver = scope.ServiceProvider.GetRequiredService<IDapperResolver>();
                Assert.Same(resolver.ResolveDapper(), consumer.DefaultWriter);
                Assert.Same(resolver.ResolveDapper(true), consumer.DefaultReader);
                Assert.Same(resolver.ResolveDapper("orders"), consumer.Writer);
                Assert.Same(resolver.ResolveDapper("orders", true), consumer.Reader);
                Assert.Same(resolver.ResolveDapper("orders_slave"), consumer.Independent);
                Assert.Same(scope.ServiceProvider.GetRequiredKeyedService<IDapper>("orders"), consumer.Writer);
                var instances = new[] { consumer.DefaultWriter, consumer.DefaultReader, consumer.Writer, consumer.Reader, consumer.Independent };
                Assert.Equal(5, instances.Distinct().Count());
                Assert.All(instances, db => Assert.Equal(1, db.ExecuteScalar<int>("select 1")));
                var activated = ActivatorUtilities.CreateInstance<SelectionConsumer>(scope.ServiceProvider);
                Assert.Same(consumer.DefaultWriter, activated.DefaultWriter);
                Assert.Same(consumer.DefaultReader, activated.DefaultReader);
                Assert.Same(consumer.Writer, activated.Writer);
                Assert.Same(consumer.Reader, activated.Reader);
                Assert.Same(consumer.Independent, activated.Independent);
                if (previous != null)
                {
                    Assert.NotSame(previous.Writer, consumer.Writer);
                    Assert.NotSame(previous.Reader, consumer.Reader);
                }
                previous = consumer;
            }
            var state = provider.GetRequiredService<TrackingState>();
            Assert.Equal(10, state.Instances.Count);
            Assert.Equal(4, state.Instances.Count(x => x.ReadOnlyRole));
            Assert.All(state.Instances, db => Assert.Equal(1, db.DisposeCalls));
        }

        [Fact]
        public void DefaultSelectionNormalizesEmptyKeysAndDoesNotInheritTheConsumersKey()
        {
            var services = CreateServices();
            services.AddDapperForSQLite();
            services.AddKeyedScoped<DefaultConsumer>("consumer");
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            var consumer = scope.ServiceProvider.GetRequiredKeyedService<DefaultConsumer>("consumer");
            var db = scope.ServiceProvider.GetRequiredService<IDapper>();
            Assert.Same(db, consumer.Default);
            Assert.Same(db, consumer.NullKey);
            Assert.Same(db, consumer.WhitespaceKey);
        }

        [Theory]
        [InlineData(typeof(MissingConsumer))]
        [InlineData(typeof(NamedReaderConsumer))]
        [InlineData(typeof(DefaultReaderConsumer))]
        public void MissingNameOrUnregisteredSlaveFailsInsteadOfFallingBack(Type consumerType)
        {
            var services = CreateServices();
            services.AddDapperForSQLite();
            services.AddDapperForSQLite("Orders", "orders");
            services.AddScoped(consumerType);
            using var provider = services.BuildServiceProvider();
            using var scope = provider.CreateScope();
            Assert.Throws<InvalidOperationException>(() => scope.ServiceProvider.GetRequiredService(consumerType));
        }

        public sealed class SelectionConsumer
        {
            public IDapper DefaultWriter { get; }
            public IDapper DefaultReader { get; }
            public IDapper Writer { get; }
            public IDapper Reader { get; }
            public IDapper Independent { get; }

            public SelectionConsumer([FromDapper] IDapper defaultWriter,
                [FromDapper(readOnly: true)] IDapper defaultReader,
                [FromDapper("orders")] IDapper writer,
                [FromDapper("orders", readOnly: true)] IDapper reader,
                [FromDapper("orders_slave")] IDapper independent)
            {
                DefaultWriter = defaultWriter; DefaultReader = defaultReader;
                Writer = writer; Reader = reader; Independent = independent;
            }
        }

        public sealed class DefaultConsumer
        {
            public IDapper Default { get; }
            public IDapper NullKey { get; }
            public IDapper WhitespaceKey { get; }
            public DefaultConsumer([FromDapper] IDapper db, [FromDapper(null)] IDapper nullKey,
                [FromDapper(" \t")] IDapper whitespaceKey)
            { Default = db; NullKey = nullKey; WhitespaceKey = whitespaceKey; }
        }

        public sealed class MissingConsumer
        {
            public MissingConsumer([FromDapper("missing")] IDapper db) { }
        }

        public sealed class NamedReaderConsumer
        {
            public NamedReaderConsumer([FromDapper("orders", readOnly: true)] IDapper db) { }
        }

        public sealed class DefaultReaderConsumer
        {
            public DefaultReaderConsumer([FromDapper(readOnly: true)] IDapper db) { }
        }
    }
}
