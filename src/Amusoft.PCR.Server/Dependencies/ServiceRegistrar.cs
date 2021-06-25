using Amusoft.PCR.Blazor.Services;
using Amusoft.PCR.Grpc.Common;
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
			collection.AddSingleton<NamedPipeChannel>(_ => new NamedPipeChannel(".", Globals.NamedPipeChannel));
			collection.AddSingleton<IInteropService, InteropService>();

			collection.AddScoped<IDialogService, DialogService>();
		}
	}
}