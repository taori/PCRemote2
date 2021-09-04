using System.Linq;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioApplicationOnExpander : LookupExpanderBase
	{
		public AudioApplicationOnExpander(ILookup<string, string> lookup) : base(lookup)
		{
		}

		protected override bool SupportsExpansion(IVoiceCommand voiceCommand)
		{
			return voiceCommand is AudioApplicationOnCommand;
		}
	}
}