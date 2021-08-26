using Android.OS;
using AndroidX.Fragment.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public interface IFragmentCreationComponent : IComponent
	{
		public void OnCreate(Fragment fragment, Bundle savedState);
	}
}