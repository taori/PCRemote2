using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Amusoft.PCR.Server.Authorization;
using Amusoft.PCR.Server.BackgroundServices;
using Amusoft.PCR.Server.Dependencies;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog;
using NLog.Web;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Amusoft.PCR.Server
{
	public class Program
	{
		private static Logger _logger;

		public static void Main(string[] args)
		{
			_logger = NLog.Web.NLogBuilder.ConfigureNLog("nlog.config").GetCurrentClassLogger();
			try
			{
				_logger.Debug("Executing {Method}", nameof(Main));


				var isService = !(Debugger.IsAttached || args.Contains("--console"));

				if (isService)
				{
					var pathToExe = Assembly.GetExecutingAssembly().Location;
					_logger.Debug("Retrieved {Path} as execution file path", pathToExe);
					var pathToContentRoot = Path.GetDirectoryName(pathToExe);
					Directory.SetCurrentDirectory(pathToContentRoot);
				}

				var builder = CreateHostBuilder(
					args.Where(arg => arg != "--console").ToArray());

				var host = builder.Build();

				host.Run();
			}
			catch (Exception exception)
			{
				_logger.Error(exception, "Stopped program because of exception");
				throw;
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				NLog.LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logging =>
				{
					logging.AddEventLog(settings =>
					{
						settings.Filter = (s, level) => level >= LogLevel.Debug;
					});
				})
				.UseNLog()
				.UseWindowsService()
				.ConfigureServices(services =>
				{
					services.AddHostedService<DesktopIntegrationLauncherService>();
					services.AddHostedService<LanAddressBroadcastService>();
					services.AddHostedService<PermissionSeedService>();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					// var contentRoot = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
					// _logger.Debug("Setting content root to {Path}", contentRoot);
					// webBuilder.UseContentRoot(contentRoot);
					webBuilder.UseStartup<Startup>();
				});
	}
}
