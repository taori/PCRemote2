using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.Toolkit.Impersonation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.IPC
{
	public class DesktopIntegrationLauncherService : BackgroundService
	{
		private readonly ILogger<DesktopIntegrationLauncherService> _logger;
		private readonly IIntegrationApplicationLocator _integrationApplicationLocator;
		private readonly ApplicationStateTransmitter _applicationStateTransmitter;
		private bool _canOperate;

		public DesktopIntegrationLauncherService(ILogger<DesktopIntegrationLauncherService> logger, IIntegrationApplicationLocator integrationApplicationLocator, ApplicationStateTransmitter applicationStateTransmitter)
		{
			_logger = logger;
			_integrationApplicationLocator = integrationApplicationLocator;
			_applicationStateTransmitter = applicationStateTransmitter;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("{Name} starting", nameof(DesktopIntegrationLauncherService));
			_canOperate = _integrationApplicationLocator.IsOperational();
			if (_canOperate)
			{
				_logger.LogInformation("{Name} is working", nameof(DesktopIntegrationLauncherService));
			}
			else
			{
				_logger.LogCritical("{Name} is NOT operational", nameof(DesktopIntegrationLauncherService));
			}

			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var waitDuration = TimeSpan.FromSeconds(60);
			_logger.LogDebug("Checking for integration up state every {Seconds} seconds", waitDuration.TotalSeconds);
			while (!stoppingToken.IsCancellationRequested && _canOperate)
			{
				if(!_integrationApplicationLocator.IsRunning())
					await TryLaunchIntegrationAsync();

				_logger.LogTrace("Waiting for next turn to check if integration backend is working ({Time}ms)", waitDuration.TotalMilliseconds);
				await Task.Delay(waitDuration, stoppingToken);
			}
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			if (_canOperate)
			{
				_logger.LogInformation("Terminating current integration instances");

				var runningProcesses = _integrationApplicationLocator.GetIntegrationProcesses().ToArray();
				if (runningProcesses.Length > 0)
				{
					_logger.LogDebug("Terminating {Count} instances", runningProcesses.Length);
					foreach (var match in runningProcesses)
					{
						_logger.LogDebug("Killing process {Id}", match.processId);
						Process.GetProcessById(match.processId).Kill();
					}
				}
				else
				{
					_logger.LogWarning("No integration instances found. It must have crashed?");
				}
			}
			else
			{
				_logger.LogInformation("Not operational. Nothing to do");
			}

			return base.StopAsync(cancellationToken);
		}

		private async Task<bool> TryLaunchIntegrationAsync()
		{
			try
			{
				_logger.LogDebug("Waiting for application configuration to be done");
				await _applicationStateTransmitter.ConfigurationDone;

				var fullPath = _integrationApplicationLocator.GetAbsolutePath();
				if (!File.Exists(fullPath))
				{
					_logger.LogError("Cannot launch {Path} because it does not exist", fullPath);
					return false;
				}

				_logger.LogInformation("Launching application at {Path}", fullPath);

				ProcessImpersonation.Launch(fullPath);

				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling {Name}", nameof(TryLaunchIntegrationAsync));
				return false;
			}
		}
	}
}