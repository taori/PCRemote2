using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Features.WakeOnLan;
using Amusoft.PCR.Mobile.Droid.Domain.Server.HostControl;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Grpc.Core;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.HostSelection
{
	public class HostSelectionFragment : SmartFragment, SwipeRefreshLayout.IOnRefreshListener
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(HostSelectionFragment));
		
		private RecyclerView _recyclerView;
		private SwipeRefreshLayout _swipeRefreshLayout;
		private Button _wakeUpButton;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_selection_main, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.listView); 
			_recyclerView.SetAdapter(HostSelectionDataSource.Instance);

			_swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout1);
			_swipeRefreshLayout.SetOnRefreshListener(this);

			_wakeUpButton = view.FindViewById<Button>(Resource.Id.button_wake_up_host);
			_wakeUpButton.Click += WakeUpButtonOnClick;

			HostSelectionDataSource.Instance.ItemClicked -= InstanceOnItemClicked;
			HostSelectionDataSource.Instance.ItemClicked += InstanceOnItemClicked;
		}

		public override void OnResume()
		{
			OnRefresh();
			base.OnResume();
		}

		private void WakeUpButtonOnClick(object sender, EventArgs e)
		{
			using (var transaction = Activity.SupportFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Wake on LAN");
				transaction.ReplaceContentAnimated(new WakeOnLanFragment());
				transaction.Commit();
			}
		}

		public void OnRefresh()
		{
			_swipeRefreshLayout.Refreshing = true;
			var taskScheduler = TaskScheduler.FromCurrentSynchronizationContext();
			HostSelectionDataSource.Instance.RequestHostRepliesAsync()
				.ContinueWith(prev =>
				{
					HostSelectionDataSource.Instance.NotifyDataSetChanged();
					_swipeRefreshLayout.Refreshing = false;
				}, taskScheduler);
		}

		private async Task<LoginResponse> GetLoginResponseAsync(GrpcApplicationAgent grpcApplicationAgent, JwtLoginCredentials input)
		{
			try
			{
				return await grpcApplicationAgent.FullDesktopClient.LoginAsync(new LoginRequest() { User = input.User, Password = input.Password });
			}
			catch (RpcException exception)
			{
				Log.Error(exception, "Login error");
				return new LoginResponse() { InvalidCredentials = true };
			}
		}

		private async void InstanceOnItemClicked(object sender, HostSelectionDataSource.ServerDataItem e)
		{
			var endpointAddress = new HostEndpointAddress(e.EndPoint.Address.ToString(), e.HttpsPorts[0]);
			try
			{
				var portOpen = await SocketHelper.IsPortOpenAsync(endpointAddress.IpAddress, endpointAddress.Port, TimeSpan.FromSeconds(1));
				if (!portOpen)
				{
					ToastHelper.Display($"Failed to connect to {endpointAddress.FullAddress}", ToastLength.Long);
					return;
				}
				
				using (var agent = GrpcApplicationAgentFactory.Create(GrpcChannelHub.GetChannelFor(endpointAddress)))
				{
					var authenticated = await CheckIsAuthenticatedAsync(agent);
					if (!authenticated)
					{
						if (!await TryUpdateAuthenticationAsync(agent, endpointAddress))
							return;
					}

					NavigateToHost(e);
				}
			}
			catch (Exception exception)
			{
				ToastHelper.Display($"Failed to connect to {endpointAddress.FullAddress}", ToastLength.Long);
				Log.Error(exception);
			}
		}

		private void NavigateToHost(HostSelectionDataSource.ServerDataItem e)
		{
			var fragment = new HostControlFragment();
			fragment.WithAgent(e.EndPoint.Address.ToString(), e.MachineName, e.HttpsPorts[0]);
			fragment.DisplayListHeader = true;
			var bundle = new Bundle();
			bundle.PutInt(HostControlFragment.ArgumentTargetPort, e.HttpsPorts[0]);
			bundle.PutString(HostControlFragment.ArgumentTargetAddress, e.EndPoint.Address.ToString());
			bundle.PutString(HostControlFragment.ArgumentTargetMachineName, e.MachineName);
			fragment.Arguments = bundle;

			if (Activity?.SupportFragmentManager == null)
			{
				Log.Error("-------------------------------- ERROR HAPPENED");
			}
			using (var transaction = Activity.SupportFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle(e.MachineName);
				transaction.ReplaceContentAnimated(fragment);
				transaction.Commit();
			}
		}

		private async Task<bool> TryUpdateAuthenticationAsync(GrpcApplicationAgent agent, HostEndpointAddress endpointAddress)
		{
			var input = await LoginDialog.GetInputAsync("Authentication required");
			if (input == null)
			{
				ToastHelper.Display("Authentication required", ToastLength.Long);
				return false;
			}

			var loginResponse = await GetLoginResponseAsync(agent, input);
			if (loginResponse.InvalidCredentials)
			{
				ToastHelper.Display("Invalid credentials", ToastLength.Long);
				return false;
			}

			var authenticationStorage = new AuthenticationStorage(endpointAddress);
			await authenticationStorage.UpdateAsync(loginResponse.AccessToken, loginResponse.RefreshToken).ConfigureAwait(false);

			return true;
		}

		private static async Task<bool> CheckIsAuthenticatedAsync(GrpcApplicationAgent agent)
		{
			var authenticated = true;
			CheckIsAuthenticatedResponse response = null;
			try
			{
				response = await agent.FullDesktopClient.CheckIsAuthenticatedAsync(new CheckIsAuthenticatedRequest()).ResponseAsync;
			}
			catch (RpcException exception) when (exception.Status.StatusCode == StatusCode.Unauthenticated)
			{
				authenticated = false;
			}
			catch (RpcException exception)
			{
				Log.Error(exception);
				ToastHelper.Display("Failed to check authentication state", ToastLength.Long);
				authenticated = false;
			}

			return authenticated && response.Result;
		}
	}
}