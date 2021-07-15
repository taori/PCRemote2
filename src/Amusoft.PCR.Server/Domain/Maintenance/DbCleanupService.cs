using System;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Domain.Authorization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.Maintenance
{
	public class DbCleanupService : BackgroundService
	{
		private readonly ApplicationStateTransmitter _applicationStateTransmitter;
		private readonly ILogger<DbCleanupService> _log;
		private readonly IServiceScopeFactory _serviceScopeFactory;

		public DbCleanupService(
			ApplicationStateTransmitter applicationStateTransmitter,
			ILogger<DbCleanupService> log,
			IServiceScopeFactory serviceScopeFactory)
		{
			_applicationStateTransmitter = applicationStateTransmitter;
			_log = log;
			_serviceScopeFactory = serviceScopeFactory;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			await _applicationStateTransmitter.ConfigurationDone;

			while (true)
			{
				_log.LogInformation("Cleaning database");

				var scope = _serviceScopeFactory.CreateScope();
				using (scope)
				{
					var refreshTokenManager = scope.ServiceProvider.GetRequiredService<IRefreshTokenManager>();
					if (!await refreshTokenManager.RemoveAllExpiredTokensAsync())
					{
						_log.LogError("Failed to clean up expired refreshTokens");
					}
				}

				await Task.Delay(TimeSpan.FromDays(1), stoppingToken);
			}
		}
	}
}