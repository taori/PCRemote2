using Amusoft.PCR.Mobile.Droid.Services;
using Fragment = AndroidX.Fragment.App.Fragment;
using FragmentTransaction = AndroidX.Fragment.App.FragmentTransaction;

namespace Amusoft.PCR.Mobile.Droid.Extensions
{
	public static class FragmentTransactionExtensions
	{
		public static FragmentTransaction SetStatusBarTitle(this FragmentTransaction source, string title)
		{
			ActivityLifecycleCallbacks.Instance.TopMostActivity.SetStatusBarTitle(title);

			return source;
		}

		public static FragmentTransaction ReplaceContentAnimated(this FragmentTransaction source, Fragment fragment, string tag = null)
		{
			source.AddToBackStack(null);
			source.SetCustomAnimations(Resource.Animation.enter_from_right, Resource.Animation.exit_to_left, Resource.Animation.enter_from_left, Resource.Animation.exit_to_right);
			if (tag == null)
			{
				source.Replace(Resource.Id.content_display_frame, fragment);
			}
			else
			{
				source.Replace(Resource.Id.content_display_frame, fragment, tag);
			}

			return source;
		}
	}
}