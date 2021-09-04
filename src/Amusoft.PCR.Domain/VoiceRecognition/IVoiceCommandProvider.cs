using System.Collections.Generic;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public interface IVoiceCommandProvider
	{
		IEnumerable<IVoiceCommand> GetCommands();
	}
}