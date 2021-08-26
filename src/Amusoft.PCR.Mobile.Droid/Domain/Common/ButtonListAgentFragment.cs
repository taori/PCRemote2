using Android.OS;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public abstract class ButtonListAgentFragment : ButtonListFragment, IAgentFragment
	{
		public string IpAddress { get; set; }
		public int Port { get; set; }
		public string HostName { get; set; }

		protected ButtonListAgentFragment()
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
	}
}