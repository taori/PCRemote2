using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.CustomControls;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Log;
using Amusoft.PCR.Mobile.Droid.Domain.Networking;
using Amusoft.PCR.Mobile.Droid.Domain.Server;
using Amusoft.PCR.Mobile.Droid.Domain.Server.HostSelection;
using Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using Android.Content;
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
using Java.Lang;
using NLog;
using NLog.Config;
using Xamarin.Essentials;
using Environment = System.Environment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;

namespace Amusoft.PCR.Mobile.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(MainActivity));
		private LoaderPanel? _loaderPanel;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
			Application.RegisterActivityLifecycleCallbacks(ActivityLifecycleCallbacks.Instance);

			NLog.LogManager.Configuration = new XmlLoggingConfiguration("assets/nlog.config");
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			_loaderPanel = FindViewById<LoaderPanel>(Resource.Id.content_display_frame_loading_panel);
			GrpcRequestObserver.CallRunning -= UpdateLoaderPanel;
			GrpcRequestObserver.CallFinished -= UpdateLoaderPanel;
			GrpcRequestObserver.CallRunning += UpdateLoaderPanel;
			GrpcRequestObserver.CallFinished += UpdateLoaderPanel;

			NotificationHelper.CreateNotificationChannel(this);
			AddBackgroundServices(this);

			var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open,
				Resource.String.navigation_drawer_close);
			drawer.AddDrawerListener(toggle);
			toggle.SyncState();

			var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
			navigationView.SetNavigationItemSelectedListener(this);

			using (var transaction = SupportFragmentManager.BeginTransaction())
			{
				transaction.Replace(Resource.Id.content_display_frame, new HostSelectionFragment());
				transaction.DisallowAddToBackStack();
				transaction.Commit();
			}
		}

		private void UpdateLoaderPanel(object sender, int e)
		{
			if (_loaderPanel != null)
			{
				_loaderPanel.OverlayVisible = e > 0;
			}
		}

		private void AddBackgroundServices(MainActivity mainActivity)
		{
		}

		private static void AddBackgroundService(MainActivity mainActivity, Type type)
		{
			var intent = new Intent(mainActivity, type);
			if (Android.OS.Build.VERSION.SdkInt >= Android.OS.BuildVersionCodes.O)
			{
				mainActivity.StartForegroundService(intent);
			}
			else
			{
				mainActivity.StartService(intent);
			}
		}

		public override void OnBackPressed()
		{
			if (BackStackHandler.PopCall())
			{
				base.OnBackPressed();
				return;
			}

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

		public override void OnRequestPermissionsResult(int requestCode, string[] permissions,
			[GeneratedEnum] Android.Content.PM.Permission[] grantResults)
		{
			Log.Debug("Requesting permissions");
			Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

			base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
		}
	}
}