using System.Collections.Concurrent;
using Grpc.Net.Client;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class GrpcChannelHub
	{
		private static readonly ConcurrentDictionary<string, GrpcChannel> Channels = new ConcurrentDictionary<string, GrpcChannel>();

		public static GrpcChannel GetChannelFor(HostEndpointAddress address)
		{
			return Channels.GetOrAdd(address.FullAddress.ToUpper(), s => GrpcChannelFactory.Create(address));
		}
	}
}