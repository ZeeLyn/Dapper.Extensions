using System;
using System.IO;
using System.Reflection;

using Autofac;
using Dapper.Extensions;
using Dapper.Extensions.MySql;
using Dapper.Extensions.SQLite;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Autofac.Features.AttributeFilters;
using Dapper.Extensions.Caching.Redis;
using Dapper.Extensions.MiniProfiler;
using Dapper.Extensions.MSSQL;
using Dapper.Extensions.Odbc;
using Dapper.Extensions.PostgreSql;
using Microsoft.Extensions.Hosting;
using StackExchange.Profiling.Storage;
using StackExchange.Redis;
using Autofac.Core;
using Dapper.Extensions.Monitor;
using Dapper.Extensions.Caching.Memory;

namespace Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            //IConfiguration c = new ConfigurationBuilder().Build();
            services.AddControllers().AddNewtonsoftJson().AddControllersAsServices();


            SnowflakeUtils.Initialization(1, 1);

            #region Add Dapper
            services.AddDapperForSQLite(monitorBuilder =>
            {
                monitorBuilder.Threshold = 10;
                monitorBuilder.EnableLog = true;
                monitorBuilder.AddMonitorHandler<MyMonitorHandler>();
            });
            //services.AddDapperForPostgreSQL();
            //services.AddDapperForODBC();
            //services.AddDapperForMySQL();
            //services.AddDapperForMSSQL();
            //services.AddDapperConnectionStringProvider<CustomConnectionStringProvider>();
            #endregion

            //services.AddAspectScope();
            //services.AddScoped<ITest, Test>();
            //services.ConfigureDynamicProxy(configure =>
            //{
            //    configure.Interceptors.AddTyped(typeof(DapperInterceptAttribute));
            //});
            //services.AddScopedInterfaceProxy<ITest>();

            #region Enable Caching
            //services.AddDapperCachingInRedis(new RedisConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    ConnectionString = "localhost:6379,password=nihao123"
            //});

            //services.AddDapperCachingInPartitionRedis(new PartitionRedisConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    Connections = new[] { "localhost:6379,password=nihao123,defaultDatabase=1", "localhost:6379,password=nihao123,defaultDatabase=2" }
            //});

            services.AddDapperCachingInMemory(new MemoryConfiguration
            {
                AllMethodsEnableCache = false,
                Expire = TimeSpan.FromHours(1)
            });

            #endregion

            services.AddMemoryCache();


            //Manage home url:/profiler/results-index
            //Enable MiniProfiler
            //services.AddMiniProfiler(options =>
            //{
            //    options.RouteBasePath = "/profiler";
            //    options.Storage = new RedisStorage("127.0.0.1:6379,password=nihao123")
            //    {
            //        CacheDuration = TimeSpan.FromMinutes(5)
            //    };
            //});
            services.AddScoped<DITest>();
            //services.BuildDynamicProxyProvider();
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            #region Add Dapper
            //builder.AddDapperForMySQL();

            //builder.AddDapperForMySQL("MySqlConnection", "mysql-conn");

            //builder.AddDapperForMSSQL("MSSqlConnection", "msql-conn");

            //builder.AddDapperForSQLite("SQLite1Connection", "sqlite1-conn");

            //builder.AddDapperForSQLite("SQLite2Connection", "sqlite2-conn");

            //builder.AddDapperForSQLite("master_slave", "master_slave", true, monitorBuilder =>
            //{
            //    monitorBuilder.Threshold = 0;
            //    monitorBuilder.EnableLog = true;
            //    monitorBuilder.AddMonitorHandler<MyMonitorHandler>();
            //});

            //builder.AddDapperForSQLite("SQLite1Connection", monitorBuilder: monitorBuilder =>
            // {
            //     monitorBuilder.Threshold = 0;
            //     monitorBuilder.EnableLog = true;
            //     monitorBuilder.AddMonitorHandler<MyMonitorHandler>();
            // });

            //Add support for MiniProfiler
            //builder.AddMiniProfilerForDapper();

            //builder.AddSQLSeparationForDapper(Path.Combine(Directory.GetCurrentDirectory(), "sql"));
            //builder.AddDapperConnectionStringProvider<CustomConnectionStringProvider>();

            #endregion

            #region Enable Caching

            //builder.AddDapperCachingForRedis(new RedisConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    ConnectionString = "127.0.0.1:6379,password=nihao123",
            //    Expire = TimeSpan.FromHours(1)
            //});

            //builder.AddDapperCachingInPartitionRedis(new PartitionRedisConfiguration
            //{
            //    Connections = new[] { "localhost:6379,password=nihao123#@!,defaultDatabase=1", "localhost:6379,password=nihao123#@!,defaultDatabase=2" }
            //});

            //builder.AddDapperCachingInMemory(new MemoryConfiguration
            //{
            //    AllMethodsEnableCache = false
            //});

            #endregion

            builder.AddAllControllers();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMiniProfiler();
            app.UseRouting();
            app.UseStaticFiles();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
