using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class VoiceCommandRegister
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(VoiceCommandRegister));

		public readonly List<IVoiceCommand> Commands = new ();
		public readonly List<IVoiceCommandExpander> Expanders = new ();

		public void UseCommandProviders(params IVoiceCommandProvider[] items)
		{
			Log.Debug("Adding {Count} command providers", items.Length);
			Commands.AddRange(items.SelectMany(d => d.GetCommands()));
			Log.Debug("{Count} Commands registered", Commands.Count);
		}

		public void UseCommandExpanders(params IVoiceCommandExpander[] items)
		{
			Log.Debug("Adding {Count} command expanders", items.Length);
			Expanders.AddRange(items);
		}

		public void ClearExpanders()
		{
			Log.Debug("Clearing expanders");
			Expanders.Clear();
		}

		public IEnumerable<ExpandedVoiceCommand> ReadAll()
		{
			foreach (var command in Commands)
			{
				foreach (var expander in Expanders)
				{
					foreach (var expandedVoiceCommand in expander.Expand(command))
					{
						yield return expandedVoiceCommand;
					}
				}
			}
		}
	}
}