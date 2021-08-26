using Android.OS;
using AndroidX.Fragment.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public interface IFragmentPersistanceComponent : IComponent
	{
		public void OnSaveInstanceState(Fragment agentFragment, Bundle outState);
	}
}