using System;
using Amusoft.PCR.Grpc.Common;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public class GrpcApplicationAgent : IDisposable
	{
		private readonly GrpcChannel _channel;

		public GrpcApplicationAgent(Uri uri, GrpcChannelOptions options = default)
		{
			_channel = options == null ? GrpcChannel.ForAddress(uri) : GrpcChannel.ForAddress(uri, options);

			DesktopIntegrationClient = new DesktopIntegrationService.DesktopIntegrationServiceClient(_channel);
		}

		public DesktopIntegrationService.DesktopIntegrationServiceClient DesktopIntegrationClient { get; }

		public void Dispose()
		{
			_channel?.Dispose();
		}
	}
}