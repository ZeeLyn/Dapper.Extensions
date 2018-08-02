# Dapper.Extensions

```cssharp
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
			builder.RegisterType<MySqlDapper>().Named<IDapper>("mysql-conn").WithParameter("connectionName", "mysql").InstancePerLifetimeScope();
			builder.RegisterType<MsSqlDapper>().Named<IDapper>("msql-conn").WithParameter("connectionName", "mssql").InstancePerLifetimeScope();
			builder.RegisterAssemblyTypes(Assembly.GetEntryAssembly())
				.Where(t => t.Name.EndsWith("Controller"))
				.PropertiesAutowired().InstancePerLifetimeScope();
			ApplicationContainer = builder.Build();
			return new AutofacServiceProvider(ApplicationContainer);
		}
```


```cssharp
public class ValuesController : ControllerBase
	{
		public IDapper Repo { get; set; }

		public ValuesController(Func<string, IDapper> res)
		{
			Repo = res("mysql-conn");
		}
		// GET api/values
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var result = await Repo.QueryPageAsync("select count(*) from tab", "select * from tab limit @Skip,@Take;", 1, 10);

			return Ok(result);
		}
	}
```
