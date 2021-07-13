using System;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Grpc.Net.Client;
using NLog;
using Exception = System.Exception;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server
{
	public class ServerControlFragment : Fragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(ServerControlFragment));

		private TextView _header;
		private GrpcApplicationAgent _agent;
		private RecyclerView _recyclerView;

		public const string ArgumentTargetAddress = "Address";
		public const string ArgumentTargetPort = "Port";
		
		public override void OnSaveInstanceState(Bundle outState)
		{
			Arguments.PutString(ArgumentTargetAddress, _header.Text);
			base.OnSaveInstanceState(outState);
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_main, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_header = view.FindViewById<TextView>(Resource.Id.textView1);
			_header.Text = Arguments.GetString(ArgumentTargetAddress, "No Address");

			_agent = CreateApplicationAgent();

			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.listView);
			var dataSource = new ServerControlFragmentDataSource(_agent);
			_recyclerView.SetAdapter(dataSource);
			dataSource.CallStarted += DataSourceOnCallStarted;
			dataSource.CallFinished += DataSourceOnCallFinished;
			dataSource.CallFailed += DataSourceOnCallFailed;

			dataSource.SetupItems(ServerControlFragmentDataSource.DataSourceLevel.Top);
		}

		private void DataSourceOnCallFailed(object sender, Exception e)
		{
			Log.Error(e);
		}

		private void DataSourceOnCallFinished(object sender, int e)
		{
			Log.Info("Call finished. Running now: {Value}", e);
		}

		private void DataSourceOnCallStarted(object sender, int e)
		{
			Log.Info("Call started. Running now: {Value}", e);
		}

		protected override void Dispose(bool disposing)
		{
			_agent?.Dispose();
			base.Dispose(disposing);
		}

		private async void AbortShutdownButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				// SendBroadcastMessage();
				using (var host = CreateApplicationAgent())
				{
					Log.Debug("Sending Abort Shutdown");
					// host.DesktopIntegrationClient.AbortShutDownAsync(new AbortShutdownRequest(), deadline: DateTime.UtcNow.AddSeconds(5));
					await host.DesktopIntegrationClient.AbortShutDownAsync(new AbortShutdownRequest());
					Log.Debug("Sent Abort Shutdown");
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}


		private GrpcApplicationAgent CreateApplicationAgent()
		{
			// var uriString = "https://192.168.0.135:5001";
			// var uriString = "https://192.168.0.135:44365";
			var targetAddress = Arguments.GetString(ArgumentTargetAddress);
			var targetPort = Arguments.GetString(ArgumentTargetPort, "5001");
			var uriString = $"https://{targetAddress}:{targetPort}";
			var baseAddress = new Uri(uriString);

			var channelOptions = new GrpcChannelOptions()
			{
				DisposeHttpClient = true,
				HttpClient = GrpcWebHttpClientFactory.Create(baseAddress, new AuthenticationSurface(uriString))
			};
			var channel = GrpcChannel.ForAddress(new Uri(uriString), channelOptions);

			return new GrpcApplicationAgent(channel);
		}

		private async void ShutdownButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				using (var host = CreateApplicationAgent())
				{
					Log.Debug("Sending Shutdown");
					// host.DesktopIntegrationClient.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Seconds = 60 }, deadline: DateTime.UtcNow.AddSeconds(5));
					await host.DesktopIntegrationClient.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Seconds = 60 });
					Log.Debug("Sent Shutdown");
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
			}
		}
	}
}