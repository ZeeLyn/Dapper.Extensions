using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Extensions;
using Dapper.Extensions.MySql;
using Dapper.Extensions.SQLite;
using Dapper.Extensions.PostgreSql;
using Dapper.Extensions.Odbc;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Autofac.Features.AttributeFilters;
using Dapper.Extensions.Caching.Redis;
using Dapper.Extensions.Caching.Memory;

namespace Example
{
    public class Startup
    {
        public Startup(IConfiguration configuration, IHostingEnvironment env)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }
        public IContainer ApplicationContainer { get; private set; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public IServiceProvider ConfigureServices(IServiceCollection services)
        {

            services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();

            SnowflakeUtils.Initialization(1, 1);

            #region Dependency Injection For Dapper
            //services.AddDapperForSQLite();
            //services.AddDapperForPostgreSQL();
            //services.AddDapperForODBC();
            //services.AddDapperForMySQL();
            //services.AddDapperForMSSQL();
            #endregion


            #region Dependency Injection For Caching
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

            var builder = new ContainerBuilder();
            builder.Populate(services);
            builder.Register<Func<string, IDapper>>(c =>
            {
                var container = c.Resolve<IComponentContext>();
                return named => container.ResolveNamed<IDapper>(named);
            });

            #region Autofac For Dapper

            builder.AddDapperForMySQL("MySqlConnection", "mysql-conn");
            //// OR
            //builder.RegisterType<MySqlDapper>().Named<IDapper>("mysql-conn").WithParameter("connectionName", "mysql").InstancePerLifetimeScope();

            builder.AddDapperForMSSQL("MSSqlConnection", "msql-conn");
            //// OR
            //builder.RegisterType<MsSqlDapper>().Named<IDapper>("msql-conn").WithParameter("connectionName", "mssql").InstancePerLifetimeScope();

            builder.AddDapperForSQLite("SQLite1Connection", "sqlite1-conn").AddDapperForSQLite("SQLite2Connection", "sqlite2-conn");
            //// OR
            //builder.RegisterType<SQLiteDapper>().Named<IDapper>("sqlite-conn").WithParameter("connectionName", "sqlite").InstancePerLifetimeScope();
            #endregion

            #region Autofac For Caching

            builder.AddDapperCachingForRedis(new RedisConfiguration
            {
                AllMethodsEnableCache = false,
                ConnectionString = "localhost:6379,password=nihao123#@!",
                Expire = TimeSpan.FromHours(1)
            });

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
            ApplicationContainer = builder.Build();
            return new AutofacServiceProvider(ApplicationContainer);
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }

            app.UseMvc();
        }
    }
}
