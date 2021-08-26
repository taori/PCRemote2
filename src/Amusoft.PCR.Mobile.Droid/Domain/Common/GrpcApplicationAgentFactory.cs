using System.Collections.Generic;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class GrpcApplicationAgentFactory
	{
		public static GrpcApplicationAgent Create(GrpcChannel channel)
		{
			return new GrpcApplicationAgent(channel);
		}
	}
}