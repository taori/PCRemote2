namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioMasterOffCommand : VoiceCommandBase
	{
		public override string Template => "{Trigger} {AudioTrigger} {Off} {Master}";
	}
}