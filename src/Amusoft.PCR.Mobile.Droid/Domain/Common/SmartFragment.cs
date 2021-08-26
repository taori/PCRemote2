using System;
using System.Collections.Generic;
using Android.OS;
using Android.Runtime;
using AndroidX.Fragment.App;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class SmartFragment : Fragment, IComponentContainer
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SmartFragment));

		protected SmartFragment(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
		{
		}



		public SmartFragment()
		{
			Components.Add(new AgentPersistanceComponent());
		}

		public override void OnCreate(Bundle savedInstanceState)
		{
			base.OnCreate(savedInstanceState);

			ComponentRunner.Execute<IFragmentCreationComponent>(this, d => d.OnCreate(this, savedInstanceState));
		}

		public override void OnSaveInstanceState(Bundle outState)
		{
			base.OnSaveInstanceState(outState);

			ComponentRunner.Execute<IFragmentPersistanceComponent>(this, d => d.OnSaveInstanceState(this, outState));
		}

		protected bool HasBeenResumedBefore { get; private set; }

		public override void OnResume()
		{
			base.OnResume();
			HasBeenResumedBefore = true;
		}

		public IList<IComponent> Components { get; } = new List<IComponent>();
	}
}