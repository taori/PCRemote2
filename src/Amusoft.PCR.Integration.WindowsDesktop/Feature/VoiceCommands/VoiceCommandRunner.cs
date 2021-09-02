using System;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop.Feature.VoiceCommands
{
	internal static class VoiceCommandRunner
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(VoiceCommandRunner));

		public static bool TryExecute(string command, VoiceCommandTable voiceCommandTable)
		{
			Log.Debug("Executing command {Command}", command);

			if (command.StartsWith("computer audio", StringComparison.OrdinalIgnoreCase))
			{
				if (command.StartsWith("computer audio on", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}

				if (command.StartsWith("computer audio off", StringComparison.OrdinalIgnoreCase))
				{
					return true;
				}
			}

			return false;
		}
	}
}