using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Domain.Authorization;
using Amusoft.PCR.Server.Domain.IPC;
using Amusoft.PCR.Server.Domain.Maintenance;
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
			catch (OperationCanceledException e)
			{
				_logger.Error(e, "Outer operation cancelled exception occured");
			}
			catch (Exception exception)
			{
				_logger.Error(exception, "Stopped program because of exception");
				throw;
			}
			finally
			{
				// Ensure to flush and stop internal timers/threads before application-exit (Avoid segmentation fault on Linux)
				LogManager.Flush();
				LogManager.Shutdown();
			}
		}

		public static IHostBuilder CreateHostBuilder(string[] args) =>
			Host.CreateDefaultBuilder(args)
				.ConfigureLogging(logging =>
				{
					logging.AddEventLog(settings =>
					{
						settings.Filter = (s, level) => level >= LogLevel.Information;
					});
				})
				.UseNLog()
				.UseWindowsService()
				.ConfigureServices(services =>
				{
					services.AddHostedService<DesktopIntegrationLauncherService>();
					services.AddHostedService<LanAddressBroadcastService>();
					services.AddHostedService<SeedService>();
					services.AddHostedService<DbCleanupService>();
				})
				.ConfigureWebHostDefaults(webBuilder =>
				{
					webBuilder.UseStartup<Startup>();
				});
	}
}
