using System;
using System.Reactive;
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
using AndroidX.Lifecycle;
using AndroidX.Transitions;
using AndroidX.Work;
using Grpc.Net.Client;
using Java.Util;
using NLog;
using Xamarin.Essentials;
using IObserver = AndroidX.Lifecycle.IObserver;
using Logger = NLog.Logger;
using Object = Java.Lang.Object;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class SystemStateFragment : SmartFragment, IObserver
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

			var workName = await SystemStateWorkRequestFactory.EnqueueAsync<RestartWorker>(Context, _agent.Address, SystemStateKind.Restart, DateTime.Now.Add(executionDelay.Value));
			var liveData = WorkManager.GetInstance(Context).GetWorkInfosForUniqueWorkLiveData(workName);
			liveData.RemoveObserver(this);
			liveData.Observe(ViewLifecycleOwner, this);
		}

		private async void HibernateOnClick(object sender, EventArgs e)
		{
			var executionDelay = await GetExecutionDelayAsync();
			if (executionDelay == null)
			{
				Log.Debug("User canceled");
				return;
			}

			
			var workName = await SystemStateWorkRequestFactory.EnqueueAsync<HibernateWorker>(Context, _agent.Address, SystemStateKind.Hibernate, DateTime.Now.Add(executionDelay.Value));
			var liveData = WorkManager.GetInstance(Context).GetWorkInfosForUniqueWorkLiveData(workName);
			liveData.RemoveObserver(this);
			liveData.Observe(ViewLifecycleOwner, this);
		}

		private async void ShutdownOnClick(object sender, EventArgs e)
		{
			var executionDelay = await GetExecutionDelayAsync();
			if (executionDelay == null)
			{
				Log.Debug("User canceled");
				return;
			}

			var workName = await SystemStateWorkRequestFactory.EnqueueAsync<ShutdownWorker>(Context, _agent.Address, SystemStateKind.Shutdown, DateTime.Now.Add(executionDelay.Value));
			var liveData = WorkManager.GetInstance(Context).GetWorkInfosForUniqueWorkLiveData(workName);
			liveData.RemoveObserver(this);
			liveData.Observe(ViewLifecycleOwner, this);
		}

		private Task<TimeSpan?> GetExecutionDelayAsync()
		{
			var tcs = new TaskCompletionSource<TimeSpan?>();
			var timePicker = new TimePickerDialog(Activity, Resource.Style.TimeSpinnerDialogTheme, (o, args) =>
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
		
		public void OnChanged(Object p0)
		{
			if (p0 is JavaList arrayList)
			{
				if (arrayList.Size() > 0 && arrayList.Get(0) is WorkInfo workInfo)
				{
					var progress = workInfo.Progress.GetInt(DelayedSystemStateWorker.ProgressPercentTag, 0);
					var visible = workInfo.Progress.GetBoolean(DelayedSystemStateWorker.ProgressVisibleTag, false);
					_progressBar.Progress = progress;
					_progressBar.Visibility = visible ? ViewStates.Visible : ViewStates.Gone;

					var workState = workInfo.GetState();
					if (workState == WorkInfo.State.Cancelled || workState == WorkInfo.State.Failed)
						_progressBar.Visibility = ViewStates.Gone;
				}
			}
		}
	}
}