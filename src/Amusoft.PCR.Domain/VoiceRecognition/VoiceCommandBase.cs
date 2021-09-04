using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public abstract class VoiceCommandBase : IVoiceCommand
	{
		private static readonly Regex ParameterExpression = new Regex("{[^}]+}", RegexOptions.Compiled);

		public abstract string Template { get; }

		public IEnumerable<string> GetTemplateParameters()
		{
			if(Template == null)
				yield break;

			var matches = ParameterExpression.Matches(Template);
			if (matches.Count > 0)
			{
				foreach (Match match in matches)
				{
					yield return match.Value;
				}
			}
		}
	}
}