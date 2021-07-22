using System.Linq;
using System.Threading.Tasks;
using Amusoft.PCR.Model.Statics;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Amusoft.PCR.Server.Domain.Authorization
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
				if (context.User.IsInRole(RoleNames.Administrator))
				{
					context.Succeed(rolesAuthorizationRequirement);
					return Task.CompletedTask;
				}
			}

			return Task.CompletedTask;
		}
	}
}