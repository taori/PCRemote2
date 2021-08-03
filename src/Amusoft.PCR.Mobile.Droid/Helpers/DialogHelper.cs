using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.Content;
using AndroidX.AppCompat.App;
using Java.Lang;

namespace Amusoft.PCR.Mobile.Droid.Helpers
{
	public static class DialogHelper
	{
		public static Task<bool?> Prompt(Context context, string title, string message, string positiveText, string negativeText)
		{
			var tcs = new TaskCompletionSource<bool?>();
			var builder = new AlertDialog.Builder(context);
			builder.SetMessage(message);

			if (title != null)
				builder.SetTitle(title);
			builder.SetPositiveButton(positiveText, (sender, args) => tcs.TrySetResult(true));
			builder.SetNegativeButton(negativeText, (sender, args) => tcs.TrySetResult(false));
			builder.SetOnCancelListener(new DialogInterfaceOnCancelListener(() => tcs.TrySetResult(null)));
			builder.Show();

			return tcs.Task;
		}
	}
}