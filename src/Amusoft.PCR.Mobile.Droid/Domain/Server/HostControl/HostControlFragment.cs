using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl;
using Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl;
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
	public class HostControlFragment : ButtonListFragment
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(HostControlFragment));

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

			if (!HasBeenResumedBefore)
				Activity.SetStatusBarTitle($"{ipAddress} - {machineName}");

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
			// processes
			// launch program
			Toast.MakeText(Context, nameof(ProgramsClicked), ToastLength.Short).Show();
		}

		private void InputControlClicked()
		{
			// send input
			Toast.MakeText(Context, nameof(InputControlClicked), ToastLength.Short).Show(); ;
		}

		private void SystemStateClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("System state");
				transaction.ReplaceContentAnimated(new SystemStateFragment(_agent));
				transaction.Commit();
			}
		}

		private void MonitorClicked()
		{
			Toast.MakeText(Context, nameof(MonitorClicked), ToastLength.Short).Show(); 
			// monitor on
			// monitor off
		}

		private void AudioClicked()
		{
			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Audio");
				transaction.ReplaceContentAnimated(new AudioFragment(_agent));
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