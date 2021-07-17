using System;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl
{
	public class ProgramFragment : SmartFragment
	{
		private readonly GrpcApplicationAgent _agent;
		private Button _startProgram;
		private Button _killProcess;

		public ProgramFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_program_main, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			_startProgram = view.FindViewById<Button>(Resource.Id.button_start_program);
			_killProcess = view.FindViewById<Button>(Resource.Id.button_kill_process);

			_startProgram.Click += StartProgramOnClick;
			_killProcess.Click += KillProcessOnClick;
		}

		private void StartProgramOnClick(object sender, EventArgs e)
		{
			using (var fragmentTransaction = ParentFragmentManager.BeginTransaction())
			{
				fragmentTransaction.ReplaceContentAnimated(new LaunchProgramFragment(_agent));
				fragmentTransaction.SetStatusBarTitle("Launch program");
				fragmentTransaction.Commit();
			}
		}

		private void KillProcessOnClick(object sender, EventArgs e)
		{
			using (var fragmentTransaction = ParentFragmentManager.BeginTransaction())
			{
				fragmentTransaction.ReplaceContentAnimated(new KillProcessFragment(_agent));
				fragmentTransaction.SetStatusBarTitle("Processes");
				fragmentTransaction.Commit();
			}
		}
	}
}