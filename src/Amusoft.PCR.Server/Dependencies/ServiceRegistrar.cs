using Amusoft.PCR.Blazor.Services;
using Amusoft.PCR.Server.Data;
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

			collection.AddScoped<IDialogService, DialogService>();
		}
	}
}