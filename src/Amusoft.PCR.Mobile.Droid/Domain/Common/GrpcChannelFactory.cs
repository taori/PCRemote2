using System;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Grpc.Net.Client;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class GrpcChannelFactory
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(GrpcChannelFactory));

		public static GrpcChannel Create(HostEndpointAddress address)
		{
			var baseAddress = new Uri(address.FullAddress);
			Log.Trace("Creating channel with connection {Address}", address.FullAddress);

			var channelOptions = new GrpcChannelOptions()
			{
				DisposeHttpClient = true,
				HttpClient = GrpcWebHttpClientFactory.Create(baseAddress, new AuthenticationSurface(address, new AuthenticationStorage(address)))
			};
			var channel = GrpcChannel.ForAddress(new Uri(address.FullAddress), channelOptions);

			return channel;
		}
	}
}