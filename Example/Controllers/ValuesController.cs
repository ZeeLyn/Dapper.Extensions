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
        public ValuesController(IResolveKeyed resolve, [KeyFilter("sqlite1-conn")]IDapper rep1, [KeyFilter("sqlite2-conn")]IDapper rep2)
        {
            SQLiteRepo1 = resolve.ResolveDapper("sqlite1-conn");
            SQLiteRepo2 = resolve.ResolveDapper("sqlite2-conn");

            Repo1 = rep1;
            Repo2 = rep2;
        }
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Repo.BeginTransaction();
            //var result = await Repo.QueryFirstOrDefaultAsync("select * from goods ;");
            //Repo.CommitTransaction();
            //return Ok(result);

            var r1 = await Repo1.QueryAsync("select * from COMPANY LIMIT 1 OFFSET 0");
            var r2 = await Repo2.QueryAsync("select * from COMPANY LIMIT 1 OFFSET 0");
            return Ok(new { r1, r2 });
        }
    }
}
