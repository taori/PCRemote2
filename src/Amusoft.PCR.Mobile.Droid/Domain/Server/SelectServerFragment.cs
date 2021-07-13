using System;
using System.Diagnostics;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Android.Material.Snackbar;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server
{
	public class SelectServerFragment : Fragment, SwipeRefreshLayout.IOnRefreshListener
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SelectServerFragment));


		private RecyclerView _recyclerView;
		private SwipeRefreshLayout _swipeRefreshLayout;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.serverSelection, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.listView); 
			_recyclerView.SetAdapter(SelectServerFragmentDataSource.Instance);

			_swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout1);
			_swipeRefreshLayout.SetOnRefreshListener(this);

			SelectServerFragmentDataSource.Instance.ItemClicked -= InstanceOnItemClicked;
			SelectServerFragmentDataSource.Instance.ItemClicked += InstanceOnItemClicked;

			HandleForceBroadcastButton(view);
		}

		private void HandleForceBroadcastButton(View view)
		{
			var button = view.FindViewById<Button>(Resource.Id.button1);
			button.Click += ButtonOnClick;
			HideButton(button);
		}

		[Conditional("RELEASE")]
		private void HideButton(Button button)
		{
			button.Visibility = ViewStates.Gone;
		}

		public void OnRefresh()
		{
			SelectServerFragmentDataSource.Instance.NotifyDataSetChanged();
			_swipeRefreshLayout.Refreshing = false;
		}

		private void InstanceOnItemClicked(object sender, SelectServerFragmentDataSource.ServerDataItem e)
		{
			var fragment = new ServerControlFragment();
			var bundle = new Bundle();
			bundle.PutString(ServerControlFragment.ArgumentTargetAddress, e.EndPoint.Address.ToString());
			fragment.Arguments = bundle;
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.AddToBackStack(null);
				transaction.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
				transaction.Replace(Resource.Id.content_display_frame, fragment);

				transaction.Commit();
			}
		}

		private static void SendBroadcastMessage()
		{
			using var client = new UdpClient();
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			client.ExclusiveAddressUse = false;
			var bytes = Encoding.UTF8.GetBytes(GrpcHandshakeFormatter.Write("TestMachine", new[] { 55863 }));
			client.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, 55863));
		}

		private void ButtonOnClick(object sender, EventArgs e)
		{
			SendBroadcastMessage();
		}
	}
}