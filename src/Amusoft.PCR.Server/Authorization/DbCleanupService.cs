using System;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Managers;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Authorization
{
	public class DbCleanupService : BackgroundService
	{
		private readonly ApplicationStateTransmitter _applicationStateTransmitter;
		private readonly ILogger<DbCleanupService> _log;
		private readonly IRefreshTokenManager _refreshTokenManager;

		public DbCleanupService(
			ApplicationStateTransmitter applicationStateTransmitter,
			ILogger<DbCleanupService> log,
			IRefreshTokenManager refreshTokenManager)
		{
			_applicationStateTransmitter = applicationStateTransmitter;
			_log = log;
			_refreshTokenManager = refreshTokenManager;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _applicationStateTransmitter.ConfigurationDone;

			while (true)
			{
				_log.LogInformation("Cleaning database");

				if (!await _refreshTokenManager.RemoveAllExpiredTokensAsync())
				{
					_log.LogError("Failed to clean up expired refreshTokens");
				}

				await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
			}
		}
	}
}