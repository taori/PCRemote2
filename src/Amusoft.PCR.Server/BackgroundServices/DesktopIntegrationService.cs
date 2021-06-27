using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.Toolkit.Impersonation;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amusoft.PCR.Server.BackgroundServices
{
	internal class DesktopIntegrationService : BackgroundService
	{
		private readonly ILogger<DesktopIntegrationService> _logger;
		private readonly IOptions<DesktopIntegrationSettings> _settings;
		private bool _canOperate;
		private string _exeFileName;
		private string _exeAbsolutePath;

		public DesktopIntegrationService(ILogger<DesktopIntegrationService> logger, IOptions<DesktopIntegrationSettings> settings)
		{
			_logger = logger;
			_settings = settings;
		}

		public override Task StartAsync(CancellationToken cancellationToken)
		{
			_logger.LogDebug("{Name} starting", nameof(DesktopIntegrationService));
			_canOperate = IsConfigurationOperational();
			if (_canOperate)
			{
				_exeFileName = Path.GetFileName(_settings.Value.ExePath);
				_exeAbsolutePath = GetInferredAbsolutePath();
				_canOperate = File.Exists(_exeAbsolutePath);

				if (!_canOperate)
				{
					_logger.LogError("The integration path {Path} does not exist and therefore cannot be launched", _exeAbsolutePath);
				}
			}

			_logger.LogDebug("{Name} is in operational state {State}", nameof(DesktopIntegrationService), _canOperate);
			return base.StartAsync(cancellationToken);
		}

		private string GetInferredAbsolutePath()
		{
			var executingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var originUri = new Uri(executingAssemblyDirectory + Path.DirectorySeparatorChar, UriKind.Absolute);
			var relativePortion = new Uri(_settings.Value.ExePath, UriKind.Relative);
			var combinedUri = new Uri(originUri, relativePortion);

			var resultPath = combinedUri.LocalPath;
			_logger.LogDebug("Combined Uri {Path} from {OriginPath} and {RelativePath}", resultPath, executingAssemblyDirectory, _settings.Value.ExePath);

			return resultPath;
		}

		private bool IsConfigurationOperational()
		{
			if (_settings.Value == null)
			{
				_logger.LogError("Settings for IntegrationRunner are not present");
				return false;
			}

			if (string.IsNullOrEmpty(_settings.Value.ExePath))
			{
				_logger.LogError("IntegrationRunner settings ExePath is null or empty");
				return false;
			}

			return true;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var waitDuration = TimeSpan.FromSeconds(10);
			while (!stoppingToken.IsCancellationRequested && _canOperate)
			{
				if(!IsIntegrationRunning())
					TryLaunchIntegration();

				_logger.LogTrace("Waiting for next turn to check if integration backend is working ({Time}ms)", waitDuration.TotalMilliseconds);
				await Task.Delay(waitDuration, stoppingToken);
			}
		}

		private bool IsIntegrationRunning()
		{
			var processExePaths = GetProcessExePaths();
			var result = _settings.Value.PathCheck switch
			{
				DesktopIntegrationSettings.PathCheckMode.FileCheckOnly => processExePaths.Any(d => d.fullPath.EndsWith(_exeFileName)),
				DesktopIntegrationSettings.PathCheckMode.Exact => processExePaths.Any(d => Path.GetFullPath(d.fullPath).Equals(Path.GetFullPath(_exeAbsolutePath))),
				
			};
			return result;
		}

		public override Task StopAsync(CancellationToken cancellationToken)
		{
			if (_canOperate)
			{
				_logger.LogInformation("Terminating current integration instances");

				var allProcesses = GetProcessExePaths();
				var matches = allProcesses
					.Where(d => Path.GetFullPath(d.fullPath).Equals(_exeAbsolutePath))
					.ToArray();

				if (matches.Length > 0)
				{
					_logger.LogDebug("Terminating {Count} instances", matches.Length);
					foreach (var match in matches)
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

		private IReadOnlyList<(int processId, string fullPath)> GetProcessExePaths()
		{
			var results = new List<(int processId, string fullPath)>();
			foreach (var process in Process.GetProcesses())
			{
				try
				{
					if (process.MainModule?.FileName != null)
					{
						results.Add((process.Id, process.MainModule.FileName));
					}
				}
				catch (Exception)
				{
					// ignored
				}
			}

			return results;
		}

		private bool TryLaunchIntegration()
		{
			try
			{
				if (!File.Exists(_exeAbsolutePath))
				{
					_logger.LogError("Cannot launch {Path} because it does not exist", _exeAbsolutePath);
					return false;
				}

				_logger.LogInformation("Launching application at {Path}", _exeAbsolutePath);


				ProcessImpersonation.Launch(_exeAbsolutePath);

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