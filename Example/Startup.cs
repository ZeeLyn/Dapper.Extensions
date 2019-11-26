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
            //services.AddDapperForSQLite();
            //services.AddDapperForPostgreSQL();
            //services.AddDapperForODBC();
            //services.AddDapperForMySQL();
            //services.AddDapperForMSSQL();
            #endregion


            #region Enable Caching
            //services.AddDapperCachingInRedis(new RedisConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    ConnectionString = "localhost:6379,password=nihao123#@!"
            //});

            //services.AddDapperCachingInPartitionRedis(new PartitionRedisConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    Connections = new[] { "localhost:6379,password=nihao123#@!,defaultDatabase=1", "localhost:6379,password=nihao123#@!,defaultDatabase=2" }
            //});

            //services.AddDapperCachingInMemory(new MemoryConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    Expire = TimeSpan.FromHours(1)
            //});

            #endregion

            services.AddMemoryCache();
            //Enable MiniProfiler
            services.AddMiniProfiler(options =>
            {
                options.RouteBasePath = "/profiler";
                options.Storage = new RedisStorage("localhost:6379,password=nihao123#@!")
                {
                    CacheDuration = TimeSpan.FromMinutes(5)
                };
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            #region Add Dapper


            builder.AddDapperForMySQL("MySqlConnection", "mysql-conn");

            builder.AddDapperForMSSQL("MSSqlConnection", "msql-conn");

            builder.AddDapperForSQLite("SQLite1Connection", "sqlite1-conn");

            builder.AddDapperForSQLite("SQLite2Connection", "sqlite2-conn");

            builder.AddDapperForSQLite("master_slave", "master_slave", true);

            //Add support for MiniProfiler
            builder.AddMiniProfilerForDapper();

            builder.AddSQLSeparationForDapper(Path.Combine(Directory.GetCurrentDirectory(), "sql"));

            #endregion

            #region Enable Caching

            //builder.AddDapperCachingForRedis(new RedisConfiguration
            //{
            //    AllMethodsEnableCache = false,
            //    ConnectionString = "localhost:6379,password=nihao123#@!",
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

            builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
                .Where(t => t.Name.EndsWith("Controller"))
                .PropertiesAutowired().WithAttributeFiltering().InstancePerLifetimeScope();
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
