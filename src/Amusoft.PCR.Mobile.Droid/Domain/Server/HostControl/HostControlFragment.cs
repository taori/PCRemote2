using System;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using AndroidX.RecyclerView.Widget;
using Grpc.Net.Client;
using NLog;
using Exception = System.Exception;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.HostControl
{
	public class HostControlFragment : Fragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(HostControlFragment));

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
			var dataSource = new HostControlDataSource(_agent);
			_recyclerView.SetAdapter(dataSource);
			dataSource.CallStarted += DataSourceOnCallStarted;
			dataSource.CallFinished += DataSourceOnCallFinished;
			dataSource.CallFailed += DataSourceOnCallFailed;

			dataSource.SetupItems(HostControlDataSource.DataSourceLevel.Top);
		}

		private void DataSourceOnCallFailed(object sender, Exception e)
		{
			// var message = _lastMessage == default
			// 	? "Replace with your own action"
			// 	: Encoding.UTF8.GetString(_lastMessage.Buffer);
			// View view = (View)sender;
			//
			// Snackbar.Make(view, message, Snackbar.LengthLong)
			// 	.SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
			var toast = new Toast(Context);
			toast.Duration = ToastLength.Short;
			// toast.SetGravity(GravityFlags.Bottom);
			toast.SetText("An error occured.");
			toast.Show();
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


		private GrpcApplicationAgent CreateApplicationAgent()
		{
			// var uriString = "https://192.168.0.135:5001";
			// var uriString = "https://192.168.0.135:44365";
			var targetAddress = Arguments.GetString(ArgumentTargetAddress);
			var targetPort = Arguments.GetString(ArgumentTargetPort, "5001");
			var uriString = $"https://{targetAddress}:{targetPort}";
			var baseAddress = new Uri(uriString);
			Log.Debug("Creating agent with connection {Address}", uriString);

			var channelOptions = new GrpcChannelOptions()
			{
				DisposeHttpClient = true,
				HttpClient = GrpcWebHttpClientFactory.Create(baseAddress, new AuthenticationSurface(uriString))
			};
			var channel = GrpcChannel.ForAddress(new Uri(uriString), channelOptions);

			return new GrpcApplicationAgent(channel);
		}
	}
}