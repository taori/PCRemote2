using System.Linq;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioMasterOnExpander : LookupExpanderBase
	{
		public AudioMasterOnExpander(ILookup<string, string> lookup) : base(lookup)
		{
		}

		protected override bool SupportsExpansion(IVoiceCommand voiceCommand)
		{
			return voiceCommand is AudioMasterOnCommand;
		}
	}
}