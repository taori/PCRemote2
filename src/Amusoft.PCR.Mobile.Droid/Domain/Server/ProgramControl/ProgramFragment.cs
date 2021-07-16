using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Communication;
using Android.OS;
using Android.Views;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.ProgramControl
{
	public class ProgramFragment : SmartFragment
	{
		private readonly GrpcApplicationAgent _agent;

		public ProgramFragment(GrpcApplicationAgent agent)
		{
			_agent = agent;
		}

		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_program_main, container, false);
		}
	}
}