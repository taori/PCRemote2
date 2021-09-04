using System.Linq;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioMasterOffExpander : LookupExpanderBase
	{
		public AudioMasterOffExpander(ILookup<string, string> lookup) : base(lookup)
		{
		}

		protected override bool SupportsExpansion(IVoiceCommand voiceCommand)
		{
			return voiceCommand is AudioMasterOffCommand;
		}
	}
}