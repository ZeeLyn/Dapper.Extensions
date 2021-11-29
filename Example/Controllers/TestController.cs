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
        public IActionResult Get()
        {

            var r = Dapper.Query("select * from COMPANY where id=1 LIMIT 1 OFFSET 0;");
            return Ok(new { r, DI.Value });
        }

        // GET api/<TestController>/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/<TestController>
        [HttpPost]
        public void Post([FromBody] string value)
        {
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
