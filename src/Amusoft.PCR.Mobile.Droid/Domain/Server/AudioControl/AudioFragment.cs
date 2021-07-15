using Android.OS;
using Android.Views;
using AndroidX.Fragment.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Server.AudioControl
{
	public class AudioFragment : Fragment
	{
		public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
		{
			return inflater.Inflate(Resource.Layout.server_control_audio, container, false);
		}
	}
}