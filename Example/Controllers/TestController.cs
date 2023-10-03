using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Dapper.Extensions;
using Dapper.Extensions.Monitor;
using System.Linq.Expressions;

// For more information on enabling Web API for empty projects, visit https://go.microsoft.com/fwlink/?LinkID=397860

namespace Example.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class TestController : ControllerBase
    {
        private IDapper Dapper { get; }

        private DITest DI { get; }

        public TestController(IDapper dapper, DITest di)
        {
            Dapper = dapper;
            DI = di;
        }

        // GET: api/<TestController>
        [HttpGet]
        public async Task<IActionResult> Get()
        {
            var r = await Dapper.QueryAsync("select * from COMPANY where id=1 LIMIT 1 OFFSET 0;", enableCache: true,
                forceUpdateCache: false);
            return Ok(new { r, DI.Value });
        }

        // GET api/<TestController>/5
        [HttpGet("{id}")]
        public IActionResult Get(int id)
        {
            Dapper.RemoveCache("758347f54594c0d45e070b95d5220b7f");
            return Ok();
        }

        // POST api/<TestController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
            var sql = "select * from tab where 0=0 {and id=1} {and name='123'}".Splice(true, false);
        }

        // PUT api/<TestController>/5
        [HttpPut("{id}")]
        public void Put(int id, [FromBody] string value)
        {
        }

        // DELETE api/<TestController>/5
        [HttpDelete("{id}")]
        public void Delete(int id)
        {
        }
    }
}