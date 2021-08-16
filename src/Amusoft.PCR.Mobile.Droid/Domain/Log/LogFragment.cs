using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Android.Text.Method;
using NLog;
using Environment = System.Environment;
using Fragment = AndroidX.Fragment.App.Fragment;

namespace Amusoft.PCR.Mobile.Droid.Domain.Log
{
	public class LogFragment : Fragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(LogFragment));

		private Button _refreshButton;
		private TextView _textView;
		private ProgressBar _progressBar;

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			_refreshButton.Click -= RefreshButtonOnClick;
			_refreshButton.Dispose();
			_textView.Dispose();
			_progressBar.Dispose();
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			_refreshButton = view.FindViewById<Button>(Resource.Id.button_refresh);
			_textView = view.FindViewById<TextView>(Resource.Id.content_text_view);
			_progressBar = view.FindViewById<ProgressBar>(Resource.Id.progress_bar);

			_refreshButton.Click += RefreshButtonOnClick;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.log_main, container, false);
		}

		private async void RefreshButtonOnClick(object sender, EventArgs e)
		{
			await LoadLogIntoViewAsync();
		}

		public override async void OnStart()
		{
			base.OnStart();
			await LoadLogIntoViewAsync();
		}

		private async Task LoadLogIntoViewAsync()
		{
			
			var root = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
			var path = Path.Combine(root, "logs", "nlog.csv");
			if (!File.Exists(path))
			{
				// content_text_view
				_textView.Text = "No log file present";
			}
			else
			{
				_progressBar.Visibility = ViewStates.Visible;
				await Task.Delay(1000);
				var logContent = await File.ReadAllTextAsync(path, Encoding.UTF8);
				// Log.Debug(logContent);
				_textView.Text = logContent;
				_progressBar.Visibility = ViewStates.Gone;
			}

		}
	}
}