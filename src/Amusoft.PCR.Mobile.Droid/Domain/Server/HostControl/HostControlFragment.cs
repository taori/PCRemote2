using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl;
using Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl;
using Amusoft.PCR.Mobile.Droid.Domain.Server.MonitorControl;
using Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl;
using Amusoft.PCR.Mobile.Droid.Domain.Server.SystemStateControl;
using Amusoft.PCR.Mobile.Droid.Extensions;
using Amusoft.PCR.Mobile.Droid.Helpers;
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

			var ipAddress = GetConnectionAddress();
			var machineName = Arguments.GetString(ArgumentTargetMachineName);
			var ipPort = GetConnectionPort();

			if (!HasBeenResumedBefore)
				Activity.SetStatusBarTitle($"{ipAddress} - {machineName}");

			_agent?.Dispose();
			_agent = GrpcApplicationAgentFactory.Create(ipAddress, ipPort);
		}

		private int GetConnectionPort()
		{
			return Arguments.GetInt(ArgumentTargetPort);
		}

		private string GetConnectionAddress()
		{
			return Arguments.GetString(ArgumentTargetAddress);
		}


		private async Task<List<ButtonElement>> CreateButtons()
		{
			var buttons = new List<ButtonElement>();
			if (await SocketHelper.IsPortOpenAsync(GetConnectionAddress(), GetConnectionPort(), TimeSpan.FromSeconds(5)))
			{
				buttons.Add(CreateButton("Audio", true, AudioClicked));
				buttons.Add(CreateButton("Monitors", true, MonitorClicked));
				buttons.Add(CreateButton("System state", true, SystemStateClicked));
				buttons.Add(CreateButton("Input control", true, InputControlClicked));
				buttons.Add(CreateButton("Programs", true, ProgramsClicked));
			}
			else
			{
				var buttonText = $"Unable to connect to {GetConnectionAddress()}:{GetConnectionPort()}";
				buttons.Add(new ButtonElement()
				{
					ButtonText = buttonText,
					Clickable = true,
					ButtonAction = () => this.ParentFragmentManager.PopBackStack()
				});
			}

			return buttons;
		}

		private async void ProgramsClicked()
		{
			if (!await IsAuthorizedAsync())
				return;

			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Programs");
				transaction.ReplaceContentAnimated(new ProgramFragment(_agent));
				transaction.Commit();
			}
		}

		private async void InputControlClicked()
		{
			if (!await IsAuthorizedAsync())
				return;

			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Send input");
				transaction.ReplaceContentAnimated(new InputFragment(_agent));
				transaction.Commit();
			}
		}

		private async void SystemStateClicked()
		{
			if (!await IsAuthorizedAsync())
				return;

			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("System state");
				transaction.ReplaceContentAnimated(new SystemStateFragment(_agent));
				transaction.Commit();
			}
		}

		private async void MonitorClicked()
		{
			if (!await IsAuthorizedAsync())
				return;

			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Monitors");
				transaction.ReplaceContentAnimated(new MonitorFragment(_agent));
				transaction.Commit();
			}
		}

		private async void AudioClicked()
		{
			if (!await IsAuthorizedAsync())
				return;

			using (var transaction = ParentFragmentManager.BeginTransaction())
			{
				transaction.SetStatusBarTitle("Audio");
				transaction.ReplaceContentAnimated(new AudioFragment(_agent));
				transaction.Commit();
			}
		}

		private async Task<bool> IsAuthorizedAsync()
		{
			var result = await _agent.DesktopClient.AuthenticateAsync();
			if (!result)
			{
				Toast.MakeText(Context, "Authentication required.", ToastLength.Long).Show();
			}

			return result;
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