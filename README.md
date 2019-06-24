# Dapper.Extensions
A dapper extension library. Support MySql,SQL Server,PostgreSql,SQLite and ODBC.

# Packages & Status
Packages | NuGet
---------|------
Dapper.Extensions.NetCore|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.NetCore)](https://www.nuget.org/packages/Dapper.Extensions.NetCore)
Dapper.Extensions.MySql|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.MySql)](https://www.nuget.org/packages/DDapper.Extensions.MySql)
Dapper.Extensions.PostgreSql|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.PostgreSql)](https://www.nuget.org/packages/Dapper.Extensions.PostgreSql)
Dapper.Extensions.Odbc|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.Odbc)](https://www.nuget.org/packages/Dapper.Extensions.Odbc)
Dapper.Extensions.SQLite|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.SQLite)](https://www.nuget.org/packages/Dapper.Extensions.SQLite)



# Registration database configuration


## For Dependency Injection

Note:Dependency injection only supports a single database. If you need to use multiple databases, use autofac.

```csharp
public void ConfigureServices(IServiceCollection services)
		{

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
			services.AddDapperForSQLite();
		}
```

```csharp
public class ValuesController : ControllerBase
	{
		private IDapper Repo { get;}

		public ValuesController(IDapper repo)
		{
			Repo = repo;
		}

		// GET api/values
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var result = await Repo.QueryAsync("select * from tab;");
			return Ok(result);
		}
	}
```
## For Autofac

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
		{

			services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();

			var builder = new ContainerBuilder();
			builder.Populate(services);

			builder.AddDapperForMSSQL("mssql", "msql-conn");
			builder.AddDapperForSQLite("sqlite1", "sqlite1-conn").AddDapperForSQLite("sqlite2", "sqlite2-conn");

			builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.PropertiesAutowired().InstancePerLifetimeScope();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);
		}
```


```csharp
public class ValuesController : ControllerBase
	{
		private IDapper SQLiteRepo1 { get; }
        private IDapper SQLiteRepo2 { get; }

        public ValuesController(IResolveNamed resolve)
        {
            SQLiteRepo1 = resolve.ResolveDapper("sqlite1-conn");
            SQLiteRepo2 = resolve.ResolveDapper("sqlite2-conn");
        }
		// GET api/values
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var r1 = await SQLiteRepo1.QueryAsync("select * from COMPANY LIMIT 1 OFFSET 0");
            var r2 = await SQLiteRepo2.QueryAsync("select * from COMPANY LIMIT 1 OFFSET 0");
            return Ok(new { r1, r2 });
		}
	}
```
