using System.Text.RegularExpressions;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class HostEndpointAddress
	{
		private static readonly Regex AddressPattern = new Regex(@"https://((\[?(?<address>([\dabcdef]{1,4}:?){8})\]?:(?<port>[\d]{1,3}))|((?<address>[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}\.[\d]{1,3}):(?<port>[\d]{2,5})))", RegexOptions.IgnoreCase | RegexOptions.Compiled);

		public static bool TryParse(string fullAddress, out HostEndpointAddress address)
		{
			address = null;

			var match = AddressPattern.Match(fullAddress);
			if (!match.Success)
				return false;

			address = new HostEndpointAddress(match.Groups["address"].Value, int.Parse(match.Groups["port"].Value));
			return true;
		}

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