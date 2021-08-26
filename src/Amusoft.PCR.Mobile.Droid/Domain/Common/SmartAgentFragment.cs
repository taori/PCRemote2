namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class SmartAgentFragment : SmartFragment, IAgentFragment
	{
		public string IpAddress { get; set; }
		public int Port { get; set; }
		public string HostName { get; set; }
	}
}