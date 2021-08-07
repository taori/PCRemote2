using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace Amusoft.Toolkit.Networking
{
	public static class WakeOnLan
	{
		public static async Task UsingAddressAsync(string macAddress)
		{
			var magicPacket = ToMagicPacket(macAddress);
			foreach (var networkInterface in GetRunningNetworkInterfaces())
			{
				await BroadcastOnNetworkInterface(networkInterface, magicPacket);
			}
		}

		private static async Task BroadcastOnNetworkInterface(NetworkInterface networkInterface, byte[] magicPacket)
		{
			var interfaceProperties = networkInterface.GetIPProperties();
			foreach (var targetAddress in interfaceProperties.MulticastAddresses)
			{
				if (IsAddressIpv4(targetAddress.Address))
				{
					if (await TrySendIpv4(magicPacket, interfaceProperties, targetAddress.Address)) 
						break;
				}

				if (IsAddressIpv6(targetAddress.Address))
				{
					if (await TrySendIpv6(magicPacket, interfaceProperties, targetAddress.Address)) 
						break;
				}
			}
		}

		private static async Task<bool> TrySendIpv4(byte[] magicPacket, IPInterfaceProperties ipProperties, IPAddress address)
		{
			// Ipv4: All hosts on LAN
			var addressInformation = ipProperties
				.UnicastAddresses
				.FirstOrDefault(d => d.Address.AddressFamily == AddressFamily.InterNetwork && !ipProperties.GetIPv4Properties().IsAutomaticPrivateAddressingActive);
			if (addressInformation != null)
			{
				await SendBytesAsync(addressInformation.Address, address, magicPacket);
				return true;
			}

			return false;
		}

		private static async Task<bool> TrySendIpv6(byte[] magicPacket, IPInterfaceProperties ipProperties, IPAddress address)
		{
			// Ipv6: All hosts on LAN (with zone index)
			var addressInformation = ipProperties
				.UnicastAddresses
				.FirstOrDefault(d => d.Address.AddressFamily == AddressFamily.InterNetworkV6 && !d.Address.IsIPv6LinkLocal);
			if (addressInformation != null)
			{
				await SendBytesAsync(addressInformation.Address, address, magicPacket);
				return true;
			}

			return false;
		}

		private static bool IsAddressIpv4(IPAddress address)
		{
			return address.ToString().Equals("224.0.0.1");
		}

		private static bool IsAddressIpv6(IPAddress address)
		{
			return address.ToString().StartsWith("ff02::1%", StringComparison.OrdinalIgnoreCase);
		}

		private static IEnumerable<NetworkInterface> GetRunningNetworkInterfaces()
		{
			return NetworkInterface.GetAllNetworkInterfaces().Where((n) =>
				n.NetworkInterfaceType != NetworkInterfaceType.Loopback && n.OperationalStatus == OperationalStatus.Up);
		}

		static byte[] ToMagicPacket(string macAddress) // MacAddress in any standard HEX format
		{
			macAddress = Regex.Replace(macAddress, "[: -]", "");
			var macBytes = new byte[6];
			for (var i = 0; i < 6; i++)
			{
				macBytes[i] = Convert.ToByte(macAddress.Substring(i * 2, 2), 16);
			}

			using var memoryStream = new MemoryStream();
			using (var binaryWriter = new BinaryWriter(memoryStream))
			{
				//First 6 times 0xff
				for (var i = 0; i < 6; i++)  
				{
					binaryWriter.Write((byte)0xff);
				}
				// then 16 times MacAddress
				for (var i = 0; i < 16; i++) 
				{
					binaryWriter.Write(macBytes);
				}
			}

			// 102 bytes magic packet
			return memoryStream.ToArray(); 
		}

		private static async Task SendBytesAsync(IPAddress localAddress, IPAddress targetAddress, byte[] magicBytes)
		{
			using var client = new UdpClient(new IPEndPoint(localAddress, 0));
			await client.SendAsync(magicBytes, magicBytes.Length, targetAddress.ToString(), 9);
		}
	}
}