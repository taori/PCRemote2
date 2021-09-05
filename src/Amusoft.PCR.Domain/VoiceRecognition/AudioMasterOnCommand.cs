namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class AudioMasterOnCommand : VoiceCommandBase
	{
		public override string Template => "{Trigger} {AudioTrigger} {On} {Master}";
	}
}