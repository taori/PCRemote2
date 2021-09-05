namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioApplicationOnCommand : VoiceCommandBase
	{
		public override string Template => "{Trigger} {AudioTrigger} {On} {Application}";
	}
}