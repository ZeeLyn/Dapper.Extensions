using System;
using System.Collections.Generic;
using Dapper.Extensions;
using Dapper.Extensions.Caching.Memory;
using Dapper.Extensions.SQLite;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace XUnitTest
{
    internal sealed class RegressionEnvironment : IDisposable
    {
        public ServiceProvider Services { get; }
        public SQLiteDapper Db { get; }

        public static ServiceCollection CreateServices()
        {
            var services = new ServiceCollection();
            services.AddLogging();
            services.AddSingleton<IConfiguration>(new ConfigurationBuilder().AddInMemoryCollection(
                new Dictionary<string, string>
                {
                    ["ConnectionStrings:DefaultConnection"] = "Data Source=:memory:",
                    ["ConnectionStrings:A"] = "Data Source=:memory:",
                    ["ConnectionStrings:B"] = "Data Source=:memory:"
                }).Build());
            services.AddDapper<SQLiteDapper>();
            services.AddDapperCachingInMemory(new MemoryConfiguration { AllMethodsEnableCache = true, KeyPrefix = "regression" });
            return services;
        }

        public RegressionEnvironment(Action<ServiceCollection> configure = null)
        {
            var services = CreateServices();
            configure?.Invoke(services);
            Services = services.BuildServiceProvider();
            Db = new SQLiteDapper(Services);
        }

        public void Dispose()
        {
            Db.Dispose();
            Services.Dispose();
        }
    }
}
