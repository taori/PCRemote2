using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.Toolkit.Networking;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Amusoft.PCR.Server.Domain.IPC
{
	internal class ClientDiscoverySettings
	{
		public int Port { get; set; }

		public int[] PublicHttpsPorts { get; set; }
	}

	internal class ClientDiscoveryService : BackgroundService
	{
		public readonly ILogger<ClientDiscoveryService> _logger;
		private readonly IOptions<ClientDiscoverySettings> _settings;
		private UdpBroadcastCommunicationChannel _channel;

		public ClientDiscoveryService(ILogger<ClientDiscoveryService> logger, IOptions<ClientDiscoverySettings> settings)
		{
			_logger = logger;
			_settings = settings;
		}

		public override void Dispose()
		{
			_channel?.Dispose();

			base.Dispose();
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			_channel = new UdpBroadcastCommunicationChannel(new UdpBroadcastCommunicationChannelSettings(_settings.Value.Port));
			var receiveHandler = _channel.MessageReceived
				.Subscribe(async (d) => { await HandleReceive(d); });
			try
			{
				_logger.LogInformation("{Service} is listening on port {Port}", nameof(ClientDiscoveryService), _settings.Value.Port);
				await _channel.StartListeningAsync(stoppingToken);

				_logger.LogInformation("Channel terminated");
			}
			catch (OperationCanceledException)
			{
				receiveHandler.Dispose();
				_logger.LogInformation("Terminating channel");
			}
			catch (Exception e)
			{
				receiveHandler.Dispose();
				_logger.LogError(e, "Terminating channel");
			}
		}

		private async Task HandleReceive(UdpReceiveResult received)
		{
			var message = Encoding.UTF8.GetString(received.Buffer);
			if (!string.Equals(message, GrpcHandshakeClientMessage.Message))
			{
				_logger.LogDebug("Discarding message - invalid");
				return;
			}
				
			_logger.LogInformation("Received handshake from [{Address}]", received.RemoteEndPoint);
			var ports = _settings.Value.PublicHttpsPorts;
			var replyText = GrpcHandshakeFormatter.Write(Environment.MachineName, ports);
			await _channel.SendAsync(Encoding.UTF8.GetBytes(replyText));
			_logger.LogDebug("Reply \"{Message}\" sent", replyText);
		}
	}
}