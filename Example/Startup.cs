using System;
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

            services.AddControllers().AddNewtonsoftJson().AddControllersAsServices();

            SnowflakeUtils.Initialization(1, 1);

            #region Add Dapper
            //services.AddDapperForSQLite();
            //services.AddDapperForPostgreSQL();
            //services.AddDapperForODBC();
            //services.AddDapperForMySQL();
            //services.AddDapperForMSSQL();
            #endregion


            #region Add Caching
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

            services.AddMiniProfiler(options =>
            {
                var storage = new RedisStorage("localhost:6379,password=nihao123#@!")
                {
                    CacheDuration = TimeSpan.FromMinutes(5)
                };
                options.RouteBasePath = "/profiler";
                options.SqlFormatter = new StackExchange.Profiling.SqlFormatters.InlineFormatter();
                options.Storage = storage;
            });
        }

        public void ConfigureContainer(ContainerBuilder builder)
        {
            builder.Register<Func<string, IDapper>>(c =>
            {
                var container = c.Resolve<IComponentContext>();
                return named => container.ResolveNamed<IDapper>(named);
            });

            #region Add Dapper

            builder.AddDapperForMySQL("MySqlConnection", "mysql-conn");

            builder.AddDapperForMSSQL("MSSqlConnection", "msql-conn");

            builder.AddDapperForSQLite("SQLite1Connection", "sqlite1-conn").AddDapperForSQLite("SQLite2Connection", "sqlite2-conn");

            builder.AddMiniProfilerForDapper();

            #endregion

            #region Add Caching

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
