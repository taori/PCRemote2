using System.Collections.Generic;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioCommandProvider : IVoiceCommandProvider
	{
		public IEnumerable<IVoiceCommand> GetCommands()
		{
			yield return new AudioApplicationOnCommand();
			yield return new AudioApplicationOffCommand();
			yield return new AudioMasterOnCommand();
			yield return new AudioMasterOffCommand();
		}
	}
}