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
			var result = await Repo.QueryFirstOrDefaultAsync("select * from h5 ;");

			return Ok(result);
		}
	}
}
