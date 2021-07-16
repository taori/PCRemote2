using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using AndroidX.AppCompat.App;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace Amusoft.PCR.Mobile.Droid.Extensions
{
	public static class ActivityExtensions
	{
		public static void SetStatusBarTitle(this Activity source, string title)
		{
			if (source is AppCompatActivity appCompatActivity)
			{
				var currentTitle = appCompatActivity.SupportActionBar.Title;

				BackStackHandler.Add(() => appCompatActivity.SupportActionBar.Title = currentTitle);

				appCompatActivity.SupportActionBar.Title = title;
			}
		}
	}

	public static class FragmentTransactionExtensions
	{
		public static FragmentTransaction SetStatusBarTitle(this FragmentTransaction source, string title)
		{
			ActivityLifecycleCallbacks.Instance.TopMostActivity.SetStatusBarTitle(title);

			return source;
		}

		public static FragmentTransaction ReplaceContentAnimated(this FragmentTransaction source, Fragment fragment)
		{
			source.AddToBackStack(null);
			source.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
			source.Replace(Resource.Id.content_display_frame, fragment);

			return source;
		}
	}
}