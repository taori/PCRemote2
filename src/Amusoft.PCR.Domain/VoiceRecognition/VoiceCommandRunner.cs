using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NLog;

namespace Amusoft.PCR.Domain.VoiceRecognition
{
	public class VoiceCommandRunner
	{
		private readonly HashSet<IVoiceCommandBinder> _commandBinders = new ();

		private static readonly Logger Log = LogManager.GetLogger(nameof(VoiceCommandRunner));

		public void UseCommandBinders(params IVoiceCommandBinder[] commandBinders)
		{
			foreach (var binder in commandBinders)
			{
				_commandBinders.Add(binder);
			}
		}

		public bool TryExecute(string command, VoiceCommandRegister voiceCommandRegister)
		{
			Log.Debug("Attempting to execute command {Command}", command);

			Log.Debug("Reading command register");
			var allCommands = voiceCommandRegister.ReadAll().ToArray();
			Log.Debug("Retrieved {Count} results", allCommands.Length);

			var matchingCommands = allCommands.Where(d => d.Resolve().Equals(command, StringComparison.OrdinalIgnoreCase)).ToArray();
			if (matchingCommands.Length <= 0)
			{
				Log.Warn("No matching command found");
				return false;
			}
			if (matchingCommands.Length > 1)
			{
				Log.Error("More than one command could be applied");
				return false;
			}
			
			var binders = _commandBinders;
			Log.Debug("There are {Count} command binders present", _commandBinders.Count);

			var binderResults = binders.Select(binder => (bindSuccess : binder.CanHandle(matchingCommands[0]), binder) ).ToArray();
			var binderBySuccess = binderResults.ToLookup(d => d.bindSuccess);
			var bindSuccessCount = binderBySuccess[true].Count();
			if (bindSuccessCount <= 0)
			{
				Log.Warn("No valid command binder found");
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
				successfulBinder.ExecuteAsync(matchingCommands[0]);
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, "Binder execution fail");
				return false;
			}
		}
	}
}