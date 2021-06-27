using System.Net;

namespace Amusoft.PCR.Server.Extensions
{
	public static class EndpointExtensions
	{
		public static string Prettify(this IPEndPoint source)
		{
			return $"{source.Address.ToString().PadLeft(15, ' ')}:{source.Port.ToString().PadLeft(5, ' ')}";
		}
	}
}