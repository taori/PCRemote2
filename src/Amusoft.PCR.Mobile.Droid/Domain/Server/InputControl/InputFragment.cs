using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.OS;
using Android.Views;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.InputControl
{
	public class InputFragment : SmartFragment
	{
		private readonly GrpcApplicationAgent _agent;

		public InputFragment(GrpcApplicationAgent agent)
		{
			// send input
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_input_main, container, false);
		}
	}
}