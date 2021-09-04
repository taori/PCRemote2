using System.Collections.Generic;
using System.Linq;

namespace Amusoft.PCR.Integration.WindowsDesktop.Feature.VoiceCommands
{
	public class CompositeKeyValueSource
	{
		private readonly List<(string key, string value)> _values = new();

		public void Clear() => _values.Clear();

		public void AddRange(IEnumerable<(string key, string value)> values) => _values.AddRange(values);

		public ILookup<string, string> Compose()
		{
			return _values.ToLookup(d => d.key, d => d.value);
		}
	}
}