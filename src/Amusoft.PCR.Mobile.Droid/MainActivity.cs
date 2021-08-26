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
using Amusoft.PCR.Mobile.Droid.Domain.Settings;
using Amusoft.PCR.Mobile.Droid.Domain.Updates;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using Android.Content;
using Android.Graphics;
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
using Java.Net;
using Microsoft.AppCenter;
using Microsoft.AppCenter.Analytics;
using Microsoft.AppCenter.Crashes;
using NLog;
using NLog.Config;
using Xamarin.Essentials;
using Environment = System.Environment;
using FragmentManager = AndroidX.Fragment.App.FragmentManager;
using Path = System.IO.Path;
using Toolbar = AndroidX.AppCompat.Widget.Toolbar;
using Uri = Android.Net.Uri;

namespace Amusoft.PCR.Mobile.Droid
{
	[Activity(Label = "@string/app_name", Theme = "@style/AppTheme.NoActionBar", MainLauncher = true)]
	public class MainActivity : AppCompatActivity, NavigationView.IOnNavigationItemSelectedListener
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(MainActivity));
		private LoaderPanel _loaderPanel;

		protected override void OnCreate(Bundle savedInstanceState)
		{
			AppCenter.Start("ae5403f8-deb8-41b5-8f6c-ebd2bd41648f", typeof(Crashes), typeof(Analytics));
			Analytics.TrackEvent("Application launching");

			base.OnCreate(savedInstanceState);
			
			AppContext.SetSwitch("System.Net.Http.SocketsHttpHandler.Http2UnencryptedSupport", true);
			Application.UnregisterActivityLifecycleCallbacks(ActivityLifecycleCallbacks.Instance);
			Application.RegisterActivityLifecycleCallbacks(ActivityLifecycleCallbacks.Instance);
			
			ApplicationContext.RegisterReceiver(AbortBroadcastReceiver.Instance, new IntentFilter(AbortBroadcastReceiver.ActionKindHibernate));
			ApplicationContext.RegisterReceiver(AbortBroadcastReceiver.Instance, new IntentFilter(AbortBroadcastReceiver.ActionKindRestart));
			ApplicationContext.RegisterReceiver(AbortBroadcastReceiver.Instance, new IntentFilter(AbortBroadcastReceiver.ActionKindShutdown));

			NLog.LogManager.Configuration = new XmlLoggingConfiguration("assets/nlog.config");
			Xamarin.Essentials.Platform.Init(this, savedInstanceState);
			SetContentView(Resource.Layout.activity_main);

			_loaderPanel = FindViewById<LoaderPanel>(Resource.Id.content_display_frame_loading_panel);
			GrpcRequestObserver.CallRunning -= UpdateLoaderPanel;
			GrpcRequestObserver.CallFinished -= UpdateLoaderPanel;
			GrpcRequestObserver.CallRunning += UpdateLoaderPanel;
			GrpcRequestObserver.CallFinished += UpdateLoaderPanel;

			NotificationHelper.SetupNotificationChannels();

			var toolbar = FindViewById<Toolbar>(Resource.Id.toolbar);
			SetSupportActionBar(toolbar);

			SetupGitCommitInfo();

			var drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
			var toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open,
				Resource.String.navigation_drawer_close);
			drawer.AddDrawerListener(toggle);
			toggle.SyncState();

			var navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
			navigationView.SetNavigationItemSelectedListener(this);

			if (savedInstanceState == null)
			{
				using (var transaction = SupportFragmentManager.BeginTransaction())
				{
					transaction.Replace(Resource.Id.content_display_frame, new HostSelectionFragment());
					transaction.DisallowAddToBackStack();
					transaction.Commit();
				}
			}
			else
			{
				Log.Debug("Activity restarting - skipping replacing content fragment");
			}

			Analytics.TrackEvent("Application started");
		}

		private void SetupGitCommitInfo()
		{
			var navView = FindViewById<NavigationView>(Resource.Id.nav_view);
			if (navView == null)
				return;

			var textViewVersion = navView.GetHeaderView(0).FindViewById<TextView>(Resource.Id.drawer_header_text_line2);
			if (textViewVersion == null)
				return;
			
			textViewVersion.SetTextColor(Color.LightBlue);
			textViewVersion.Text = GetCommitId().Substring(0, 20) + " ...";
			textViewVersion.Clickable = true;
			textViewVersion.Click += CommitClicked;
		}

		private string GetCommitId()
		{
			using var stream = Resources.OpenRawResource(Resource.Raw.gitcommitid);
			using var streamReader = new StreamReader(stream, Encoding.UTF8);

			return streamReader.ReadToEnd();
		}

		private async void CommitClicked(object sender, EventArgs e)
		{
			if (await DialogHelper.Prompt(this, "Question",
				"Navigate to repository state of this application?", "Yes", "No") == true)
			{
				var id = GetCommitId();
				var uri = $"https://github.com/taori/PCRemote2/tree/{id}";
				var browserIntent = new Intent(Intent.ActionView, Uri.Parse(uri));
				StartActivity(browserIntent);
			}
		}

		private void UpdateLoaderPanel(object sender, int e)
		{
			if (_loaderPanel != null)
			{
				_loaderPanel.OverlayVisible = e > 0;
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
			// MenuInflater.Inflate(Resource.Menu.menu_main, menu);
			return false;
		}

		public bool OnNavigationItemSelected(IMenuItem item)
		{
			int id = item.ItemId;
			
			if (id == Resource.Id.nav_issues)
			{
				DialogHelper.Prompt(this, "Question",
					"Go to GitHub issues?", "Yes", "No").ContinueWith(prev =>
				{
					if (prev.Result == true)
					{
						var uri = "https://github.com/taori/PCRemote2/issues";
						var browserIntent = new Intent(Intent.ActionView, Uri.Parse(uri));
						StartActivity(browserIntent);
					}
				});
			}
			else if (id == Resource.Id.nav_logs)
			{
				using var transaction = SupportFragmentManager.BeginTransaction();
				var fragment = SupportFragmentManager.FindFragmentByTag(nameof(LogFragment));
				if (fragment == null)
				{
					transaction.SetStatusBarTitle("Logs");
					transaction.SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentFade);
					transaction.AddToBackStack(null);
					transaction.Replace(Resource.Id.app_bar_main_container, new LogFragment(), nameof(LogFragment));
					transaction.Commit();
				}
				else
				{
					transaction.SetTransition(AndroidX.Fragment.App.FragmentTransaction.TransitFragmentFade);
					transaction.Replace(Resource.Id.app_bar_main_container, fragment, nameof(LogFragment));
					transaction.Commit();
				}
			}
			else if (id == Resource.Id.nav_settings)
			{
				var fragmentExists = SupportFragmentManager.FindFragmentByTag(nameof(SettingsFragment)) != null;
				if (!fragmentExists)
				{
					using var transaction = SupportFragmentManager.BeginTransaction();
					transaction.SetStatusBarTitle("Settings");
					transaction.ReplaceContentAnimated(new SettingsFragment(), nameof(SettingsFragment));
					transaction.Commit();
				}

			}
			else if (id == Resource.Id.nav_update)
			{
				var fragmentExists = SupportFragmentManager.FindFragmentByTag(nameof(UpdateFragment)) != null;
				if (!fragmentExists)
				{
					using var transaction = SupportFragmentManager.BeginTransaction();
					transaction.SetStatusBarTitle("Update");
					transaction.ReplaceContentAnimated(new UpdateFragment(), nameof(UpdateFragment));
					transaction.Commit();
				}
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