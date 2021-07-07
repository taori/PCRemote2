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
						var result = await udpClient.ReceiveAsync();
						Log.Trace("Received message from {Ip}", result.RemoteEndPoint.ToString());
						_messageReceived.OnNext(result);
					}
					catch (Exception e)
					{
						_messageReceived.OnError(e);
						_messageReceived.OnCompleted();
						IsReceiverDead = true;
					}
				}
				// _udpClient.BeginReceive(OnUdpDataReceived, _udpClient);

				// var socket = new DatagramSocket(beaconPort, InetAddress.GetByAddress(Encoding.UTF8.GetBytes("0.0.0.0")));
				// socket.Broadcast = true;
				// var bytes = new byte[128];
				// while (!_cts.Token.IsCancellationRequested)
				// {
				// 	var packet = new DatagramPacket(bytes, 128);
				// 	await socket.ReceiveAsync(packet);
				// }
				//
				// using (var broadcaster = new UdpClient())
				// {
				// 	var ep = new IPEndPoint(IPAddress.Any, beaconPort);
				// 	broadcaster.ExclusiveAddressUse = false;
				// 	// broadcaster.AllowNatTraversal(true);
				// 	broadcaster.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
				// 	broadcaster.Client.Bind(ep);
				// 	Log.Info($"Binding ({nameof(UdpReceiver)}) on [{ep}]");
				//
				// 	while (!_cts.Token.IsCancellationRequested)
				// 	{
				// 		var receive = await broadcaster.ReceiveAsync();
				// 		Log.Debug($"<--- [{receive.RemoteEndPoint.Prettify()}] {Encoding.UTF8.GetString(receive.Buffer)}");
				// 	}
				// }

				Log.Info("Beacon receiver terminating.");
			}
			catch (Exception e)
			{
				Log.Error(e);
			}
		}

		// private static void OnUdpDataReceived(IAsyncResult result)
		// {
		// 	// Debug.WriteLine($">>> in receive");
		//
		// 	var udpClient = result.AsyncState as UdpClient;
		// 	if (udpClient == null)
		// 		return;
		//
		// 	IPEndPoint remoteAddr = null;
		// 	var recvBuffer = udpClient.EndReceive(result, ref remoteAddr);
		//
		// 	// Debug.WriteLine($"MESSAGE FROM: {remoteAddr.Address}:{remoteAddr.Port}, MESSAGE SIZE: {recvBuffer?.Length ?? 0}");
		//
		// 	udpClient.BeginReceive(OnUdpDataReceived, udpClient);
		// }

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