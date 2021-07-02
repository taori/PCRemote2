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

namespace Amusoft.PCR.Server.BackgroundServices
{
	public class DesktopIntegrationService : BackgroundService
	{
		private readonly ILogger<DesktopIntegrationService> _logger;
		private readonly IIntegrationApplicationLocator _integrationApplicationLocator;
		private bool _canOperate;

		public DesktopIntegrationService(ILogger<DesktopIntegrationService> logger, IIntegrationApplicationLocator integrationApplicationLocator)
		{
			_logger = logger;
			_integrationApplicationLocator = integrationApplicationLocator;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("{Name} starting", nameof(DesktopIntegrationService));
			_canOperate = _integrationApplicationLocator.IsOperational();
			_logger.LogDebug("{Name} is in operational state {State}", nameof(DesktopIntegrationService), _canOperate);
			return base.StartAsync(cancellationToken);
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var waitDuration = TimeSpan.FromSeconds(10);
			while (!stoppingToken.IsCancellationRequested && _canOperate)
			{
				if(!_integrationApplicationLocator.IsRunning())
					TryLaunchIntegration();

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

			return base.StopAsync(cancellationToken);
		}

		private bool TryLaunchIntegration()
		{
			try
			{
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
				_logger.LogError(e, "Exception occured while calling {Name}", nameof(TryLaunchIntegration));
				return false;
			}
		}
	}
}