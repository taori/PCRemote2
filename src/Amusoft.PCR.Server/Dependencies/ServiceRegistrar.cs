using System;
using System.Net.Http;
using Amusoft.PCR.Blazor.Services;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Server.Areas.Identity;
using Amusoft.PCR.Server.Domain.Authorization;
using Amusoft.PCR.Server.Domain.IPC;
using Grpc.Net.Client;
using GrpcDotNetNamedPipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Amusoft.PCR.Server.Dependencies
{
	public static class ServiceRegistrar
	{
		public static void Register(IServiceCollection collection)
		{
			collection.AddSingleton<ClassLoader>();
			collection.AddSingleton<IValidationService, ValidationService>();
			collection.AddSingleton<NamedPipeChannel>(CreateConfiguredNamedPipeChannel());
			collection.AddSingleton<IInteropService, InteropService>();
			collection.AddSingleton<IIntegrationApplicationLocator, IntegrationApplicationLocator>();
			collection.AddSingleton<IAuthorizationHandler, RoleOrAdminAuthorizationHandler>();
			collection.AddSingleton<ApplicationStateTransmitter>(); 
			collection.AddSingleton<IRoleNameProvider, BackEndRoleProvider>(); 
			collection.AddSingleton<IRoleNameProvider, DefaultRoleProvider>(); 

			collection.AddScoped<IRefreshTokenManager, RefreshTokenManager>();
			collection.AddScoped<IJwtTokenService, JwtTokenService>();
			collection.AddScoped<AuthenticationStateProvider, RevalidatingIdentityAuthenticationStateProvider<ApplicationUser>>();
			collection.AddScoped<IDialogService, DialogService>();
			collection.AddScoped<BackendIntegrationService>();
		}

		private static Func<IServiceProvider, NamedPipeChannel> CreateConfiguredNamedPipeChannel()
		{
			return serviceProvider =>
			{
				var options = new NamedPipeChannelOptions();
				options.ConnectionTimeout = (int)TimeSpan.FromSeconds(3).TotalMilliseconds;
				var channel = new NamedPipeChannel(".", Globals.NamedPipeChannel, options);
				return channel;
			};
		}
	}
}