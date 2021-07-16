using System;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.Core.App;
using AndroidX.Transitions;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl
{
	public class SystemStateFragment : SmartFragment
	{
		private readonly GrpcApplicationAgent _agent;
		private Button _lockWorkstation;
		private Button _restart;
		private ProgressBar _progressBar;
		private Button _abort;
		private Button _shutdown;

		public SystemStateFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
			// processes
			// launch program
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
			
			_shutdown.Click += ShutdownOnClick;
			_abort.Click += AbortOnClick;
			_restart.Click += RestartOnClick;
			_lockWorkstation.Click += LockWorkstationOnClick;
		}

		private void ProgressBarOnClick(object sender, EventArgs e)
		{
			// throw new NotImplementedException();
		}

		private async void LockWorkstationOnClick(object sender, EventArgs e)
		{
			await _agent.DesktopClient.LockWorkStationAsync(TimeSpan.FromSeconds(5));
		}

		private async void RestartOnClick(object sender, EventArgs e)
		{
			await _agent.DesktopClient.RestartAsync(TimeSpan.FromSeconds(5), false, TimeSpan.FromSeconds(60));
		}

		private async void AbortOnClick(object sender, EventArgs e)
		{
			await _agent.DesktopClient.AbortShutDownAsync(TimeSpan.FromSeconds(5));
		}

		private async void ShutdownOnClick(object sender, EventArgs e)
		{
			await _agent.DesktopClient.ShutDownDelayedAsync(TimeSpan.FromSeconds(5), false, TimeSpan.FromSeconds(60));
		}
	}
}