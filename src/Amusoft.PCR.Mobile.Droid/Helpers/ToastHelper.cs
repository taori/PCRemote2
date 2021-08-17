using Android.App;
using Android.Content;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Helpers
{
	public static class ToastHelper
	{
		public static void Display(string message, ToastLength duration)
		{
			Toast.MakeText(Application.Context, message, duration)?.Show();
		}

		public static void DisplaySuccess(bool success, ToastLength toastLength)
		{
			Display(success ? "OK" : "Error", toastLength);
		}
	}
}