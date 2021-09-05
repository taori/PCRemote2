using System.Linq;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioApplicationOffExpander : LookupExpanderBase
	{
		public AudioApplicationOffExpander(ILookup<string, string> lookup) : base(lookup)
		{
		}

		protected override bool SupportsExpansion(IVoiceCommand voiceCommand)
		{
			return voiceCommand is AudioApplicationOffCommand;
		}
	}
}