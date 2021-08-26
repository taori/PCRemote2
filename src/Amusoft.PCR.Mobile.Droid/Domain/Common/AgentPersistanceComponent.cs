using Android.OS;
using AndroidX.Fragment.App;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Common
{
	public class AgentPersistanceComponent : IComponent, IFragmentCreationComponent, IFragmentPersistanceComponent
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AgentPersistanceComponent));

		public void OnCreate(Fragment fragment, Bundle savedState)
		{
			var agentFragment = fragment as IAgentFragment;
			if (savedState == null || agentFragment == null) 
				return;

			Log.Debug("Restoring state from previous state");

			agentFragment.HostName = savedState.GetString(nameof(IAgentFragment) + nameof(agentFragment.HostName));
			agentFragment.IpAddress = savedState.GetString(nameof(IAgentFragment) + nameof(agentFragment.IpAddress));
			agentFragment.Port = savedState.GetInt(nameof(IAgentFragment) + nameof(agentFragment.Port));
		}

		public void OnSaveInstanceState(Fragment fragment, Bundle outState)
		{
			var agentFragment = fragment as IAgentFragment;
			if (agentFragment == null)
				return;

			Log.Debug("Saving state for implementation of {Type}", typeof(IAgentFragment));

			outState.PutString(nameof(IAgentFragment) + nameof(agentFragment.HostName), agentFragment.HostName);
			outState.PutString(nameof(IAgentFragment) + nameof(agentFragment.IpAddress), agentFragment.IpAddress);
			outState.PutInt(nameof(IAgentFragment) + nameof(agentFragment.Port), agentFragment.Port);
		}
	}
}