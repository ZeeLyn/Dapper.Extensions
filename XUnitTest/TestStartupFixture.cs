using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
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
            _host = Host.CreateDefaultBuilder().ConfigureServices(context => { context.AddDapperForSQLite(); }).ConfigureHostConfiguration(builder =>
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
