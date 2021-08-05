using System;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.App;
using Android.Nfc;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.Transitions;
using Grpc.Net.Client;
using Java.Util;
using NLog;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class SystemStateFragment : SmartFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SystemStateFragment));

		private readonly GrpcApplicationAgent _agent;
		private Button _lockWorkstation;
		private Button _restart;
		private ProgressBar _progressBar;
		private Button _abort;
		private Button _shutdown;
		private Button _hibernate;

		public SystemStateFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_system_state, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_progressBar = view.FindViewById<ProgressBar>(Resource.Id.progressBar1);
			_lockWorkstation = view.FindViewById<Button>(Resource.Id.button_lock_workstation);
			_shutdown = view.FindViewById<Button>(Resource.Id.button_shutdown);
			_restart = view.FindViewById<Button>(Resource.Id.button_restart);
			_abort = view.FindViewById<Button>(Resource.Id.button_abort);
			_hibernate = view.FindViewById<Button>(Resource.Id.button_hibernate);

			_hibernate.Click += HibernateOnClick;
			_shutdown.Click += ShutdownOnClick;
			_abort.Click += AbortOnClick;
			_restart.Click += RestartOnClick;
			_lockWorkstation.Click += LockWorkstationOnClick;
			_progressBar.Visibility = ViewStates.Gone;
		}


		private void ProgressBarOnClick(object sender, EventArgs e)
		{
			// throw new NotImplementedException();
		}

		private async void LockWorkstationOnClick(object sender, EventArgs e)
		{
			var result = await _agent.DesktopClient.LockWorkStationAsync(TimeSpan.FromSeconds(5));
			ToastHelper.DisplaySuccess(Context, result, ToastLength.Short);
		}

		private async void AbortOnClick(object sender, EventArgs e)
		{
			SystemStateManager.AbortAllTimers(_agent.Address);

			var result = await _agent.DesktopClient.AbortShutDownAsync(TimeSpan.FromSeconds(5));
			ToastHelper.DisplaySuccess(Context, result, ToastLength.Short);
		}

		private async void RestartOnClick(object sender, EventArgs e)
		{
			var executionDelay = await GetExecutionDelayAsync();
			if (executionDelay == null)
			{
				Log.Debug("User canceled");
				return;
			}

			await SystemStateWorkRequestFactory.EnqueueAsync<RestartWorker>(Context, _agent.Address, SystemStateKind.Restart, DateTime.Now.Add(executionDelay.Value));
		}

		private async void HibernateOnClick(object sender, EventArgs e)
		{
			var executionDelay = await GetExecutionDelayAsync();
			if (executionDelay == null)
			{
				Log.Debug("User canceled");
				return;
			}

			await SystemStateWorkRequestFactory.EnqueueAsync<HibernateWorker>(Context, _agent.Address, SystemStateKind.Hibernate, DateTime.Now.Add(executionDelay.Value));
		}

		private async void ShutdownOnClick(object sender, EventArgs e)
		{
			var executionDelay = await GetExecutionDelayAsync();
			if (executionDelay == null)
			{
				Log.Debug("User canceled");
				return;
			}

			await SystemStateWorkRequestFactory.EnqueueAsync<ShutdownWorker>(Context, _agent.Address, SystemStateKind.Shutdown, DateTime.Now.Add(executionDelay.Value));
		}

		private Task<TimeSpan?> GetExecutionDelayAsync()
		{
			// Resource.Style.Theme_AppCompat_DayNight_NoActionBar
			var tcs = new TaskCompletionSource<TimeSpan?>();
			// var timePicker = new TimePickerDialog(Activity, Resource.Style.holo, async (o, args) =>
			var timePicker = new TimePickerDialog(Activity, Resource.Style.TimeSpinnerDialogTheme, async (o, args) =>
			{
				var delay = new TimeSpan(args.HourOfDay, args.Minute, 0);

				if (delay.Equals(TimeSpan.Zero))
					delay = TimeSpan.FromMinutes(1);

				tcs.TrySetResult(delay);

			}, 0, 1, true);

			timePicker.SetTitle("Delay");
			timePicker.SetOnCancelListener(new DialogInterfaceOnCancelListener(() => { tcs.TrySetResult(null); }));
			timePicker.Show();

			return tcs.Task;
		}
	}
}