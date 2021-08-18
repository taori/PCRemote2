using System;
using Android.Runtime;
using AndroidX.Fragment.App;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class SmartFragment : Fragment
	{
		protected SmartFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}

		public SmartFragment()
		{
		}

		protected bool HasBeenResumedBefore { get; private set; }

		public override void OnResume()
		{
			base.OnResume();
			HasBeenResumedBefore = true;
		}
	}
}