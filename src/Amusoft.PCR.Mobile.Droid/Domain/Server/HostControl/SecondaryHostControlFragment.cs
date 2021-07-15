using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Amusoft.PCR.Mobile.Droid.Services;
using Android.OS;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Grpc.Net.Client;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.HostControl
{
	public class SecondaryHostControlFragment : ButtonListFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SecondaryHostControlFragment));

		private GrpcApplicationAgent _agent;

		public const string ArgumentTargetMachineName = "MachineName";
		public const string ArgumentTargetAddress = "Address";
		public const string ArgumentTargetPort = "Port";

		protected override ButtonListDataSource CreateDataSource()
		{
			return new ButtonListDataSource(CreateButtons);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);

			var ipAddress = Arguments.GetString(ArgumentTargetAddress);
			var machineName = Arguments.GetString(ArgumentTargetMachineName);
			var ipPort = Arguments.GetInt(ArgumentTargetPort);
			
			if (Activity is AppCompatActivity appCompatActivity)
			{
				var currenTitle = appCompatActivity.SupportActionBar.Title;
				BackStackHandler.Add(() => { appCompatActivity.SupportActionBar.Title = currenTitle; });
				appCompatActivity.SupportActionBar.Title = $"{ipAddress} - {machineName}";
			}

			_agent?.Dispose();
			_agent = GrpcApplicationAgentFactory.Create(ipAddress, ipPort);
		}

		private Task<List<ButtonElement>> CreateButtons()
		{
			var buttons = new List<ButtonElement>();
			buttons.Add(CreateButton("Audio", true, AudioClicked));
			buttons.Add(CreateButton("Monitors", true, MonitorClicked));
			buttons.Add(CreateButton("System state", true, SystemStateClicked));
			buttons.Add(CreateButton("Input control", true, InputControlClicked));
			buttons.Add(CreateButton("Programs", true, ProgramsClicked));

			return Task.FromResult(buttons);
		}

		private void ProgramsClicked()
		{
			Toast.MakeText(Context, nameof(ProgramsClicked), ToastLength.Short).Show();
		}

		private void InputControlClicked()
		{
			Toast.MakeText(Context, nameof(InputControlClicked), ToastLength.Short).Show(); ;
		}

		private void SystemStateClicked()
		{
			Toast.MakeText(Context, nameof(SystemStateClicked), ToastLength.Short).Show(); ;
		}

		private void MonitorClicked()
		{
			Toast.MakeText(Context, nameof(MonitorClicked), ToastLength.Short).Show(); ;
		}

		private void AudioClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Audio");
				transaction.ReplaceContentAnimated(new AudioFragment());
				transaction.Commit();
			}
		}

		private ButtonElement CreateButton(string buttonText, bool clickable, Action action)
		{
			return new ButtonElement()
			{
				ButtonText = buttonText,
				Clickable = clickable,
				ButtonAction = action
			};
		}
	}
}