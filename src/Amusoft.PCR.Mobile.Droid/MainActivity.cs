using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Amusoft.PCR.Mobile.Droid.Domain.Networking;
using Android.App;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using AndroidX.Core.View;
using AndroidX.DrawerLayout.Widget;
using Google.Android.Material.FloatingActionButton;
using Google.Android.Material.Navigation;
using Google.Android.Material.Snackbar;
using NLog;
using NLog.Config;
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

            DrawerLayout drawer = FindViewById<DrawerLayout>(Resource.Id.drawer_layout);
            ActionBarDrawerToggle toggle = new ActionBarDrawerToggle(this, drawer, toolbar, Resource.String.navigation_drawer_open, Resource.String.navigation_drawer_close);
            drawer.AddDrawerListener(toggle);
            toggle.SyncState();

            NavigationView navigationView = FindViewById<NavigationView>(Resource.Id.nav_view);
            navigationView.SetNavigationItemSelectedListener(this);
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
            if(drawer.IsDrawerOpen(GravityCompat.Start))
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
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        private void FabOnClick(object sender, EventArgs eventArgs)
        {
	        var message = _lastMessage == default
		        ? "Replace with your own action"
		        : Encoding.UTF8.GetString(_lastMessage.Buffer);
            View view = (View) sender;

            Snackbar.Make(view, message, Snackbar.LengthLong)
                .SetAction("Action", (Android.Views.View.IOnClickListener)null).Show();
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
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}

