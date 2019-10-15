using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            _host = Host.CreateDefaultBuilder().ConfigureServices(context =>
            {
                context.AddDapperForSQLite();
                context.AddDapperCachingInMemory(new MemoryConfiguration
                {
                    AllMethodsEnableCache = false,
                    Expire = TimeSpan.FromMinutes(1)
                });
            }).ConfigureHostConfiguration(builder =>
                {
                    builder.SetBasePath(Directory.GetCurrentDirectory());
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
