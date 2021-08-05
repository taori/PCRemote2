using System;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Grpc.Net.Client;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class GrpcApplicationAgentFactory
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(GrpcApplicationAgentFactory));

		public static GrpcApplicationAgent Create(string addressAndPort)
		{
			var seperatorIndex = addressAndPort.IndexOf(':');
			var address = addressAndPort.Substring(0, seperatorIndex);
			var port = addressAndPort.Substring(seperatorIndex + 1);
			if (!int.TryParse(port, out var parsedPort))
				throw new Exception($"Invalid port {port}");

			return Create(address, parsedPort);
		}

		public static GrpcApplicationAgent Create(string ipAddress, int port)
		{
			var uriString = $"https://{ipAddress}:{port}";
			var baseAddress = new Uri(uriString);
			Log.Debug("Creating agent with connection {Address}", uriString);

			var channelOptions = new GrpcChannelOptions()
			{
				DisposeHttpClient = true,
				HttpClient = GrpcWebHttpClientFactory.Create(baseAddress, new AuthenticationSurface(uriString))
			};
			var channel = GrpcChannel.ForAddress(new Uri(uriString), channelOptions);

			return new GrpcApplicationAgent(channel);
		}
	}
}