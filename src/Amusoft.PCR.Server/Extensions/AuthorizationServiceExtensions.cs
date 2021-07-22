using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization.Infrastructure;

namespace Amusoft.PCR.Server.Extensions
{
	public static class AuthorizationServiceExtensions
	{
		public static async Task<bool> IsInRoleAsync(this IAuthorizationService source, ClaimsPrincipal user, params string[] allowedRoles)
		{
			var state = await source.AuthorizeAsync(user, null, new RolesAuthorizationRequirement(allowedRoles));
			return state.Succeeded;
		}
	}
}