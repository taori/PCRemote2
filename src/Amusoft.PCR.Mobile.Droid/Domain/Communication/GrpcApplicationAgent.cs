using System;
using System.Net.Http;
using Amusoft.PCR.Grpc.Common;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client.Web;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public class GrpcApplicationAgent : IDisposable
	{
		private readonly GrpcChannel _channel;

		public GrpcApplicationAgent(GrpcChannel channel)
		{
			_channel = channel;
			DesktopIntegrationClient = new DesktopIntegrationService.DesktopIntegrationServiceClient(_channel);
		}

		public DesktopIntegrationService.DesktopIntegrationServiceClient DesktopIntegrationClient { get; }

		public void Dispose()
		{
			_channel?.Dispose();
		}
	}
}