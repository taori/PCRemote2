using System;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Subjects;
using System.Threading;
using System.Threading.Tasks;

namespace Amusoft.Toolkit.Networking
{
	public class UdpBroadcastCommunicationChannelSettings
	{
		public UdpBroadcastCommunicationChannelSettings(int port)
		{
			Port = port;
		}

		public int Port { get; set; }

		public bool AllowNatTraversal { get; set; }

		public Action<Exception> ReceiveErrorHandler { get; set; }
	}

	public class UdpBroadcastCommunicationChannel : IDisposable
	{
		private UdpClient _client;
		private readonly UdpBroadcastCommunicationChannelSettings _settings;
		private CancellationTokenSource _cts;
		
		private readonly Subject<UdpReceiveResult> _messageReceived = new Subject<UdpReceiveResult>();

		public IObservable<UdpReceiveResult> MessageReceived => _messageReceived;

		public UdpBroadcastCommunicationChannel(UdpBroadcastCommunicationChannelSettings settings)
		{
			_settings = settings;
			_client = new UdpClient();

			if (_settings.AllowNatTraversal)
				_client.AllowNatTraversal(true);

			_client.EnableBroadcast = true;
			_client.ExclusiveAddressUse = false;
			_client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			_client.Client.Bind(new IPEndPoint(IPAddress.Any, _settings.Port));
		}

		public async void StartListening(CancellationToken token)
		{
			await StartListeningAsync(token);
		}

		public async Task StartListeningAsync(CancellationToken token)
		{
			_cts?.Dispose();
			_cts = CancellationTokenSource.CreateLinkedTokenSource(token);
			await Task.Run(async () =>
			{
				try
				{
					while (true)
					{
						var result = await _client.ReceiveAsync();
						_messageReceived.OnNext(result);
					}
				}
				catch (OperationCanceledException)
				{
					_messageReceived.OnCompleted();
				}
				catch (Exception e)
				{
					_settings.ReceiveErrorHandler?.Invoke(e);
					_messageReceived.OnError(e);
				}
			}, _cts.Token);
		}

		public async Task<bool> SendAsync(byte[] bytes)
		{
			var byteLength = bytes.Length;
			return await _client.SendAsync(bytes, byteLength, new IPEndPoint(IPAddress.Broadcast, _settings.Port)) == byteLength;
		}

		public void Dispose()
		{
			_cts?.Dispose();
			_client.Dispose();
		}
	}
}