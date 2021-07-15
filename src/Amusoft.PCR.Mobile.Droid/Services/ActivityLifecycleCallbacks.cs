using System;
using Android.App;
using Android.OS;
using AndroidX.Core.App;
using Java.Interop;
using NLog;
using Object = Java.Lang.Object;

namespace Amusoft.PCR.Mobile.Droid.Services
{
	public class ActivityLifecycleCallbacks : Object, Application.IActivityLifecycleCallbacks
	{
		private ActivityLifecycleCallbacks()
		{
		}

		private static readonly Logger Log = LogManager.GetLogger(nameof(ActivityLifecycleCallbacks));

		public static readonly ActivityLifecycleCallbacks Instance = new ActivityLifecycleCallbacks();
		
		public Activity TopMostActivity { get; private set; }

		public void OnActivityCreated(Activity activity, Bundle? savedInstanceState)
		{
			TopMostActivity = activity;
			Log.Debug("{Name} called", nameof(OnActivityCreated));
		}

		public void OnActivityDestroyed(Activity activity)
		{
			Log.Debug("{Name} called", nameof(OnActivityDestroyed));
		}

		public void OnActivityPaused(Activity activity)
		{
			Log.Debug("{Name} called", nameof(OnActivityPaused));
		}

		public void OnActivityResumed(Activity activity)
		{
			TopMostActivity = activity;
			Log.Debug("{Name} called", nameof(OnActivityResumed));
		}

		public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
		{
			Log.Debug("{Name} called", nameof(OnActivityResumed));
		}

		public void OnActivityStarted(Activity activity)
		{
			TopMostActivity = activity;
			Log.Debug("{Name} called", nameof(OnActivityStarted));
		}

		public void OnActivityStopped(Activity activity)
		{
			Log.Debug("{Name} called", nameof(OnActivityStopped));
		}
	}
}