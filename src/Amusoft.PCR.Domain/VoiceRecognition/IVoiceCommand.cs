using System.Collections.Generic;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public interface IVoiceCommand
	{
		public string Template { get; }

		public IEnumerable<string> GetTemplateParameters();
	}
}