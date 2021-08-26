using System;
using System.Numerics;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.CustomControls;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Fragment.App;
using Grpc.Core;
using NLog;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class MouseInputFragment : SmartAgentFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(MouseInputFragment));
		private AsyncClientStreamingCall<SendMouseMoveRequestItem, SendMouseMoveResponse> _mouseMoveStream;
		private SeekBar _seekbar;
		private TextView _seekbarLabel;
		private Button _buttonRMB;
		private Button _buttonLMB;
		private TrackingView _trackView;

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_mouse_input, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			_trackView = view.FindViewById<TrackingView>(Resource.Id.trackingView1);
			_trackView.VelocityOccured += TrackViewOnVelocityOccured;
			_trackView.SingleTapGesture += TrackViewOnSingleTapGesture;
			_trackView.MultiTouchGesture += TrackViewOnMultiTouchGesture;
			_seekbar = view.FindViewById<SeekBar>(Resource.Id.seekBar1);
			_seekbarLabel = view.FindViewById<TextView>(Resource.Id.seekbar1Label);
			
			_seekbar.Min = 0;
			_seekbar.Max = 100;
			_seekbar.ProgressChanged += SeekbarOnProgressChanged;
			SetDefaultSensitivityAsync();

			_buttonRMB = view.FindViewById<Button>(Resource.Id.buttonRMB);
			_buttonRMB.Click += ButtonRMBOnClick;

			_buttonLMB = view.FindViewById<Button>(Resource.Id.buttonLMB);
			_buttonLMB.Click += ButtonLMBOnClick;
		}

		private void TrackViewOnMultiTouchGesture(object sender, EventArgs e)
		{
			ButtonRMBOnClick(sender, EventArgs.Empty);
		}

		private void TrackViewOnSingleTapGesture(object sender, EventArgs e)
		{
			ButtonLMBOnClick(sender, EventArgs.Empty);
		}

		
		public override void OnResume()
		{
			Log.Info("Starting mouse move stream");
			_mouseMoveStream?.Dispose();
			_mouseMoveStream = this.GetAgent().FullDesktopClient.SendMouseMove();
			base.OnResume();
		}

		public override void OnStop()
		{
			Log.Info("Shutting down mouse stream");
			_mouseMoveStream.RequestStream.CompleteAsync();
			base.OnStop();
		}

		private async void ButtonLMBOnClick(object sender, EventArgs e)
		{
			await this.GetAgent().FullDesktopClient.SendLeftMouseButtonClickAsync(new DefaultRequest());
		}

		private async void ButtonRMBOnClick(object sender, EventArgs e)
		{
			await this.GetAgent().FullDesktopClient.SendRightMouseButtonClickAsync(new DefaultRequest());
		}

		private void SeekbarOnProgressChanged(object sender, SeekBar.ProgressChangedEventArgs e)
		{
			if (e.FromUser)
				SecureStorage.SetAsync(GetSensitivityStorageKey(), e.Progress.ToString());

			_seekbarLabel.Text = $"Sensitivity: {e.Progress}";
		}

		private string GetSensitivityStorageKey()
		{
			return $"{this.GetAgent().Address}:MouseMoveSensitivity";
		}

		private async void SetDefaultSensitivityAsync()
		{
			_seekbar.Progress = await GetDefaultSensitivityAsync();
		}

		private async Task<int> GetDefaultSensitivityAsync()
		{
			var value = await SecureStorage.GetAsync(GetSensitivityStorageKey());
			if (value == null)
			{
				return 20;
			}
			else
			{
				return int.TryParse(value, out var parsed) ? parsed : 20;
			}
		}

		private void TrackViewOnVelocityOccured(object sender, Vector2 e)
		{
			var newX = (e.X / 1000f) * _seekbar.Progress;
			var newY = (e.Y / 1000f) * _seekbar.Progress;
			_mouseMoveStream.RequestStream.WriteAsync(new SendMouseMoveRequestItem() {X = (int) newX, Y = (int) newY});
			Log.Trace("Gesture velocity: {X} {Y}", newX, newY);
		}
	}
}