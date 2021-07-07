using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Java.Net;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Networking
{
	public class UdpBroadcastReceiver : IDisposable
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(UdpBroadcastReceiver));

		private readonly int _port;
		private readonly CancellationTokenSource _cts;

		private readonly Subject<UdpReceiveResult> _messageReceived = new Subject<UdpReceiveResult>();

		public IObservable<UdpReceiveResult> MessageReceived => _messageReceived;

		public bool IsReceiverDead { get; set; }

		public UdpBroadcastReceiver(int port)
		{
			_port = port;
			_cts = new CancellationTokenSource();
		}

		public void Abort() => _cts.Cancel(true);

		public async void Start()
		{
			try
			{
				Log.Info($"{nameof(UdpBroadcastReceiver)} is listening to [{_port}].");

				var udpClient = new UdpClient()
				{
					ExclusiveAddressUse = false,
					EnableBroadcast = true
				};
				udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				udpClient.Client.Bind(new IPEndPoint(IPAddress.Any, _port));
				while (!_cts.IsCancellationRequested)
				{
					try
					{
						Log.Trace("Waiting for UDP message on port [{Port}]", _port);
						var result = await udpClient.ReceiveAsync();
						Log.Trace("Received message from [{Ip}]", result.RemoteEndPoint.ToString());
						_messageReceived.OnNext(result);
					}
					catch (Exception e)
					{
						Log.Error(e, "Unexpected error occured - Terminating receiver");
						_messageReceived.OnError(e);
						_messageReceived.OnCompleted();
						IsReceiverDead = true;
					}
				}

				Log.Info("Beacon receiver terminating.");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		public void Dispose()
		{
			if (_messageReceived.IsDisposed)
				return;

			_cts?.Dispose();
			_messageReceived?.Dispose();
		}
	}

	public static class EndpointExtensions
	{
		public static string Prettify(this IPEndPoint source)
		{
			return $"{source.Address.ToString().PadLeft(15, ' ')}:{source.Port.ToString().PadLeft(5, ' ')}";
		}
	}
}