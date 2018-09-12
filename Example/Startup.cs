using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using AppSettings.Loader.MVC;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Dapper.Extensions;
using Dapper.Extensions.MySql;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

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

			var builder = new ContainerBuilder();
			builder.Populate(services);
			builder.Register<Func<string, IDapper>>(c =>
			{
				var container = c.Resolve<IComponentContext>();
				return named => container.ResolveNamed<IDapper>(named);
			});
			builder.RegisterType<MySqlDapper>().Named<IDapper>("mysql-conn").WithParameter("connectionName", "mysql").PropertiesAutowired().InstancePerLifetimeScope();
			builder.RegisterType<MsSqlDapper>().Named<IDapper>("msql-conn").WithParameter("connectionName", "mssql").PropertiesAutowired().InstancePerLifetimeScope();
			builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.PropertiesAutowired().InstancePerLifetimeScope();
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
