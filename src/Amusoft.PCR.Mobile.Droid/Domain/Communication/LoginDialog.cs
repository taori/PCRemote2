using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.App;
using Android.Content;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.Widget;
using Java.Lang;
using AlertDialog = AndroidX.AppCompat.App.AlertDialog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public static class LoginDialog
	{
		public static Task<JwtLoginCredentials> GetInputAsync(string title)
		{
			var context = GetContext();
			var tcs = new TaskCompletionSource<JwtLoginCredentials>();
			var result = new JwtLoginCredentials();
			AlertDialog dialog = null;
			AlertDialog.Builder builder = new AlertDialog.Builder(context);
			builder.SetTitle(title);
			
			var viewInflated = LayoutInflater.From(context).Inflate(Resource.Layout.login_view, null, false);
			var userInput = viewInflated.FindViewById<AppCompatEditText>(Resource.Id.input_user);
			var passwordInput = viewInflated.FindViewById<AppCompatEditText>(Resource.Id.input_password);

			builder.SetView(viewInflated);

			builder.SetOnCancelListener(new DialogInterfaceOnCancelListener(() => tcs.TrySetResult(null)));
			builder.SetOnDismissListener(new DialogInterfaceOnDismissListener(() => tcs.TrySetResult(null)));
			builder.SetPositiveButton(Android.Resource.String.Ok, (sender, args) =>
			{
				result.User = userInput.Text;
				result.Password = passwordInput.Text;
				tcs.TrySetResult(result);
				dialog.Dismiss();
			});
			builder.SetNegativeButton(Android.Resource.String.Cancel, (sender, args) =>
			{
				tcs.TrySetResult(null);
				dialog.Cancel();
			});

			dialog = builder.Show();

			return tcs.Task;
		}

		private static Activity GetContext()
		{							 
			return ActivityLifecycleCallbacks.Instance.TopMostActivity;
		}
	}

	public class DialogInterfaceOnDismissListener : Java.Lang.Object, IDialogInterfaceOnDismissListener
	{
		private readonly Action _callback;

		public DialogInterfaceOnDismissListener(Action callback)
		{
			_callback = callback;
		}

		public void OnDismiss(IDialogInterface dialog)
		{
			_callback?.Invoke();
		}
	}

	public class DialogInterfaceOnCancelListener : Java.Lang.Object, IDialogInterfaceOnCancelListener
	{
		private readonly Action _callback;

		public DialogInterfaceOnCancelListener(Action callback)
		{
			_callback = callback;
		}

		public void OnCancel(IDialogInterface dialog)
		{
			_callback?.Invoke();
		}
	}
}