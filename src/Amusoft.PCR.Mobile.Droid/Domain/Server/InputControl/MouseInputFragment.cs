using System.Numerics;
using Amusoft.PCR.Mobile.Droid.CustomControls;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.OS;
using Android.Views;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class MouseInputFragment : SmartFragment
	{
		private readonly GrpcApplicationAgent _agent;
		private TrackingView _trackView;

		private static readonly Logger Log = LogManager.GetLogger(nameof(MouseInputFragment));

		public MouseInputFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_mouse_input, container, false);
		}

		public override void OnViewCreated(View view, Bundle savedInstanceState)
		{
			base.OnViewCreated(view, savedInstanceState);
			_trackView = view.FindViewById<TrackingView>(Resource.Id.trackingView1);
			_trackView.VelocityOccured += TrackViewOnVelocityOccured;

		}

		private void TrackViewOnVelocityOccured(object sender, Vector2 e)
		{
			Log.Debug("Gesture velocity: {X} {Y}", e.X, e.Y);
		}
	}
}