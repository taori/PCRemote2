namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioApplicationOffCommand : VoiceCommandBase
	{
		public override string Template => "{Trigger} {AudioTrigger} {Off} {Application}";
	}
}