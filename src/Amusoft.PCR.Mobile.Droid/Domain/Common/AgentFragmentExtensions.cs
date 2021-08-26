using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using AndroidX.Fragment.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public static class AgentFragmentExtensions
	{
		public static GrpcApplicationAgent GetAgent(this IAgentFragment source)
		{
			return new GrpcApplicationAgent(GrpcChannelHub.GetChannelFor(new HostEndpointAddress(source.IpAddress, source.Port)));
		}


		public static Fragment WithAgent<TFragment>(this TFragment fragment, string ipAddress, string hostName, int port)
			where  TFragment : Fragment, IAgentFragment
		{
			fragment.IpAddress = ipAddress;
			fragment.Port = port;
			fragment.HostName = hostName;
			return fragment;
		}

		public static Fragment WithAgent<TFragment>(this TFragment fragment, IAgentFragment sourceFragment)
			where  TFragment : Fragment, IAgentFragment
		{
			fragment.IpAddress = sourceFragment.IpAddress;
			fragment.Port = sourceFragment.Port;
			fragment.HostName = sourceFragment.HostName;
			return fragment;
		}
	}
}