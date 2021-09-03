using System;
using System.Collections.Generic;
using System.Linq;
using Amusoft.PCR.Grpc.Common;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class VoiceCommandTable : IEquatable<VoiceCommandTable>
	{
		public bool Equals(VoiceCommandTable other)
		{
			if (ReferenceEquals(null, other)) return false;
			if (ReferenceEquals(this, other)) return true;

			return this.TriggerPhrase == other.TriggerPhrase && this.AudioPhrase == other.AudioPhrase
			                                          && this.OnAliases.SequenceEqual(other.OnAliases)
			                                          && this.OffAliases.SequenceEqual(other.OffAliases)
			                                          && this.PhraseCommands.SequenceEqual(other.PhraseCommands);
		}

		public override bool Equals(object obj)
		{
			if (ReferenceEquals(null, obj)) return false;
			if (ReferenceEquals(this, obj)) return true;
			if (obj.GetType() != this.GetType()) return false;
			return Equals((VoiceCommandTable) obj);
		}

		public override int GetHashCode()
		{
			return HashCode.Combine(TriggerPhrase, AudioPhrase, OnAliases, OffAliases, PhraseCommands);
		}

		public string TriggerPhrase { get; set; }

		public string AudioPhrase { get; set; }

		public string[] OnAliases { get; set; }

		public string[] OffAliases { get; set; }

		public List<PhraseCommand> PhraseCommands { get; set; }

		public void Populate(UpdateVoiceRecognitionRequest request)
		{
			TriggerPhrase = request.TriggerPhrase;
			AudioPhrase = request.AudioPhrase;
			OnAliases = request.OnAliases.ToArray();
			OffAliases = request.OffAliases.ToArray();
			PhraseCommands = BuildPhraseCommands(request.Items);
		}

		private List<PhraseCommand> BuildPhraseCommands(IList<UpdateVoiceRecognitionRequestItem> requestItems)
		{
			var results = new List<PhraseCommand>();
			var audioTriggers = OffAliases.Concat(OnAliases).ToArray();
			foreach (var requestItem in requestItems)
			{
				foreach (var audioTrigger in audioTriggers)
				{
					results.Add(new PhraseCommand() { Kind = PhraseCommandKind.Audio, Phrases = new[] { audioTrigger, requestItem.Alias } });
				}
			}

			return results;
		}
	}
}