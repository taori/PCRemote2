using System;
using System.Collections.Generic;
using System.Reflection;
using Amusoft.PCR.Server.Domain.IPC;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.Authorization
{
	public interface IRoleNameProvider
	{
		IEnumerable<string> GetRoleNames();
	}

	public class BackEndRoleProvider : IRoleNameProvider
	{
		private readonly ILogger<BackEndRoleProvider> _log;

		public BackEndRoleProvider(ILogger<BackEndRoleProvider> log)
		{
			_log = log;
		}

		public IEnumerable<string> GetRoleNames()
		{
			var backendType = typeof(BackendIntegrationService);
			var methods = backendType.GetMethods();
			var roleNames = new HashSet<string>();

			foreach (var method in methods)
			{
				var authorizeAttribute = method.GetCustomAttribute<AuthorizeAttribute>();
				if (authorizeAttribute == null)
					continue;

				if(string.IsNullOrEmpty(authorizeAttribute.Roles))
					continue;

				var roles = authorizeAttribute.Roles.Split(',', StringSplitOptions.RemoveEmptyEntries);
				foreach (var role in roles)
				{
					_log.LogDebug("Adding role {RoleName} as declared on method {MethodName}", role, method.Name);
					roleNames.Add(role);
				}
			}

			return roleNames;
		}
	}
}