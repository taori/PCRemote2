using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.Toolkit.UI;
using Android.OS;
using Android.Views;
using Android.Widget;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl
{
	public class AudioFragment : SmartFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AudioFragment));

		private readonly GrpcApplicationAgent _agent;
		private SeekBar _seekBar;
		private TextView _textView;
		private Button _button;

		public AudioFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_audio, container, false);
		}

		public override async void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_seekBar = view.FindViewById<SeekBar>(Resource.Id.seekBar1);
			_textView = view.FindViewById<TextView>(Resource.Id.textView1);
			_button = view.FindViewById<Button>(Resource.Id.button1);

			_button.Click += ToggleMuteClicked;
			_seekBar.ProgressChanged += SeekBarOnProgressChanged;
			_seekBar.SetProgress(await _agent.DesktopClient.GetMasterVolumeAsync(TimeSpan.FromSeconds(5), 45), false);
		}

		private async void ToggleMuteClicked(object sender, EventArgs e)
		{
			var result = await _agent.DesktopClient.ToggleMuteAsync(TimeSpan.FromSeconds(5));
			switch (result)
			{
				case null:
					ToastHelper.Display(Context, "Error", ToastLength.Short);
					break;
				case true:
					ToastHelper.Display(Context, "Muted", ToastLength.Short);
					break;
				case false:
					ToastHelper.Display(Context, "Unmuted", ToastLength.Short);
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

			Log.Debug("Setting master volume to {Value} FromUser: {FromUser}", progress, e.FromUser);
			if (e.FromUser)
			{
				Debouncer.Debounce(nameof(AudioFragment) + nameof(SeekBarOnProgressChanged), async () =>
				{
					await _agent.DesktopClient.SetMasterVolumeAsync(TimeSpan.FromSeconds(5), progress);
					ToastHelper.Display(Context, "Volume updated", ToastLength.Short);
				}, TimeSpan.FromSeconds(2));
			}
		}
	}
}