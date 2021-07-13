using System;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Extensions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Hosting.Server.Features;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Protocols;
using NLog;
using LogLevel = Microsoft.Extensions.Logging.LogLevel;

namespace Amusoft.PCR.Server.BackgroundServices
{
	internal class LanAddressBroadcastSettings
	{
		public bool IsBroadcastDiagnosticsEnabled { get; set; }

		public int Port { get; set; }

		public TimeSpan BroadcastInterval { get; set; }

		public int[] PublicHttpsPorts { get; set; }
	}

	internal class LanAddressBroadcastService : BackgroundService
	{
		public readonly ILogger<LanAddressBroadcastService> _logger;
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
				await Task.WhenAny(ReceiveLoop(stoppingToken), BroadcastLoopAsync(stoppingToken));

				_logger.LogInformation("Beacon terminated");
			}
			catch (OperationCanceledException)
			{
				_logger.LogInformation("Beacon terminated");
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Unexpected error occured");
			}
		}

		private async Task BroadcastLoopAsync(CancellationToken stoppingToken)
		{
			var beaconPort = _settings.Value.Port;
			_logger.LogInformation("Broadcasting address on port [{Port}]", beaconPort);

			var ports = _settings.Value.PublicHttpsPorts;
			var targetEndpoint = new IPEndPoint(IPAddress.Broadcast, beaconPort);
			var datagram = Encoding.UTF8.GetBytes(GrpcHandshakeFormatter.Write(Environment.MachineName, ports));
			var broadcastInterval = _settings.Value.BroadcastInterval;

			using (var broadcaster = new UdpClient())
			{
				broadcaster.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				broadcaster.ExclusiveAddressUse = false;
				broadcaster.AllowNatTraversal(true);

				while (!stoppingToken.IsCancellationRequested)
				{
					_logger.LogTrace("---> Transmitting datagram");
					await broadcaster.SendAsync(datagram, datagram.Length, targetEndpoint);

					_logger.LogTrace("Transmission success - Waiting for next turn ...");
					await Task.Delay(broadcastInterval, stoppingToken);
				}
			}
		}

		public async Task ReceiveLoop(CancellationToken cancellationToken)
		{
			using var client = new UdpClient();
			client.EnableBroadcast = true;
			client.Client.ExclusiveAddressUse = false;
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			client.Client.Bind(new IPEndPoint(IPAddress.Any, _settings.Value.Port));

			while (!cancellationToken.IsCancellationRequested)
			{
				if (!_settings.Value.IsBroadcastDiagnosticsEnabled)
				{
					await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);
					continue;
				}

				var result = await client.ReceiveAsync();
				_logger.LogDebug("Received UDP Package from {Address} -> {Message}", result.RemoteEndPoint,
					Encoding.UTF8.GetString(result.Buffer));
			}
		}
	}
}