using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Model;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Model.Statics;
using Amusoft.PCR.Server.Data;
using Amusoft.PCR.Server.Dependencies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amusoft.PCR.Server.Authorization
{
	public class PermissionSeedService : BackgroundService
	{
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly IOptions<AuthorizationSettings> _authorizationSettings;
		private readonly ILogger<PermissionSeedService> _logger;
		private readonly ApplicationStateTransmitter _applicationStateTransmitter;

		public PermissionSeedService(IServiceScopeFactory serviceScopeFactory, 
			IOptions<AuthorizationSettings> authorizationSettings, 
			ILogger<PermissionSeedService> logger, 
			ApplicationStateTransmitter applicationStateTransmitter)
		{
			_serviceScopeFactory = serviceScopeFactory;
			_authorizationSettings = authorizationSettings;
			_logger = logger;
			_applicationStateTransmitter = applicationStateTransmitter;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_logger.LogTrace("Waiting for configuration to be done");
			await _applicationStateTransmitter.ConfigurationDone;

			_logger.LogTrace("{Name} running", nameof(PermissionSeedService));

			using (var serviceScope = _serviceScopeFactory.CreateScope())
			{
				using var roleManager = serviceScope.ServiceProvider.GetRequiredService<RoleManager<IdentityRole>>();

				await EnsureRoleExistsAsync(RoleNames.Administrator, roleManager);
				await EnsureRoleExistsAsync(RoleNames.Functions, roleManager);
				await EnsureRoleExistsAsync(RoleNames.Processes, roleManager);
				await EnsureRoleExistsAsync(RoleNames.Computer, roleManager);
				await EnsureRoleExistsAsync(RoleNames.Audio, roleManager);
				await EnsureRoleExistsAsync(RoleNames.ActiveWindow, roleManager);

				_logger.LogTrace("Adding additional roles from appsettings.json");
				if (_authorizationSettings.Value.AdditionalRoles != null)
				{
					foreach (var roleName in _authorizationSettings.Value.AdditionalRoles)
					{
						if (!string.IsNullOrEmpty(roleName))
							await EnsureRoleExistsAsync(roleName, roleManager);
					}
				}

				await EnsureAdminsHavePermissionsAsync(serviceScope.ServiceProvider);
			}

			_logger.LogTrace("{Name} complete", nameof(PermissionSeedService));
		}

		private async Task EnsureAdminsHavePermissionsAsync(IServiceProvider serviceProvider)
		{
			var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();
			var adminUsers = await userManager.Users.Where(d => d.UserType == UserType.Administrator).ToListAsync();
			foreach (var applicationUser in adminUsers)
			{
				if(await userManager.IsInRoleAsync(applicationUser, RoleNames.Administrator))
					continue;

				_logger.LogWarning("User {Name} is missing administrator role - granting permission", applicationUser.UserName);
				await userManager.AddToRoleAsync(applicationUser, RoleNames.Administrator);
			}
		}

		private async Task EnsureRoleExistsAsync(string roleName, RoleManager<IdentityRole> roleManager)
		{
			_logger.LogTrace("Ensuring that permission {Name} exists", roleName);
			var role = await roleManager.FindByNameAsync(roleName);
			if (role == null)
			{
				_logger.LogDebug("Creating role {Role}", roleName);
				await roleManager.CreateAsync(new IdentityRole(roleName));
			}
			else
			{
				_logger.LogDebug("Role {Role} already exists", roleName);
			}
		}
	}
}