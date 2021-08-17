using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Reactive.Disposables;
using System.Reactive.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Networking;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.Toolkit.Networking;
using Android.App;
using Android.Media;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using Xamarin.Essentials;
using Logger = NLog.Logger;
using LogManager = NLog.LogManager;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.HostSelection
{
	public class HostSelectionDataSource : RecyclerView.Adapter
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(HostSelectionDataSource));

		public class ServerDataItem
		{
			public IPEndPoint EndPoint { get; set; }

			public DateTime LastSeen { get; set; }

			public int[] HttpsPorts { get; set; }

			public string MachineName { get; set; }
		}

		public static readonly HostSelectionDataSource Instance = new HostSelectionDataSource();

		private List<UdpBroadcastCommunicationChannel> _receivers = new List<UdpBroadcastCommunicationChannel>();
		private CompositeDisposable _subscriptions = new CompositeDisposable();
		private List<ServerDataItem> _dataItems = new List<ServerDataItem>();

		public event EventHandler<ServerDataItem> ItemClicked;
		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_subscriptions.Dispose();
				_subscriptions = null;

				foreach (var channel in _receivers)
				{
					channel.Dispose();
				}
				_receivers.Clear();
			}
			base.Dispose(disposing);
		}

		private HostSelectionDataSource()
		{
			ClearPortListeners();
			BuildListenersFromStorage();
			AddDebugEndpoint();
		}

		[Conditional("DEBUG")]
		private void AddDebugEndpoint()
		{
			_dataItems.Add(new ServerDataItem()
			{
				EndPoint = new IPEndPoint(IPAddress.Parse("192.168.0.135"), 5001),
				HttpsPorts = new []{ 5001 },
				MachineName = "Debug Endpoint"
			});
		}

		private async void BuildListenersFromStorage()
		{
			var portString = await SecureStorage.GetAsync("ServerBroadcastPorts");
			if (portString == null)
			{
				BindListener(55863);
			}
			else
			{
				var ports = portString.Split(';');
				foreach (var portSplit in ports)
				{
					if (int.TryParse(portSplit, out var parsedPort))
					{
						BindListener(parsedPort);
					}
					else
					{
						Log.Error("Failed to parse port value from {String}", portSplit);
					}
				}
			}
		}

		public void BindListener(int portId)
		{
			Log.Debug("Binding to port {Id}", portId);
			var receiver = new UdpBroadcastCommunicationChannel(CreateChannelSettings(portId));
			_subscriptions.Add(receiver.MessageReceived
				.Subscribe(ServerContactReceived));

			receiver.StartListening(CancellationToken.None);
			_receivers.Add(receiver);
		}

		private static UdpBroadcastCommunicationChannelSettings CreateChannelSettings(int portId)
		{
			var channelSettings = new UdpBroadcastCommunicationChannelSettings(portId);
			channelSettings.ReceiveErrorHandler = ReceiveErrorHandler;
			return channelSettings;
		}

		private static void ReceiveErrorHandler(Exception obj)
		{
			Log.Error(obj, "Receive error");
		}

		private bool CompareEndpoints(IPEndPoint a, IPEndPoint b)
		{
			if (a.AddressFamily != b.AddressFamily)
				return false;
			
			if (a.Address.ToString() != b.Address.ToString())
				return false;

			return true;
		}

		private void ServerContactReceived(UdpReceiveResult obj)
		{
			Log.Debug("Received UDP package from {Address}", obj.RemoteEndPoint.Address.ToString());
			var message = GrpcHandshakeFormatter.Parse(obj.Buffer);
			if (string.IsNullOrEmpty(message.MachineName))
			{
				Log.Debug("Discarding Endpoint because hostname is empty.");
				return;
			}

			var index = _dataItems.FindIndex(d => CompareEndpoints(d.EndPoint, obj.RemoteEndPoint));
			if (index >= 0)
			{
				Log.Trace("Updating endpoint for {Name} to {@Ports}", message.MachineName, message.Ports);

				_dataItems[index].LastSeen = DateTime.Now;
				_dataItems[index].HttpsPorts = message.Ports;
				_dataItems[index].MachineName = message.MachineName;
				MainThread.BeginInvokeOnMainThread(() => NotifyItemChanged(index));
				// NotifyItemChanged(index);
			}
			else
			{
				_dataItems.Add(new ServerDataItem()
				{
					HttpsPorts = message.Ports,
					MachineName = message.MachineName,
					LastSeen = DateTime.Now, 
					EndPoint = obj.RemoteEndPoint
				});

				var indexUpdate = _dataItems.Count - 1;

				Log.Trace("Adding endpoint for {Name}, at address {Endpoint} to {@Ports}", message.MachineName, obj.RemoteEndPoint, message.Ports);
				MainThread.BeginInvokeOnMainThread(() => NotifyItemInserted(indexUpdate));
				// NotifyItemInserted(_dataItems.Count - 1);
			}
		}

		private void ClearPortListeners()
		{
			_dataItems.Clear();
		}

		public override void OnBindViewHolder(RecyclerView.ViewHolder holder, int position)
		{
			var dataItem = _dataItems[position];
			if(holder is SelectServerFragmentDataSourceItem holderItem)
			{
				holderItem.ItemClicked -= HolderItemOnItemClicked;
				holderItem.ItemClicked += HolderItemOnItemClicked;

				// var difference = DateTime.Now - dataItem.LastSeen;
				// var differenceString = difference.TotalSeconds >= 60
				// 	? $"{difference.TotalMinutes:0} minutes ago"
				// 	: $"{difference.TotalSeconds:0} seconds ago";

				holderItem.ViewButton.Text = $"{dataItem.EndPoint.Address} - {dataItem.MachineName}";
			}
		}

		private void HolderItemOnItemClicked(object sender, EventArgs e)
		{
			if (sender is SelectServerFragmentDataSourceItem holder)
			{
				if (holder.AbsoluteAdapterPosition < 0)
				{
					ToastHelper.Display("Error - Restart application", ToastLength.Short);
					Log.Debug("holder.AbsoluteAdapterPosition < 0");
					return;
				}
					
				ItemClicked?.Invoke(null, _dataItems[holder.AbsoluteAdapterPosition]);
			}
		}

		public override RecyclerView.ViewHolder OnCreateViewHolder(ViewGroup parent, int viewType)
		{
			var inflater = LayoutInflater.FromContext(parent.Context);
			var view = inflater.Inflate(Resource.Layout.server_selection_item, parent, false);

			var viewHolder = new SelectServerFragmentDataSourceItem(view);
			return viewHolder;
		}

		public override int ItemCount => _dataItems.Count;

		private class SelectServerFragmentDataSourceItem : RecyclerView.ViewHolder
		{
			public SelectServerFragmentDataSourceItem(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
			{
			}

			public SelectServerFragmentDataSourceItem(View itemView) : base(itemView)
			{
				ViewButton = itemView as Button;
				itemView.Click += ItemViewOnClick;
			}

			public event EventHandler ItemClicked;

			private void ItemViewOnClick(object sender, EventArgs e)
			{
				ItemClicked?.Invoke(this, e);
			}

			public TextView ViewButton { get; }
		}

		public async Task RequestHostRepliesAsync()
		{
			foreach (var channel in _receivers)
			{
				await channel.SendAsync(System.Text.Encoding.UTF8.GetBytes(GrpcHandshakeClientMessage.Message));
			}
		}
	}
}