using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Server.HostControl;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.HostSelection
{
	public class HostSelectionFragment : Fragment, SwipeRefreshLayout.IOnRefreshListener
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(HostSelectionFragment));
		
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
			_recyclerView.SetAdapter(HostSelectionDataSource.Instance);

			_swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout1);
			_swipeRefreshLayout.SetOnRefreshListener(this);

			HostSelectionDataSource.Instance.ItemClicked -= InstanceOnItemClicked;
			HostSelectionDataSource.Instance.ItemClicked += InstanceOnItemClicked;
		}

		public void OnRefresh()
		{
			HostSelectionDataSource.Instance.NotifyDataSetChanged();
			_swipeRefreshLayout.Refreshing = false;
		}

		private void InstanceOnItemClicked(object sender, HostSelectionDataSource.ServerDataItem e)
		{
			var fragment = new HostControlFragment();
			fragment.DisplayListHeader = true;
			var bundle = new Bundle();
			bundle.PutInt(HostControlFragment.ArgumentTargetPort, e.HttpsPorts[0]);
			bundle.PutString(HostControlFragment.ArgumentTargetAddress, e.EndPoint.Address.ToString());
			bundle.PutString(HostControlFragment.ArgumentTargetMachineName, e.MachineName);
			fragment.Arguments = bundle;

			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.AddToBackStack(null);
				transaction.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
				transaction.Replace(Resource.Id.content_display_frame, fragment);

				transaction.Commit();
			}
		}
	}
}