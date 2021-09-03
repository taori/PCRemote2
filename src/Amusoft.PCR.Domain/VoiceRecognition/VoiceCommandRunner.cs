using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using NLog;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	internal class AudioApplicationToggleCommandBinder : ICommandBinder
	{
		public bool TryBind(string command, ILookup<string, string> aliasLookup)
		{
			return false;
		}

		public Task ExecuteAsync()
		{
			return Task.CompletedTask;
		}
	}

	public interface IVoiceCommand
	{
		public string Template { get; }

		public IEnumerable<string> GetTemplateParameters();
	}

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

	public class AudioApplicationToggleCommand : VoiceCommandBase
	{
		public override string Template => "{Trigger} {AudioTrigger} {Switch} {Application}";
	}

	public class AudioGlobalToggleCommand : VoiceCommandBase
	{
		public override string Template => "{Trigger} {AudioTrigger} {Switch} {Application}";
	}

	public class AudioCommandProvider : IVoiceCommandProvider
	{
		public IEnumerable<IVoiceCommand> GetCommands()
		{
			yield return new AudioApplicationToggleCommand();
			yield return new AudioGlobalToggleCommand();
		}
	}

	public interface IVoiceCommandProvider
	{
		IEnumerable<IVoiceCommand> GetCommands();
	}


	public abstract class LookupExpanderBase : IVoiceCommandExpander
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AudioLookupExpander));

		protected readonly ILookup<string, string> Lookup;

		protected LookupExpanderBase(ILookup<string, string> lookup)
		{
			Lookup = lookup;
		}

		public IEnumerable<string> Expand(IVoiceCommand voiceCommand)
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

		protected virtual IEnumerable<string> OnExpand(IVoiceCommand voiceCommand, string[] parameters)
		{
			yield break;
		}
	}

	public class AudioLookupExpander : LookupExpanderBase
	{
		public AudioLookupExpander(ILookup<string, string> lookup) : base(lookup)
		{
		}
	}
	
	public interface IVoiceCommandExpander
	{
		public IEnumerable<string> Expand(IVoiceCommand voiceCommand);
	}

	public class VoiceCommandRegister
	{
		public readonly List<IVoiceCommand> Commands = new ();
		public readonly List<IVoiceCommandExpander> Expanders = new ();

		public void UseCommandProviders(params IVoiceCommandProvider[] items)
		{
			Commands.AddRange(items.SelectMany(d => d.GetCommands()));
		}

		public void UseCommandExpanders(params IVoiceCommandExpander[] items)
		{
			Expanders.AddRange(items);
		}
	}

	public static class VoiceCommandRunner
	{
		private static readonly HashSet<ICommandBinder> CommandBinders = new HashSet<ICommandBinder>();

		private static readonly Logger Log = LogManager.GetLogger(nameof(VoiceCommandRunner));

		static VoiceCommandRunner()
		{
			CommandBinders.Add(new AudioApplicationToggleCommandBinder());
		}

		public static bool TryExecute(string command, VoiceCommandRegister voiceCommandRegister)
		{
			Log.Debug("Executing command {Command}", command);

			Log.Debug("Loading command binders");
			var binders = GetCommandBinders();
			Log.Debug("Retrieving binder results");

			var binderResults = binders.Select(binder => (bindSuccess : binder.TryBind(command, null), binder) ).ToArray();
			var binderBySuccess = binderResults.ToLookup(d => d.bindSuccess);
			var bindSuccessCount = binderBySuccess[true].Count();
			if (bindSuccessCount <= 0)
			{
				Log.Warn("No valid binder found");
				return false;
			}
			if (bindSuccessCount > 1)
			{
				Log.Error("More than one binder could be applied");
				return false;
			}

			var successfulBinder = binderBySuccess[true].First().binder;
			try
			{
				successfulBinder.ExecuteAsync();
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, "Binder execution fail");
				return false;
			}
		}

		private static IEnumerable<ICommandBinder> GetCommandBinders()
		{
			yield return new AudioApplicationToggleCommandBinder();
		}
	}
}