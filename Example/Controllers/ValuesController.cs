using System;
using Dapper.Extensions;
using Microsoft.AspNetCore.Mvc;
using System.Threading.Tasks;
using System.Globalization;

namespace Example.Controllers
{
    [Route("api/values")]
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

        private IDapper MySql { get; }

        private IDapper MsSql { get; }


        public ValuesController(IResolveContext context, [DependencyDapper("master_slave")] IDapper writer,
            [DependencyDapper("master_slave", true)]
            IDapper reader, [DependencyDapper("mysql-conn")] IDapper mySql,
            [DependencyDapper("mssql-conn")] IDapper msSql)
        {
            MasterReader = reader;
            MasterWriter = writer;
            SQLiteRepo1 = context.ResolveDapper("sqlite1-conn");
            SQLiteRepo2 = context.ResolveDapper("sqlite2-conn");
            MySql = mySql;
            MsSql = msSql;

            //Repo1 = rep1;
            //Repo2 = rep2;

            //SQLRepo = sql;
        }

        //public ValuesController([DependencyDapper("master_slave")]IDapper writer, [DependencyDapper("master_slave", true)]IDapper reader)
        //{
        //    MasterReader = reader;
        //    MasterWriter = writer;
        //}

        // GET api/values

        [HttpGet("MasterSlave")]
        public async Task<IActionResult> MasterSlave()
        {
            var writer = await MasterWriter.QueryAsync("select * from COMPANY;");
            var reader = await MasterReader.QueryAsync("select * from COMPANY;");
            return Ok(new { writer, reader });
        }

        private static T ConvertTo<T>(object value) => value switch
        {
            T typed => typed,
            null or DBNull => default,
            _ => (T)Convert.ChangeType(value, Nullable.GetUnderlyingType(typeof(T)) ?? typeof(T),
                CultureInfo.InvariantCulture),
        };

        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Repo.BeginTransaction();
            //var result = await Repo.QueryFirstOrDefaultAsync("select * from goods ;");
            //Repo.CommitTransaction();
            //return Ok(result);

            //var r1 = await Repo1.QueryAsync<object>("select * from COMPANY where id=1 LIMIT 1 OFFSET 0", enableCache: true, cacheExpire: TimeSpan.FromSeconds(100));
            int pageindex = 1;
            //var page = await MySql.QueryPageAsync<object>("select count(*) from COMPANY;",
            //    "select * from COMPANY limit @Take OFFSET @Skip;", pageindex, 20, enableCache: true,
            //    cacheKey: $"page:{pageindex}");

            var page = await MsSql.QueryPageAsync<object>("select COUNT_BIG(*) from COMPANY;",
                "select * from COMPANY order by id offset @Skip rows fetch next @Take rows only;", pageindex, 20,
                enableCache: true,
                cacheKey: $"page:{pageindex}");
            ////var r2 = await Repo2.QueryAsync("select * from COMPANY where id=2 LIMIT 1 OFFSET 0");
            //return Ok(new
            //{
            //    SQLite = new { r1, page },
            //    //SQL = new
            //    //{
            //    //    page = await SQLRepo.QueryPageAsync("select count(*) from Company;", "select * from Company;", 1, 20)
            //    //}
            //});

            //var con = Repo2.Query("select * from company;");
            //var list = await Repo1.QueryAsync<object>(name: "COMPANY.list.query", new { id = 1 });
            //var sql = Repo1.GetSQL("contact.query");
            //var r = await Repo1.QueryAsync<Contact, Passport, Contact>(sql, (contact, passport) =>
            //{
            //    contact.Passport = passport;
            //    return contact;
            //}, null, "PassportNumber");

            return Ok(new
            {
                page,
                convert = ConvertTo<long>("11")
            });
        }

        //[HttpGet("Masterslave")]
        //public async Task<IActionResult> Masterslave()
        //{
        //    return Ok(new
        //    {
        //        Master = new
        //        {
        //            rows = await MasterWriter.ExecuteAsync("update company set name=@name where id=@id;", new { name = Guid.NewGuid().ToString(), id = 1 }),
        //            MasterWriter.Conn.Value.ConnectionString
        //        },
        //        Slave = new
        //        {
        //            data = await MasterReader.QueryAsync("select * from company;"),
        //            MasterReader.Conn.Value.ConnectionString
        //        }
        //    });
        //}

        [HttpGet("Transaction")]
        public async Task<IActionResult> Transaction()
        {
            Repo1.BeginTransaction();
            var result = await Repo1.QueryFirstOrDefaultAsync("select * from COMPANY where id=1;");
            if (result != null)
            {
                await Repo1.ExecuteAsync("update COMPANY set name=@name where id=1;",
                    new { name = Guid.NewGuid().ToString() });
                Repo1.CommitTransaction();
            }

            return Ok();
        }

        [HttpGet("search")]
        public async Task<IActionResult> Search([FromQuery] string name = "")
        {
            var result = await MasterReader.QueryAsync(
                "select * from company {where name like '%'||@name||'%'};".Splice(!string.IsNullOrWhiteSpace(name)),
                new { name });
            return Ok(result);
        }

        [HttpGet("generateid")]
        public IActionResult GenerateId()
        {
            return Ok(SnowflakeUtils.GenerateId());
        }


        [HttpGet("splice")]
        public IActionResult Splice()
        {
            return Ok("select * from tab where 0=1 {and id=@id} {and type=@type#and status=@status};".Splice('#', true,
                false));
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