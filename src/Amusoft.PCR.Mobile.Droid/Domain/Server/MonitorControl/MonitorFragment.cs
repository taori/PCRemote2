using System;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.OS;
using Android.Views;
using Android.Widget;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.MonitorControl
{
	public class MonitorFragment : SmartFragment
	{
		private readonly GrpcApplicationAgent _agent;
		private Button _monitorOn;
		private Button _monitorOff;

		public MonitorFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_monitor, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_monitorOn = view.FindViewById<Button>(Resource.Id.button_monitor_on);
			_monitorOff = view.FindViewById<Button>(Resource.Id.button_monitor_off);

			_monitorOff.Click += MonitorOffOnClick;
			_monitorOn.Click += MonitorOnOnClick;
		}

		private async void MonitorOnOnClick(object sender, EventArgs e)
		{
			var result = await _agent.DesktopClient.MonitorOnAsync(TimeSpan.FromSeconds(5));
			ToastHelper.DisplaySuccess(result, ToastLength.Short);
		}

		private async void MonitorOffOnClick(object sender, EventArgs e)
		{
			var result = await _agent.DesktopClient.MonitorOffAsync(TimeSpan.FromSeconds(10));
			ToastHelper.DisplaySuccess(result, ToastLength.Short);
		}
	}
}