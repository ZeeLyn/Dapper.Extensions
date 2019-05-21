using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions;
using Microsoft.AspNetCore.Mvc;

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ValuesController : ControllerBase
    {
        //private IDapper Repo { get;  }

        private IDapper SQLiteRepo { get; }

        public ValuesController(Func<string, IDapper> res)
        {
            //Repo = res("mysql-conn");
            SQLiteRepo = res("sqlite-conn");
        }
        // GET api/values
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            //Repo.BeginTransaction();
            //var result = await Repo.QueryFirstOrDefaultAsync("select * from goods ;");
            //Repo.CommitTransaction();
            //return Ok(result);

           var r= await SQLiteRepo.QueryAsync("select * from COMPANY LIMIT 1 OFFSET 0");
           return Ok(r);
        }
    }
}
