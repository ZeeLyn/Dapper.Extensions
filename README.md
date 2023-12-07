---
---

**Note: There is a dependency injection [bug](https://github.com/aspnet/Extensions/issues/2431) when upgrading netcore 3.0. You can temporarily use the autofac container, waiting to be fixed.**

---

---

# Dapper.Extensions

A dapper extension library.

1.Support MySQL,SQL Server,PostgreSQL,SQLite,Oracle and ODBC.

2.Support cache.

3.Support sql separation.

4.Support reading and writing separation.

5.Support performance monitoring.

# Packages & Status

| Packages                         | NuGet                                                                                                                                               |Note|
| -------------------------------- | --------------------------------------------------------------------------------------------------------------------------------------------------- |---|
| Dapper.Extensions.NetCore        | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.NetCore)](https://www.nuget.org/packages/Dapper.Extensions.NetCore)               ||
| Dapper.Extensions.MySQL          | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.MySQL)](https://www.nuget.org/packages/Dapper.Extensions.MySQL)                   ||
| Dapper.Extensions.MSSQL          | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.MSSQL)](https://www.nuget.org/packages/Dapper.Extensions.MSSQL)                   ||
| Dapper.Extensions.PostgreSQL     | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.PostgreSQL)](https://www.nuget.org/packages/Dapper.Extensions.PostgreSQL)         ||
| Dapper.Extensions.ODBC           | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.ODBC)](https://www.nuget.org/packages/Dapper.Extensions.ODBC)                     ||
| Dapper.Extensions.SQLite         | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.SQLite)](https://www.nuget.org/packages/Dapper.Extensions.SQLite)                 ||
| ZeeLyn.Dapper.Extensions.Oracle         |<div style="width:220px;"> [![NuGet package](https://buildstats.info/nuget/ZeeLyn.Dapper.Extensions.Oracle)](https://www.nuget.org/packages/ZeeLyn.Dapper.Extensions.Oracle)   </div>              |Nuget does not allow me to use Dapper.Extensions as the package ID of the prefix, so ......|
| Dapper.Extensions.Caching.Redis  | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.Caching.Redis)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Redis)   ||
| Dapper.Extensions.Caching.Memory | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.Caching.Memory)](https://www.nuget.org/packages/Dapper.Extensions.Caching.Memory) ||
| Dapper.Extensions.MiniProfiler   | [![NuGet package](https://buildstats.info/nuget/Dapper.Extensions.MiniProfiler)](https://www.nuget.org/packages/Dapper.Extensions.MiniProfiler)     ||

# Database connection string configuration

The default connection name is 'DefaultConnection'

```json
{
    "ConnectionStrings": {
        "DefaultConnection": "Data Source=localhost;port=3306;Pooling=true;Initial Catalog=ShopDB;User Id=root;Password=123456;SslMode=none;",
        "MySqlConnection": "Data Source=localhost;port=3306;Pooling=true;Initial Catalog=ShopDB;User Id=root;Password=123456;SslMode=none;",
        "SQLite1Connection": "data source=db//test1.db",
        "SQLite2Connection": "data source=db//test2.db",
        "master_slave": {
            "Master": "data source=db//test.master.db",
            "Slaves": [
                {
                    "ConnectionString": "data source=db//test1.db",
                    "Weight": 4
                },
                {
                    "ConnectionString": "data source=db//test2.db",
                    "Weight": 6
                }
            ]
        }
    }
}
```
If you need to customize the source of the database connection string, you can implement the IConnectionStringProvider interface.
```csharp

public class CustomConnectionStringProvider : IConnectionStringProvider
{
	public string GetConnectionString(string connectionName, bool enableMasterSlave = false, bool readOnly = false)
	{
		throw new NotImplementedException();
	}
}

```

Register custom provider

```csharp
services.AddDapperConnectionStringProvider<CustomConnectionStringProvider>();
```


# Using Dependency Injection

Note:Dependency injection only supports a single database and the default connection name is 'DefaultConnection'. If you need to use multiple databases, use autofac.

```csharp
public void ConfigureServices(IServiceCollection services)
{
	//For Sqlite
	services.AddDapperForSQLite();

	//For sql server
 	services.AddDapperForMSSQL();

	//For MySql
 	services.AddDapperForMySQL();

	//For ODBC
 	services.AddDapperForODBC();
	
	//For Postgre
 	services.AddDapperForPostgreSQL();

	//For Oracle
 	services.AddDapperForOracle();

}
```

```csharp
public class ValuesController : ControllerBase
{
	private IDapper Dapper { get;}

	public ValuesController(IDapper dapper)
	{
    		Dapper = dapper;
	}

	// GET api/values
	[HttpGet]
	public async Task<IActionResult> Get()
	{
  		var result = await Dapper.QueryAsync("select * from tab;");
		return Ok(result);
	}
}
```

# Using Autofac

```csharp
public IServiceProvider ConfigureServices(IServiceCollection services)
{
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

#### Inject objects with IResolveContext

```csharp
public class ValuesController : ControllerBase
{
	private IDapper SQLiteRepo1 { get; }
	private IDapper SQLiteRepo2 { get; }

	public ValuesController(IResolveContext context)
	{
		SQLiteRepo1 = context.ResolveDapper("sqlite1-conn");
		SQLiteRepo2 = context.ResolveDapper("sqlite2-conn");
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

#### Filter injection using DependencyDapperAttribute

Note:If you’re using metadata filters (DependencyDapperAttribute or WithAttributeFiltering in your constructors), you need to register those components using the [WithAttributeFiltering](https://autofaccn.readthedocs.io/en/latest/advanced/metadata.html) extension. Note that if you’re only using filters but not attributed metadata, you don’t actually need the AttributedMetadataModule. Metadata filters stand on their own.

```csharp
public class ValuesController : ControllerBase
{
	private IDapper Repo1 { get; }

	private IDapper Repo2 { get; }

	public ValuesController([DependencyDapper("sqlite1-conn")]IDapper rep1, [DependencyDapper("sqlite2-conn")]IDapper rep2)
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

# Support for console application

```csharp
static void Main(string[] args)
{
	//registration
	DapperFactory.CreateInstance().ConfigureServices(service =>
	{
		service.AddDapperForSQLite();
	}).ConfigureContainer(container =>
	{
		container.AddDapperForSQLite("Sqlite2", "sqlite2");
	}).ConfigureConfiguration(builder =>
	{
		builder.SetBasePath(Directory.GetCurrentDirectory());
		builder.AddJsonFile("appsettings.json");
	}).Build();

	//query database
	DapperFactory.Step(dapper =>
	{
		var query = dapper.Query("select * from Contact;");
		Console.WriteLine(JsonConvert.SerializeObject(query));
	});
}
```

# Support for sql separation

Like mybatis, but does not support Dynamic SQL. Modify the xml file to take effect immediately, no need to restart the application.

```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddSQLSeparationForDapper(Path.Combine(Directory.GetCurrentDirectory(), "sql"));
}
```

```xml
<?xml version="1.0" encoding="utf-8" ?>
<sql-set>
  <sql name="COMPANY.list.query"><![CDATA[select * from COMPANY where id=@id;]]></sql>
  <paging-sql name="COMPANY.paging">
    <count>select count(*) from COMPANY;</count>
    <query>select * from COMPANY limit @Skip,@Take;</query>
  </paging-sql>
</sql-set>
```

The name must be globally unique.

```csharp
var list = await Repo1.QueryAsync<Company>(name: "COMPANY.list.query",new{ id=1 });
var page = await Repo1.QueryPageAsync<Company>(name: "COMPANY.paging", 1,20 );
```

# Caching

### In redis

```csharp
public void ConfigureServices(IServiceCollection services)
{
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
	services.AddDapperCachingInMemory(new RedisConfiguration
	{
		AllMethodsEnableCache = false
	});

}
```

### Recommended usage

It is recommended to use a custom cache key, because the built-in key generator is based on all parameters MD5 hash, which affects performance.

```csharp
public async Task<IActionResult> Get()
{
	int pageindex = 1;
	var page = await Repo1.QueryPageAsync<object>("select count(*) from COMPANY;", "select * from COMPANY limit @Take OFFSET @Skip;", pageindex, 20, enableCache: true, cacheKey: $"page:{pageindex}", cacheExpire: TimeSpan.FromSeconds(100));
	return Ok(page);
}
```

---

### **_WARNING_**

**_If the cache does not exist, it adds a lock when reading data from the database to prevent cache breakdown.If the lock cannot be acquired, a DapperCacheException will be thrown, which needs to be caught in your business code. At this time, you need to consider the issues of Cache Penetration and Cache Breakdown and Cache Avalanche. Dapper.Extensions is just a low-level data access library. Preventing malicious attacks is not its responsibility. You can add Bloom filters before this._**

---

# Support for MiniProfiler

Dapper.Extensions.MiniProfiler just adds support for MiniProfiler. To enable MiniProfiler, you need to configure it yourself. Please check the [documentation](https://miniprofiler.com/dotnet/).

```csharp
public void ConfigureServices(IServiceCollection services)
{
	services.AddMiniProfilerForDapper();
}
```

# Support performance monitoring

```csharp
services.AddDapperForSQLite(monitorBuilder =>
{
	monitorBuilder.Threshold = 200;
	monitorBuilder.EnableLog = true;
	monitorBuilder.AddMonitorHandler<MyMonitorHandler>();
});
```

| Option            | Description                                                                                                                              |
| ----------------- | ---------------------------------------------------------------------------------------------------------------------------------------- |
| Threshold         | Slow SQL command execution time critical value, greater than this value will trigger the monitoring event, default 200, unit millisecond |
| EnableLog         | Write the log when there is a slow query                                                                                                 |
| AddMonitorHandler | Add a custom processor                                                                                                                   |

# Read and write separation

To use read and write separation, you must use autofac injection.

```json
{
    "ConnectionStrings": {
        "master_slave": {
            "Master": "data source=db//test.master.db",
            "Slaves": [
                {
                    "ConnectionString": "data source=db//test1.db",
                    "Weight": 4
                },
                {
                    "ConnectionString": "data source=db//test2.db",
                    "Weight": 6
                }
            ]
        }
    }
}
```

### enableMasterSlave: Enable read and write separation.

```csharp
public void ConfigureContainer(ContainerBuilder builder)
{
	builder.AddDapperForSQLite("master_slave", "master_slave", enableMasterSlave:true);
}
```

### readOnly: Access to the slave database(s), using weighted polling by default.

```csharp
public class ValuesController : ControllerBase
{
    private IDapper Writer { get; }

    private IDapper Reader { get; }

    public ValuesController([DependencyDapper("master_slave")]IDapper writer, [DependencyDapper("master_slave",readOnly:true)]IDapper reader)
    {
        Writer = writer;
        Reader = reader;
    }
    [HttpGet]
    public async Task<IActionResult> Get()
    {
        await writer.ExecuteAsync("delete * from COMPANY;");
        var result = await reader.QueryAsync("select * from COMPANY;");
        return Ok(result);
    }
}
```

# Splicing sql strings

Similar to the usage of string.format(), the content in {} is determined by the parameters; it supports ‘else’, {sql1: sql2}, if the parameter is true, use sql1, otherwise use sql2.

Example:

```csharp
var id=1;
var sql="select * from tab where 1=1 {and id=@id} {and status=0} and {r1=1:r2=2} and {t1=1:t2=2};".Splice(id>0,false, true, false);
```

Output:

```sql
select * from tab where 1=1 and id=@id  and r1=1 and t2=2;
```

Extended market installation extension plug-in(SQLSpliceHighlight) highlights

![](https://raw.githubusercontent.com/1100100/Dapper.Extensions/master/assets/vsix.gif)

# Built-in global unique id generator(Snowflake)

```csharp

// Initialization
public void ConfigureServices(IServiceCollection services)
{
	SnowflakeUtils.Initialization(1, 1);
}

// generate
public IActionResult GenerateId()
{
	return Ok(SnowflakeUtils.GenerateId());
}
```
