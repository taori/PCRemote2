using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Android.App;
using AndroidX.AppCompat.App;

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
}