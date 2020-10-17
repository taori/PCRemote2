using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;

namespace Amusoft.PCR.Server.Controllers
{
	public class TestController : Controller
	{
		// GET
		public async Task<ActionResult<int>> Index()
		{
			await Task.Delay(5000);
			return Ok(42);
		}
	}
}