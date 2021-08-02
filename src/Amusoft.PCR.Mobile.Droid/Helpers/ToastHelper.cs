using Android.Content;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Helpers
{
	public static class ToastHelper
	{
		public static void Display(Context context, string message, ToastLength duration)
		{
			Toast.MakeText(context, message, duration).Show();
		}
	}
}