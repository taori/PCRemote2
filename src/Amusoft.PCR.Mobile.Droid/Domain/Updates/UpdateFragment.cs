using System;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using Uri = Android.Net.Uri;

namespace Amusoft.PCR.Mobile.Droid.Domain.Updates
{
	public class UpdateFragment : SmartFragment
	{
		private Button _buttonUpdate;
		private ProgressBar _progress;
		private Button _buttonGotoReleases;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.update_main, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			_buttonUpdate = view.FindViewById<Button>(Resource.Id.button_start_update);
			_buttonGotoReleases = view.FindViewById<Button>(Resource.Id.button_goto_releases);
			_progress = view.FindViewById<ProgressBar>(Resource.Id.progressBar1);
			_progress.Visibility = ViewStates.Gone;
			_progress.Max = 100;
			_progress.Min = 0;

			_buttonUpdate.Click += OnUpdateClicked;
			_buttonGotoReleases.Click += OnGotoReleasesClicked;
			base.OnViewCreated(view, savedInstanceState);
		}

		private void OnGotoReleasesClicked(object sender, EventArgs e)
		{
			var uri = "https://github.com/taori/PCRemote2/releases/latest";
			var browserIntent = new Intent(Intent.ActionView, Uri.Parse(uri));
			StartActivity(browserIntent);
		}

		private async void OnUpdateClicked(object sender, EventArgs e)
		{
			var featureContext = new InAppUpdateFeatureContext();
			featureContext.UserInitiated = true;
			featureContext.DownloadProgressHandler = DownloadProgressHandler;
			var feature = new InAppUpdateFeature(Context, featureContext);
			_buttonUpdate.Clickable = false;
			_progress.Progress = 0;
			_progress.Visibility = ViewStates.Visible;

			await feature.ExecuteAsync();

			_buttonUpdate.Clickable = true;
			_progress.Visibility = ViewStates.Gone;
		}

		private void DownloadProgressHandler(long? totalfilesize, long totalbytesdownloaded, double? progresspercentage)
		{
			Xamarin.Essentials.MainThread.BeginInvokeOnMainThread(() =>
			{
				_progress.Progress = (int)(progresspercentage ?? 0);
			});
		}
	}
}