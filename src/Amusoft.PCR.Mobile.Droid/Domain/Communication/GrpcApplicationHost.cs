using System;
using Amusoft.PCR.Grpc.Common;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public class GrpcApplicationHost : IDisposable
	{
		private readonly GrpcChannel _channel;

		public GrpcApplicationHost(Uri uri, GrpcChannelOptions options = default)
		{
			_channel = options == null ? GrpcChannel.ForAddress(uri) : GrpcChannel.ForAddress(uri, options);

			InteropClient = new WindowsInteropService.WindowsInteropServiceClient(_channel);
		}

		public WindowsInteropService.WindowsInteropServiceClient InteropClient { get; }

		public void Dispose()
		{
			_channel?.Dispose();
		}
	}
}