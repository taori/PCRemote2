using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Amusoft.PCR.Server.Authorization
{
	public class RoleOrAdminAuthorizationHandler : IAuthorizationHandler
	{
		public Task HandleAsync(AuthorizationHandlerContext context)
		{
			var rolesAuthorizationRequirement = context
				.Requirements
				.OfType<RolesAuthorizationRequirement>()
				.FirstOrDefault();

			if (rolesAuthorizationRequirement != null)
			{
				if (context.User.IsInRole("Administrator"))
					context.Succeed(rolesAuthorizationRequirement);
			}

			return Task.CompletedTask;
		}
	}
}