using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using Amusoft.PCR.Server.BackgroundServices;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amusoft.PCR.Server.Dependencies
{
	public interface IIntegrationApplicationLocator
	{
		bool IsOperational();
		bool IsRunning();
		string GetAbsolutePath();
		IEnumerable<(int processId, string path)> GetIntegrationProcesses();
	}

	public class IntegrationApplicationLocator : IIntegrationApplicationLocator
	{
		private readonly ILogger<IntegrationApplicationLocator> _logger;
		private readonly IOptions<DesktopIntegrationSettings> _settings;

		public IntegrationApplicationLocator(ILogger<IntegrationApplicationLocator> logger, IOptions<DesktopIntegrationSettings> settings)
		{
			_logger = logger;
			_settings = settings;
		}

		private string GetInferredAbsolutePath(string exePath)
		{
			var executingAssemblyDirectory = Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location);
			var originUri = new Uri(executingAssemblyDirectory + Path.DirectorySeparatorChar, UriKind.Absolute);
			var relativePortion = new Uri(exePath, UriKind.Relative);
			var combinedUri = new Uri(originUri, relativePortion);

			var resultPath = combinedUri.LocalPath;
			_logger.LogTrace("Combined Uri {Path} from {OriginPath} and {RelativePath}", resultPath, executingAssemblyDirectory, exePath);

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
		
		public bool IsOperational()
		{
#if DEBUG
			return IsOperationalInDebug();
#else
			return IsOperationalInRelease();
#endif
		}

		private bool IsOperationalInRelease()
		{
			var result = IsConfigurationOperational();
			if (result)
			{
				var fullPath = GetInferredAbsolutePath(_settings.Value.ExePath);
				result = File.Exists(fullPath);

				if (!result)
				{
					_logger.LogError("The integration path {Path} does not exist and therefore cannot be launched", fullPath);
				}
			}

			return result;
		}

		private bool IsOperationalInDebug()
		{
			return GetIntegrationProcesses().Any();
		}

		public bool IsRunning()
		{
			return GetIntegrationProcesses().Any();
		}

		public string GetAbsolutePath()
		{
			return GetInferredAbsolutePath(_settings.Value.ExePath);
		}

		public IEnumerable<(int processId, string path)> GetIntegrationProcesses()
		{
			var normalizedFileName = Path.GetFileName(Path.GetFullPath(GetAbsolutePath()));
			var allProcesses = GetProcessExePaths();

			return allProcesses
				.Where(d => Path.GetFileName(Path.GetFullPath(d.fullPath)).Equals(normalizedFileName));
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
	}
}