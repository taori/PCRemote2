using System;
using System.Diagnostics;
using System.Linq;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	[DebuggerDisplay("{ToString()}")]
	public class PhraseCommand : IEquatable<PhraseCommand>
	{
		public bool Equals(PhraseCommand other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;
			return Kind == other.Kind && Phrases.SequenceEqual(other.Phrases);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((PhraseCommand) obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine((int) Kind, Phrases);
		}

		public PhraseCommandKind Kind { get; set; }

		public string[] Phrases { get; set; }
	}
}