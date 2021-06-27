using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Server.Extensions;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using NLog;

namespace Amusoft.PCR.Server.BackgroundServices
{
	internal class LanAddressBroadcastSettings
	{
		public int Port { get; set; }

		public TimeSpan BroadcastInterval { get; set; }
	}

	internal class LanAddressBroadcastService : BackgroundService
	{
		public ILogger<LanAddressBroadcastService> _logger;
		private readonly IOptions<LanAddressBroadcastSettings> _settings;

		public LanAddressBroadcastService(ILogger<LanAddressBroadcastService> logger, IOptions<LanAddressBroadcastSettings> settings)
		{
			_logger = logger;
			_settings = settings;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			try
			{
				var beaconPort = _settings.Value.Port;

				var targetEndpoint = new IPEndPoint(IPAddress.Broadcast, beaconPort);
				var datagram = Encoding.UTF8.GetBytes($"PCRemote 2 looking for clients.");
				var broadcastInterval = _settings.Value.BroadcastInterval;

				using (var broadcaster = new UdpClient())
				{
					broadcaster.AllowNatTraversal(true);

					while (!stoppingToken.IsCancellationRequested)
					{
						_logger.LogTrace("---> Transmitting datagram");
						await broadcaster.SendAsync(datagram, datagram.Length, targetEndpoint);

						_logger.LogTrace("Transmission success - Waiting for next turn ...");
						await Task.Delay(broadcastInterval, stoppingToken);
					}
				}

				_logger.LogInformation("Beacon terminating");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unexpected error occured");
			}
		}
	}
}