using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	[DebuggerDisplay("{Resolve()}")]
	public class ExpandedVoiceCommand
	{
		public ExpandedVoiceCommand(Type commandType, string template, KeyValuePair<string, string>[] parameters)
		{
			CommandType = commandType;
			Template = template;
			Parameters = parameters;
		}

		public bool TryGetParameterValue(string key, out string value)
		{
			var parameter = Parameters.FirstOrDefault(d => d.Key == key);
			value = parameter.Value;
			return value != null;
		}

		public Type CommandType { get; set; }

		public string Template { get; set; }

		public KeyValuePair<string, string>[] Parameters { get; set; }

		public string Resolve()
		{
			var sb = new StringBuilder(Template);
			foreach (var pair in Parameters)
			{
				sb.Replace(pair.Key, pair.Value.ToLowerInvariant());
			}

			return sb.ToString();
		}
	}
}