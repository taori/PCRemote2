using System;
using System.Linq;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.PCR.Mobile.Droid.Toolkit.UI;
using Amusoft.Toolkit.UI;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.RecyclerView.Widget;
using AndroidX.SwipeRefreshLayout.Widget;
using Google.Protobuf.Collections;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl
{
	public class AudioFragment : SmartAgentFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AudioFragment));

		private GrpcApplicationAgent _agent;
		private SeekBar _seekBar;
		private TextView _textView;
		private ImageView _imageViewMasterToggle;
		private RecyclerView _recyclerView;
		private SwipeRefreshLayout _swipeRefreshLayout;
		
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_audio, container, false);
		}

		public override async void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_seekBar = view.FindViewById<SeekBar>(Resource.Id.seekBar1);
			_textView = view.FindViewById<TextView>(Resource.Id.textView1);
			_imageViewMasterToggle = view.FindViewById<ImageView>(Resource.Id.button1);
			_recyclerView = view.FindViewById<RecyclerView>(Resource.Id.recyclerView1);
			_swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipeRefreshLayout1);

			_agent?.Dispose();
			_agent = this.GetAgent();

			_imageViewMasterToggle.Clickable = true;

			_swipeRefreshLayout.Refresh += SwipeRefreshLayoutOnRefresh;
			var dataSource = new AudioApplicationDataSource(_agent);
			dataSource.UpdateRequired += DataSourceOnUpdateRequired;
			_recyclerView.SetAdapter(dataSource);

			_imageViewMasterToggle.Click += ToggleMuteClicked;
			_seekBar.ProgressChanged += SeekBarOnProgressChanged;
			_seekBar.SetProgress(await _agent.DesktopClient.GetMasterVolumeAsync(TimeSpan.FromSeconds(5), 45), false);
		}

		private async void DataSourceOnUpdateRequired(object sender, AudioFeedResponseItem e)
		{
			try
			{
				var result = await _agent.FullDesktopClient.UpdateAudioFeedAsync(new UpdateAudioFeedRequest() {Item = e});
				ToastHelper.DisplaySuccess(result.Success, ToastLength.Short);

				if (result.Success)
				{
					var feeds = await _agent.FullDesktopClient.GetAudioFeedsAsync(new AudioFeedRequest());
					if (_recyclerView.GetAdapter() is AudioApplicationDataSource audioDataSource)
					{
						var match = feeds.Items.FirstOrDefault(d => d.Id == e.Id);
						if (match != null)
						{
							audioDataSource.UpdateSingle(match);
						}
					}
				}
			}
			catch (Exception exception)
			{
				Log.Error(exception);
				ToastHelper.Display("Update failed", ToastLength.Short);
			}
		}

		public override void OnResume()
		{
			SwipeRefreshLayoutOnRefresh(this, EventArgs.Empty);
			base.OnResume();
		}

		private async void SwipeRefreshLayoutOnRefresh(object sender, EventArgs e)
		{
			var adapter = _recyclerView.GetAdapter();
			if (adapter is GenericDataSource<AudioFeedResponseItem> dataSource)
			{
				_swipeRefreshLayout.Refreshing = true;
				await dataSource.ReloadAsync();
				_swipeRefreshLayout.Refreshing = false;
			}
		}

		private async void ToggleMuteClicked(object sender, EventArgs e)
		{
			var result = await _agent.DesktopClient.ToggleMuteAsync(TimeSpan.FromSeconds(5));
			switch (result)
			{
				case null:
					ToastHelper.Display("Error", ToastLength.Short);
					break;
				case true:
					ToastHelper.Display("Muted", ToastLength.Short);
					_imageViewMasterToggle.SetImageResource(Resource.Drawable.outline_volume_off_24);
					break;
				case false:
					ToastHelper.Display("Unmuted", ToastLength.Short);
					_imageViewMasterToggle.SetImageResource(Resource.Drawable.outline_volume_up_24);
					break;
			}
		}

		private void SeekBarOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
		{
			const int step = 5;
			var progress = e.Progress;
			progress = progress / step;
			progress = progress * step;
			_textView.Text = $"Master volume: {progress}";
			_seekBar.SetProgress(progress, true);

			Log.Info("Setting master volume to {Value} FromUser: {FromUser}", progress, e.FromUser);
			if (e.FromUser)
			{
				Debouncer.Debounce(nameof(AudioFragment) + nameof(SeekBarOnProgressChanged), async () =>
				{
					await _agent.DesktopClient.SetMasterVolumeAsync(TimeSpan.FromSeconds(5), progress);
					ToastHelper.Display("Volume updated", ToastLength.Short);
				}, TimeSpan.FromSeconds(2));
			}
		}
	}
}