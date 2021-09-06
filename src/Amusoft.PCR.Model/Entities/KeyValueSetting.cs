using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Amusoft.PCR.Model.Entities
{
	public enum KeyValueKind
	{
		VoiceRecognitionEnabled = 0,
		VoiceRecognitionTriggerWord = 1,
		VoiceRecognitionTriggerWordAudio = 2,
		VoiceRecognitionTriggerWordOnAliases = 3,
		VoiceRecognitionTriggerWordOffAliases = 4,
		VoiceRecognitionConfirmMessage = 5,
		VoiceRecognitionErrorMessage = 6,
		VoiceRecognitionThreshold = 7,
		VoiceRecognitionTriggerWordAudioMaster = 8,
	}
	
	[DebuggerDisplay("{Key} => {Value}")]
	public class KeyValueSetting
	{
		[Key]
		public KeyValueKind Key { get; set; }

		public string Value { get; set; }
	}
}