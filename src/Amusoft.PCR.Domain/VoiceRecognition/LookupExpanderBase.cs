using System.Collections.Generic;
using System.Linq;
using NLog;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public abstract class LookupExpanderBase : IVoiceCommandExpander
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AudioApplicationOnExpander));

		protected readonly ILookup<string, string> Lookup;

		protected LookupExpanderBase(ILookup<string, string> lookup)
		{
			Lookup = lookup;
		}

		public IEnumerable<ExpandedVoiceCommand> Expand(IVoiceCommand voiceCommand)
		{
			var parameters = voiceCommand.GetTemplateParameters().ToArray();
			var error = false;

			foreach (var parameter in parameters)
			{
				if (!Lookup.Contains(parameter))
				{
					error = true;
					Log.Error("Parameter {Parameter} is missing in lookup table", parameter);
				}
			}

			if (error)
				yield break;

			foreach (var expanded in OnExpand(voiceCommand, parameters))
			{
				yield return expanded;
			}
		}

		protected virtual IEnumerable<ExpandedVoiceCommand> OnExpand(IVoiceCommand voiceCommand, string[] parameters)
		{
			if (!SupportsExpansion(voiceCommand))
			{
				Log.Trace("This expander does not handle commands of type {Type}", voiceCommand);
				yield break;
			}

			var pairsArray = GetParameterPairsMatrix(parameters, Lookup);
			foreach (var pairs in pairsArray)
			{
				yield return new ExpandedVoiceCommand(voiceCommand.GetType(), voiceCommand.Template, pairs);
			}
		}

		private IEnumerable<KeyValuePair<string, string>[]> GetParameterPairsMatrix(string[] parameters, ILookup<string, string> lookup)
		{
			var indexLimits = GetIndexLimits(parameters, lookup);
			var indexMatrix = GetIndexMatrix(indexLimits, parameters.Length);
			var valueMatrix = GetValueMatrix(parameters, lookup, indexMatrix, parameters.Length);
			
			foreach (var pairs in valueMatrix)
			{
				yield return pairs;
			}
		}

		private IEnumerable<KeyValuePair<string, string>[]> GetValueMatrix(string[] parameters, ILookup<string, string> lookup, IEnumerable<int[]> indexMatrix, int parameterCount)
		{
			foreach (var indexValues in indexMatrix)
			{
				var kvp = new KeyValuePair<string, string>[parameterCount];
				for (var parameterIndex = 0; parameterIndex < parameters.Length; parameterIndex++)
				{
					var parameter = parameters[parameterIndex];
					kvp[parameterIndex] = new KeyValuePair<string, string>(parameter, lookup[parameter].Skip(indexValues[parameterIndex]).Take(1).FirstOrDefault());
				}

				yield return kvp;
			}
		}

		private IEnumerable<int[]> GetIndexMatrix(int[] indexLimits, int allocationDepth)
		{
			var maxLinearIndex = GetMaxLinearIndex(indexLimits);
			var potencyArray = GetIndexPotency(indexLimits, allocationDepth);
			for (int i = 0; i < maxLinearIndex; i++)
			{
				var indexArray = GetIndexArray(i, potencyArray, allocationDepth);
				yield return indexArray;
			}
		}

		private int[] GetIndexPotency(int[] indexLimits, int allocationDepth)
		{
			var result = new int[allocationDepth];
			var currentPotency = 1;
			for (int i = allocationDepth - 1; i >= 0; i--)
			{
				result[i] = currentPotency;
				currentPotency = indexLimits[i] * currentPotency;
			}

			return result;
		}

		private int[] GetIndexArray(int elementIndex, int[] potencyArray, int allocationDepth)
		{
			var result = new int[allocationDepth];
			if (elementIndex == 0)
				return result;

			var remaining = elementIndex;
			for (int i = 0; i < potencyArray.Length; i++)
			{
				var factor = remaining / potencyArray[i];
				result[i] = factor;
				remaining = remaining - factor * potencyArray[i];
			}
			
			return result;
		}

		private int GetMaxLinearIndex(int[] indexLimits)
		{
			var product = indexLimits[0];
			for (int i = 1; i < indexLimits.Length; i++)
			{
				product *= indexLimits[i];
			}

			return product;
		}

		private int[] GetIndexLimits(string[] parameters, ILookup<string, string> lookup)
		{
			var result = new int[parameters.Length];
			for (var index = 0; index < parameters.Length; index++)
			{
				var parameter = parameters[index];
				result[index] = lookup[parameter].Count();
			}

			return result;
		}

		protected abstract bool SupportsExpansion(IVoiceCommand voiceCommand);
	}
}