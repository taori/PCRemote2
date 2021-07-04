﻿using System.Threading.Tasks;

namespace Amusoft.PCR.Server.Dependencies
{
	public class ApplicationStateTransmitter
	{
		private readonly TaskCompletionSource _configurationDone;

		public ApplicationStateTransmitter()
		{
			_configurationDone = new TaskCompletionSource(null);
		}

		public Task ConfigurationDone => _configurationDone.Task;

		public void NotifyConfigurationDone() => _configurationDone.SetResult();
	}
}