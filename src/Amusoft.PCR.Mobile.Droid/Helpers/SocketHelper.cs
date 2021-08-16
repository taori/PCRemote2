using System;
using System.Threading.Tasks;
using Java.Net;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Helpers
{
	public static class SocketHelper
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SocketHelper));

		public static async Task<bool> IsPortOpenAsync(string ip, int port, TimeSpan timeout)
		{
			try
			{
				Log.Trace("Verifying connectivity to {Ip}:{Port}", ip, port);
				using var socket = new Socket();
				await socket.ConnectAsync(new InetSocketAddress(ip, port), (int)timeout.TotalMilliseconds);
				socket.Close();
				Log.Debug("Connection to {Address} can be established", $"{ip}:{port}");
				return true;
			}
			catch (ConnectException ce)
			{
				Log.Error(ce, $"Error connecting to {ip}:{port}");
				return false;
			}
			catch (Exception ex)
			{
				Log.Error(ex, $"Error connecting to {ip}:{port}");
				return false;
			}
		}
	}
}