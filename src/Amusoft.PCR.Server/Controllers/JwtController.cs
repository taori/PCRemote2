using System.IdentityModel.Tokens.Jwt;
using System.Net;
using System.Net.Mime;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model.Statics;
using Amusoft.PCR.Server.Domain.Authorization;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Formatters;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Primitives;
using Microsoft.Net.Http.Headers;

namespace Amusoft.PCR.Server.Controllers
{
	[Route("[controller]")]
	public class JwtController : Controller
	{
		private readonly IJwtTokenService _jwtTokenService;
		private readonly ILogger<JwtController> _logger;

		public JwtController(IJwtTokenService jwtTokenService, ILogger<JwtController> logger)
		{
			_jwtTokenService = jwtTokenService;
			_logger = logger;
		}

		[Route("[action]")]
		[HttpPost]
		public async Task<ActionResult<JwtAuthenticationResult>> Authenticate([FromBody] JwtLoginCredentials model)
		{
			if (model == null)
				return StatusCode((int) HttpStatusCode.ExpectationFailed);

			_logger.LogInformation("User {Name} is authenticating", model.User);
			var authentication = await _jwtTokenService.CreateAuthenticationAsync(model.User, model.Password);
			if (authentication.InvalidCredentials)
				return StatusCode((int) HttpStatusCode.Unauthorized);

			return Json(authentication);
		}

		[Route("[action]")]
		[HttpPost]
		public async Task<ActionResult<JwtAuthenticationResult>> RefreshToken([FromBody] JwtAuthenticationResult model)
		{
			_logger.LogDebug("Refreshing token for RefreshToken {Token}", model.RefreshToken);
			var authentication = await _jwtTokenService.RefreshAsync(model.AccessToken, model.RefreshToken);
			if (authentication.InvalidCredentials || authentication.AuthenticationRequired)
			{
				return StatusCode((int)HttpStatusCode.Unauthorized);
			}

			return Json(authentication);
		}

		[Route("[action]")]
		[Authorize(Policy = PolicyNames.ApiPolicy)]
		[HttpPost]
		public IActionResult TokenVerify([FromServices] IHttpContextAccessor contextAccessor)
		{
			var state = contextAccessor.HttpContext.User;
			return Content("OK", MediaTypeNames.Text.Plain);
		}
	}
}