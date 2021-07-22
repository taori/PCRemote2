using System.Threading.Tasks;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Model.Statics;
using Microsoft.AspNetCore.Authorization;

namespace Amusoft.PCR.Server.Domain.Authorization
{
	public class HostCommandAuthorizationHandler : AuthorizationHandler<HostCommandPermissionRequirement, HostCommand>
	{
		protected override Task HandleRequirementAsync(AuthorizationHandlerContext context, HostCommandPermissionRequirement requirement,
			HostCommand resource)
		{
			if (requirement != null)
			{
				if (context.User.IsInRole(RoleNames.Administrator))
				{
					context.Succeed(requirement);
					return Task.CompletedTask;
				}

				if (context.User.HasClaim(d => d.Type == PermissionClaimNames.ApplicationPermissionClaim
				                               && d.Value == resource.Id 
				                               && d.ValueType == ((int) PermissionKind.HostCommand).ToString()))
				{
					context.Succeed(requirement);
					return Task.CompletedTask;
				}
			}

			return Task.CompletedTask;
		}
	}
}