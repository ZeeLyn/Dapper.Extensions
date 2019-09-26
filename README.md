# Dapper.Extensions
A dapper extension library. Support MySql,SQL Server,PostgreSql,SQLite and ODBC,  Support cache.

# Packages & Status
Packages | NuGet
---------|------
Dapper.Extensions.NetCore|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.NetCore)](https://www.nuget.org/packages/Dapper.Extensions.NetCore)
Dapper.Extensions.MySql|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.MySql)](https://www.nuget.org/packages/DDapper.Extensions.MySql)
Dapper.Extensions.MSSQL|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.MSSQL)](https://www.nuget.org/packages/DDapper.Extensions.MSSQL)
Dapper.Extensions.PostgreSql|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.PostgreSql)](https://www.nuget.org/packages/Dapper.Extensions.PostgreSql)
Dapper.Extensions.Odbc|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.Odbc)](https://www.nuget.org/packages/Dapper.Extensions.Odbc)
Dapper.Extensions.SQLite|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.SQLite)](https://www.nuget.org/packages/Dapper.Extensions.SQLite)
Dapper.Extensions.Caching.Redis|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.Caching.Redis)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Redis)
Dapper.Extensions.Caching.Memory|[![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.Caching.Memory)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Memory)

# Database connection string configuration

The default connection name is 'DefaultConnection'

```json
{
	"ConnectionStrings": {
		"DefaultConnection": "Data Source=localhost;port=3306;Pooling=true;Initial Catalog=ShopDB;User Id=root;Password=123456;SslMode=none;",
		"MySqlConnection": "Data Source=localhost;port=3306;Pooling=true;Initial Catalog=ShopDB;User Id=root;Password=123456;SslMode=none;",
		"SQLite1Connection": "data source=/data/test1.sqlite",
		"SQLite2Connection": "data source=/data/test2.sqlite"
	}
}
```

# Use Dependency Injection

Note:Dependency injection only supports a single database and the default connection name is 'DefaultConnection'. If you need to use multiple databases, use autofac.

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
# Use Autofac

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{

	services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();

	var builder = new ContainerBuilder();
	builder.Populate(services);

	builder.AddDapperForMSSQL("MySqlConnection", "msql-conn");
	builder.AddDapperForSQLite("SQLite1Connection", "sqlite1-conn").AddDapperForSQLite("SQLite2Connection", "sqlite2-conn");

	builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
		.Where(t => t.Name.EndsWith("Controller"))
		.PropertiesAutowired().InstancePerLifetimeScope();
	ApplicationContainer = builder.Build();
	return new AutofacServiceProvider(ApplicationContainer);
}
```

#### Inject objects with IResolveKeyed
```csharp
public class ValuesController : ControllerBase
{
	private IDapper SQLiteRepo1 { get; }
	private IDapper SQLiteRepo2 { get; }

	public ValuesController(IResolveKeyed resolve)
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
#### Filter injection using KeyFilterAttribute
Note:If you’re using metadata filters (KeyFilterAttribute or WithAttributeFiltering in your constructors), you need to register those components using the [WithAttributeFiltering](https://autofaccn.readthedocs.io/en/latest/advanced/metadata.html) extension. Note that if you’re only using filters but not attributed metadata, you don’t actually need the AttributedMetadataModule. Metadata filters stand on their own.

```csharp
public class ValuesController : ControllerBase
{
	private IDapper Repo1 { get; }

	private IDapper Repo2 { get; }

	public ValuesController([KeyFilter("sqlite1-conn")]IDapper rep1, [KeyFilter("sqlite2-conn")]IDapper rep2)
	{
		Repo1 = rep1;
		Repo2 = rep2;
	}
	// GET api/values
	[HttpGet]
	public async Task<IActionResult> Get()
	{
		var r1 = await Repo1.QueryAsync("select * from COMPANY;");
		var r2 = await Repo3.QueryAsync("select * from COMPANY;");
		return Ok(new { r1, r2 });
	}
}
```

# About paging query
 The paging method has four SQL variables built in: @Skip, @Take, @TakeStart, @TakeEnd. 

 ### MySQL usage example
 ```SQL
 select * from tab order by id desc limit @Skip, @Take; 
 ```

 ### MSSQL usage example
 
 SQL Server 2005
 ```SQL
 select * from (select ROW_NUMBER() over(order by id desc) as row_num,id,title from tab) tab1 where row_num between @TakeStart and @TakeEnd;
 ```

 SQL Server 2012
 ```SQL
 select * from tab offset @Skip rows fetch next @Take rows only;
 ```


# Support for caching

### In redis

```csharp
public void ConfigureServices(IServiceCollection services)
{

	services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
	
	services.AddDapperCachingInRedis(new RedisConfiguration
	{
		AllMethodsEnableCache = false,
		ConnectionString = "localhost:6379,password=nihao123#@!"
	});

	//Redis partition mode
	//services.AddDapperCachingInPartitionRedis(new PartitionRedisConfiguration
	//{
		//AllMethodsEnableCache = false,
		//Connections = new[] { "localhost:6379,password=nihao123#@!,defaultDatabase=1", "localhost:6379,password=nihao123#@!,defaultDatabase=2" }
	//});
}
```

### In Memory
```csharp
public void ConfigureServices(IServiceCollection services)
{

	services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
	
	services.AddDapperCachingInMemory(new RedisConfiguration
	{
		AllMethodsEnableCache = false
	});

}
```

### Recommended usage
```csharp
public async Task<IActionResult> Get()
{
	int pageindex = 1;
	var page = await Repo1.QueryPageAsync<object>("select count(*) from COMPANY;", "select * from COMPANY limit @Take OFFSET @Skip;", pageindex, 20, enableCache: true, cacheKey: $"page:{pageindex}", cacheExpire: TimeSpan.FromSeconds(100));
	return Ok(page);
}
```


# Built-in global unique id generator(Snowflake)
```csharp

// Initialization
public void ConfigureServices(IServiceCollection services)
{
	services.AddMvc().SetCompatibilityVersion(CompatibilityVersion.Version_2_1).AddControllersAsServices();
	SnowflakeUtils.Initialization(1, 1);
}

// generate
public IActionResult GenerateId()
{
	return Ok(SnowflakeUtils.GenerateId());
}
```