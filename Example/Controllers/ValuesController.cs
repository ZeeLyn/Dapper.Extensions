using System;
using Autofac.Features.AttributeFilters;
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
        public ValuesController(IResolveKeyed resolve, [KeyFilter("sqlite1-conn")]IDapper rep1, [KeyFilter("sqlite2-conn")]IDapper rep2, [KeyFilter("msql-conn")]IDapper sql)
        {
            SQLiteRepo1 = resolve.ResolveDapper("sqlite1-conn");
            SQLiteRepo2 = resolve.ResolveDapper("sqlite2-conn");

            Repo1 = rep1;
            Repo2 = rep2;

            SQLRepo = sql;
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
            //var page = await Repo1.QueryPageAsync<object>("select count(*) from COMPANY;", "select * from COMPANY limit @Take OFFSET @Skip;", pageindex, 20, enableCache: true, cacheKey: $"page:{pageindex}");
            ////var r2 = await Repo2.QueryAsync("select * from COMPANY where id=2 LIMIT 1 OFFSET 0");
            //return Ok(new
            //{
            //    SQLite = new { r1, page },
            //    SQL = new
            //    {
            //        page = await SQLRepo.QueryPageAsync("select count(*) from Company;", "select * from Company;", 1, 20)
            //    }
            //});

            var r = await Repo1.QueryAsync<Contact, Passport, Contact>("select Contact.id,Contact.name,Passport.PassportNumber from Contact,Passport where Passport.ContactId=Contact.id;", (contact, passport) =>
            {
                contact.Passport = passport;
                return contact;
            }, null, "PassportNumber");
            return Ok(r);
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
