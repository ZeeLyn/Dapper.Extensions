using Dapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;

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

        private IDapper MasterReader { get; }

        private IDapper MasterWriter { get; }


        public ValuesController(IResolveContext context, [DependencyDapper("master_slave")]IDapper writer, [DependencyDapper("master_slave", true)]IDapper reader, [DependencyDapper("sqlite1-conn")]IDapper rep1, [DependencyDapper("sqlite2-conn")]IDapper rep2, [DependencyDapper("msql-conn")]IDapper sql)
        {
            MasterReader = reader;
            MasterWriter = writer;
            SQLiteRepo1 = context.ResolveDapper("sqlite1-conn");
            SQLiteRepo2 = context.ResolveDapper("sqlite2-conn");

            Repo1 = rep1;
            Repo2 = rep2;

            SQLRepo = sql;
        }

        //public ValuesController([DependencyDapper]IDapper writer, [DependencyDapper(true)]IDapper reader)
        //{
        //    MasterReader = reader;
        //    MasterWriter = writer;
        //}

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

        }

        [HttpGet("Masterslave")]
        public async Task<IActionResult> Masterslave()
        {
            return Ok(new
            {
                Masert = new
                {
                    data = await MasterWriter.QueryAsync("select * from company;"),
                    MasterWriter.Conn.Value.ConnectionString
                },
                Slave = new
                {
                    data = await MasterReader.QueryAsync("select * from company;"),
                    MasterReader.Conn.Value.ConnectionString
                }
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
