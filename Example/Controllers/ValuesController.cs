using System.Collections.Concurrent;
using Dapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using Dapper.Extensions.MasterSlave;
using Microsoft.Extensions.Configuration;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        private IDapper Repo1 { get; }

        private IDapper Repo2 { get; }

        private IDapper SQLiteRepo1 { get; }

        private IDapper SQLiteRepo2 { get; }

        private IDapper SQLRepo { get; }

        private IDapper MasterSlave { get; }

        private IConfiguration Configuration { get; }

        private ILoadBalancing LoadBalancing { get; }

        public ValuesController(IResolveKeyed resolve, [Dependency("master_slave", true)]IDapper masterSlave, [Dependency("sqlite1-conn")]IDapper rep1, [Dependency("sqlite2-conn")]IDapper rep2, [Dependency("msql-conn")]IDapper sql, IConfiguration configuration, ILoadBalancing loadBalancing)
        {
            MasterSlave = masterSlave;
            LoadBalancing = loadBalancing;
            SQLiteRepo1 = resolve.ResolveDapper("sqlite1-conn");
            SQLiteRepo2 = resolve.ResolveDapper("sqlite2-conn");

            Repo1 = rep1;
            Repo2 = rep2;

            SQLRepo = sql;
            Configuration = configuration;
        }
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Repo.BeginTransaction();
            //var result = await Repo.QueryFirstOrDefaultAsync("select * from goods ;");
            //Repo.CommitTransaction();
            //return Ok(result);

            //var r1 = await Repo1.QueryAsync<object>("select * from COMPANY where id=1 LIMIT 1 OFFSET 0", enableCache: true, cacheExpire: TimeSpan.FromSeconds(100));
            //int pageindex = 1;
            //var page = await Repo2.QueryPageAsync<object>("select count(*) from COMPANY;", "select * from COMPANY limit @Take OFFSET @Skip;", pageindex, 20, enableCache: true, cacheKey: $"page:{pageindex}");
            ////var r2 = await Repo2.QueryAsync("select * from COMPANY where id=2 LIMIT 1 OFFSET 0");
            //return Ok(new
            //{
            //    SQLite = new { r1, page },
            //    //SQL = new
            //    //{
            //    //    page = await SQLRepo.QueryPageAsync("select count(*) from Company;", "select * from Company;", 1, 20)
            //    //}
            //});

            var con = Repo2.Query("select * from company;");
            var list = await Repo1.QueryAsync<object>(name: "COMPANY.list.query", new { id = 1 });
            var sql = Repo1.GetSQL("contact.query");
            var r = await Repo1.QueryAsync<Contact, Passport, Contact>(sql, (contact, passport) =>
            {
                contact.Passport = passport;
                return contact;
            }, null, "PassportNumber");

            return Ok(new { r, list });

            //ConcurrentDictionary<string, int> dic = new ConcurrentDictionary<string, int>();
            //var c = Configuration.GetSection("ConnectionStrings:master_slave").Get<ConnectionConfiguration>();
            //for (var i = 0; i < 600; i++)
            //{
            //    dic.AddOrUpdate(LoadBalancing.NextConnectionString(c.Slaves), 1, (key, old) => old + 1);
            //}

            //return Ok(dic);
        }

        [HttpGet("Masterslave")]
        public async Task<IActionResult> Masterslave()
        {
            return Ok(new
            {
                data = await MasterSlave.QueryAsync("select * from company;"),
                MasterSlave.Conn.Value.ConnectionString
            });
        }

        [HttpGet("Transaction")]
        public async Task<IActionResult> Transaction()
        {
            using (Repo1.BeginTransaction())
            {
                var result = await Repo1.QueryFirstOrDefaultAsync("select * from COMPANY where id=1;");
                if (result != null)
                {
                    await Repo1.ExecuteAsync("update COMPANY set name=@name where id=1;", new { name = "updated" });
                    Repo1.CommitTransaction();
                }
                return Ok();
            }

        }

        [HttpGet("generateid")]
        public IActionResult GenerateId()
        {
            return Ok(SnowflakeUtils.GenerateId());
        }

    }
    public class Contact
    {
        public int Id { get; set; }

        public string Name { get; set; }

        public Passport Passport { get; set; }
    }

    public class Passport
    {
        public string PassportNumber { get; set; }
    }
}
