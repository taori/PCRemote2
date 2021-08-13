namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class HostEndpointAddress
	{
		public override string ToString() => FullAddress;

		public HostEndpointAddress(string ipAddress, int port)
		{
			IpAddress = ipAddress;
			Port = port;
		}

		public string FullAddress => $"https://{IpAddress}:{Port}";

		public string IpAddress { get; }

		public int Port { get; }
	}
}