using System.Collections.Generic;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public interface IVoiceCommandExpander
	{
		public IEnumerable<ExpandedVoiceCommand> Expand(IVoiceCommand voiceCommand);
	}
}