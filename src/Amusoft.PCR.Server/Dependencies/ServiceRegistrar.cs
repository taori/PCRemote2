using System;
using Amusoft.PCR.Blazor.Services;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Server.BackgroundServices;
using Amusoft.PCR.Server.Data;
using GrpcDotNetNamedPipes;
using Microsoft.Extensions.DependencyInjection;

namespace Amusoft.PCR.Server.Dependencies
{
	public static class ServiceRegistrar
	{
		public static void Register(IServiceCollection collection)
		{
			collection.AddSingleton<WeatherForecastService>();
			collection.AddSingleton<ClassLoader>();
			collection.AddSingleton<IValidationService, ValidationService>();
			collection.AddSingleton<NamedPipeChannel>(CreateConfiguredNamedPipeChannel());
			collection.AddSingleton<IInteropService, InteropService>();
			collection.AddSingleton<IIntegrationApplicationLocator, IntegrationApplicationLocator>();

			collection.AddScoped<IDialogService, DialogService>();
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