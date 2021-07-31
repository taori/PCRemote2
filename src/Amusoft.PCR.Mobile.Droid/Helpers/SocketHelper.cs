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
				var socket = new Socket();
				await socket.ConnectAsync(new InetSocketAddress(ip, port), (int)timeout.TotalMilliseconds);
				socket.Close();
				return true;
			}
			catch (ConnectException ce)
			{
				Log.Error(ce, "connection exception");
				return false;
			}
			catch (Exception ex)
			{
				Log.Error(ex, "exception occured");
				return false;
			}
		}
	}
}