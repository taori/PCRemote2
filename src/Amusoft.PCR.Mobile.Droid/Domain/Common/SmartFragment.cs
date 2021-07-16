using AndroidX.Fragment.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class SmartFragment : Fragment
	{
		protected bool HasBeenResumedBefore { get; private set; }

		public override void OnResume()
		{
			base.OnResume();
			HasBeenResumedBefore = true;
		}
	}
}