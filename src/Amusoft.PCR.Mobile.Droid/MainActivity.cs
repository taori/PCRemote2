using System;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Log;
using Amusoft.PCR.Mobile.Droid.Domain.Networking;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.AppCompat.Widget;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using Grpc.Core;
using Grpc.Net.Client;
using NLog;
using NLog.Config;
using Xamarin.Essentials;
using Environment = System.Environment;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Amusoft.PCR.Mobile.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(MainActivity));

		private UdpBroadcastReceiver _broadcastReceiver;

		private UdpReceiveResult _lastMessage;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
			Application.RegisterActivityLifecycleCallbacks(ActivityLifecycleCallbacks.Instance);

			NLog.LogManager.Configuration = new XmlLoggingConfiguration("assets/nlog.config");
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);
			_broadcastReceiver = new UdpBroadcastReceiver(55863);
			_broadcastReceiver.MessageReceived.Subscribe(UdpMessageReceived);
			_broadcastReceiver.Start();

			Toolbar toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			FloatingActionButton fab = FindViewById<FloatingActionButton>(Resource.Id.fab);
			fab.Click += FabOnClick;

			var shutdownButton = FindViewById<Button>(Resource.Id.btnShutdown);
			shutdownButton.Click += ShutdownButtonOnClick;
			var abortShutdownButton = FindViewById<Button>(Resource.Id.btnAbortShutdown);
			abortShutdownButton.Click += AbortShutdownButtonOnClick;

			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
			drawer.AddDrawerListener(toggle);
			toggle.SyncState();

			NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
			navigationView.SetNavigationItemSelectedListener(this);
		}

		private async void AbortShutdownButtonOnClick(object sender, EventArgs e)
		{
			try
			{
				SendBroadcastMessage();
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


		private static GrpcApplicationAgent CreateApplicationAgent()
		{
			var uriString = "https://192.168.0.135:5001";
			// var uriString = "https://192.168.0.135:44365";
			var baseAddress = new Uri(uriString);

			var channelOptions = new GrpcChannelOptions()
			{
				DisposeHttpClient = true,
				HttpClient = GrpcWebHttpClientFactory.Create(baseAddress, new AuthenticationSurface(uriString))
			};
			var channel = GrpcChannel.ForAddress(new Uri(uriString), channelOptions);

			return new GrpcApplicationAgent(channel);
		}

		private static void SendBroadcastMessage()
		{
			using var client = new UdpClient();
			client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			client.ExclusiveAddressUse = false;
			var bytes = Encoding.UTF8.GetBytes("test");
			client.Send(bytes, bytes.Length, new IPEndPoint(IPAddress.Broadcast, 55863));
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

		private void UdpMessageReceived(UdpReceiveResult obj)
		{
			Log.Trace("Received UDP message");
			var textView = FindViewById<TextView>(Resource.Id.textView);
			textView.Text = obj.RemoteEndPoint.ToString();
		}

		public override void OnBackPressed()
		{
			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			if (drawer.IsDrawerOpen(GravityCompat.Start))
			{
				drawer.CloseDrawer(GravityCompat.Start);
			}
			else
			{
				base.OnBackPressed();
			}
		}

		public override bool OnCreateOptionsMenu(IMenu menu)
		{
			MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return true;
		}

		public override bool OnOptionsItemSelected(IMenuItem item)
		{
			int id = item.ItemId;
			if (id == Resource.Id.action_settings)
			{
				var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
				var path = Path.Combine(root, "logs", "nlog.csv");
				if (File.Exists(path))
				{
					File.Delete(path);
				}
				return true;
			}

			if (id == Resource.Id.action_view_logs)
			{
				using (var transaction = SupportFragmentManager.BeginTransaction())
				{
					transaction.Replace(Resource.Id.app_bar_main_container, new LogFragment());
					transaction.AddToBackStack(null);
					transaction.SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentFade);
					transaction.Commit();
				}
				return true;
			}

			if (id == Resource.Id.action_clear_storage)
			{
				var surface = new AuthenticationSurface("https://192.168.0.135:5001");
				Task.Run(() => surface.UpdateTokenStoreAsync(null));
				return true;
			}

			return base.OnOptionsItemSelected(item);
		}

		private async void FabOnClick(object sender, EventArgs eventArgs)
		{
			Log.Debug("Fab pressed");
			var sent = await SendBroadcast();
			var message = _lastMessage == default
				? $"Sent {sent} bytes"
				: Encoding.UTF8.GetString(_lastMessage.Buffer);
			View view = (View)sender;

			Snackbar.Make(view, message, Snackbar.LengthLong)
				.SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
		}

		private static async Task<int> SendBroadcast()
		{
			Log.Debug("Sending broadcast");
			var broadcaster = new UdpClient();
			var datagram = Encoding.UTF8.GetBytes("message from android client");
			broadcaster.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
			broadcaster.ExclusiveAddressUse = false;
			// broadcaster.AllowNatTraversal(true);
			return await broadcaster.SendAsync(datagram, datagram.Length, new IPEndPoint(IPAddress.Broadcast, 55863));
		}

		public bool OnNavigationItemSelected(IMenuItem item)
		{
			int id = item.ItemId;

			if (id == Resource.Id.nav_camera)
			{
				// Handle the camera action
			}
			else if (id == Resource.Id.nav_gallery)
			{

			}
			else if (id == Resource.Id.nav_slideshow)
			{

			}
			else if (id == Resource.Id.nav_manage)
			{

			}
			else if (id == Resource.Id.nav_share)
			{

			}
			else if (id == Resource.Id.nav_send)
			{

			}

			DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			drawer.CloseDrawer(GravityCompat.Start);
			return true;
		}

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Log.Debug("Requesting permissions");
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}

