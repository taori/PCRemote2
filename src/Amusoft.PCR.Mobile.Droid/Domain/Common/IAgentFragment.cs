using Amusoft.PCR.Mobile.Droid.Domain.Communication;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public interface IAgentFragment
	{
		public string IpAddress { get; set; }

		public int Port { get; set; }

		public string HostName { get; set; }
	}
}