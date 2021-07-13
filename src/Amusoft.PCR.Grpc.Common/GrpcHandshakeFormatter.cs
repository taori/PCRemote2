using System;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace Amusoft.PCR.Grpc.Common
{
	public class GrpcHandshakeFormatter
	{
		public static string Write(string machineName, int[] ports)
		{
			var portList = string.Join(";", ports);
			return $"PCRemote 2 looking for clients at ports [{machineName}___{portList}]";
		}

		private static readonly Regex ParseRegex = new Regex("\\[(?<machine>.+)___(?<ports>.+)\\]", RegexOptions.Compiled | RegexOptions.Singleline);

		private static readonly char[] PortSplitter = new char[] {';'};
		public static GrpcHandshakeMessage Parse(byte[] message)
		{
			var content = Encoding.UTF8.GetString(message);
			var match = ParseRegex.Match(content);
			var portString = match.Groups["ports"].Value;
			var ports = portString
				.Split(PortSplitter, StringSplitOptions.RemoveEmptyEntries)
				.Select(int.Parse)
				.ToArray();

			var result = new GrpcHandshakeMessage(match.Groups["machine"].Value, ports);

			return result;
		}
	}

	public readonly struct GrpcHandshakeMessage
	{
		public GrpcHandshakeMessage(string machineName, int[] ports)
		{
			MachineName = machineName;
			Ports = ports;
		}

		public string MachineName { get; }
		public int[] Ports { get; }
	}
}