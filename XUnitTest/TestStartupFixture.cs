using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Extensions;
using Dapper.Extensions.Caching.Memory;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Xunit;

namespace XUnitTest
{
    public class TestStartupFixture : IDisposable
    {
        private readonly IHost _host;
        public IServiceProvider Services { get; }

        public TestStartupFixture()
        {
            _host = Host.CreateDefaultBuilder().UseServiceProviderFactory(new AutofacServiceProviderFactory()).ConfigureHostConfiguration(builder =>
            {
                builder.SetBasePath(Directory.GetCurrentDirectory());
            }).ConfigureServices(context =>
            {
                context.AddDapperForSQLite();
                context.AddDapperCachingInMemory(new MemoryConfiguration
                {
                    AllMethodsEnableCache = false,
                    Expire = TimeSpan.FromMinutes(1)
                });
                context.AddSQLSeparateForDapper(Path.Combine(Directory.GetCurrentDirectory(), "sql"));
            }).ConfigureContainer<ContainerBuilder>(builder =>
            {
                builder.AddDapperForSQLite("master_slave", "master_slave", true);
            }).Build();
            Services = _host.Services;
        }

        public void Dispose()
        {
            _host.Dispose();
        }
    }
    [CollectionDefinition("TestHostCollection")]
    public class TestHostCollection : ICollectionFixture<TestStartupFixture>
    {

    }
}
