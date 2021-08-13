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

			return Create(new HostEndpointAddress(address, parsedPort));
		}

		public static GrpcApplicationAgent Create(HostEndpointAddress address)
		{
			var baseAddress = new Uri(address.FullAddress);
			Log.Debug("Creating agent with connection {Address}", address.FullAddress);

			var channelOptions = new GrpcChannelOptions()
			{
				DisposeHttpClient = true,
				HttpClient = GrpcWebHttpClientFactory.Create(baseAddress, new AuthenticationSurface(address, new AuthenticationStorage(address)))
			};
			var channel = GrpcChannel.ForAddress(new Uri(address.FullAddress), channelOptions);

			return new GrpcApplicationAgent(channel);
		}
	}
}