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
		public IDapper Repo { get; set; }

		public ValuesController(Func<string, IDapper> res)
		{
			Repo = res("mysql-conn");
		}
		// GET api/values
		[HttpGet]
		public async Task<IActionResult> Get()
		{
			var result = await Repo.QueryPageAsync("select count(*) from h5", "select * from h5 limit @Skip,@Take;", 1, 10);

			return Ok(result);
		}

		// GET api/values/5
		[HttpGet("{id}")]
		public ActionResult<string> Get(int id)
		{
			return "value";
		}

		// POST api/values
		[HttpPost]
		public void Post([FromBody] string value)
		{
		}

		// PUT api/values/5
		[HttpPut("{id}")]
		public void Put(int id, [FromBody] string value)
		{
		}

		// DELETE api/values/5
		[HttpDelete("{id}")]
		public void Delete(int id)
		{
		}
	}
}
