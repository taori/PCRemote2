using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.EntityFrameworkCore;

namespace Amusoft.PCR.Model.Entities
{
	public enum KeyValueKind
	{
		VoiceRecognitionEnabled,
		VoiceRecognitionTriggerWord,
		VoiceRecognitionTriggerWordAudio,
		VoiceRecognitionTriggerWordOnAliases,
		VoiceRecognitionTriggerWordOffAliases,
		VoiceRecognitionConfirmMessage,
		VoiceRecognitionErrorMessage,
	}
	
	[DebuggerDisplay("{Key} => {Value}")]
	public class KeyValueSetting
	{
		[Key]
		public KeyValueKind Key { get; set; }

		public string Value { get; set; }
	}
}