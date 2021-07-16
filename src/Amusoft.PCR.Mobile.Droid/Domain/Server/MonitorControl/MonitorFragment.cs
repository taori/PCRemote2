using System;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
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

		private void MonitorOnOnClick(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}

		private void MonitorOffOnClick(object sender, EventArgs e)
		{
			throw new NotImplementedException();
		}
	}
}