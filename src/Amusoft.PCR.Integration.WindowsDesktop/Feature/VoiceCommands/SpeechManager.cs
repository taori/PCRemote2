using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using System.Threading.Tasks;
using Amusoft.PCR.Domain.VoiceRecognition;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Integration.WindowsDesktop.Managers;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop.Feature.VoiceCommands
{
	internal class SpeechManager : IVoiceCommandBinder
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SpeechManager));

		public static readonly SpeechManager Instance = new SpeechManager();

		private readonly SpeechRecognitionEngine _speechRecognition;
		private readonly SpeechSynthesizer _synthesizer;
		private string _synthesizerErrorMessage;
		private string _synthesizerConfirmMessage;

		private readonly VoiceCommandRunner _voiceCommandRunner;
		private readonly VoiceCommandRegister _voiceCommandRegister;
		private readonly CompositeKeyValueSource _expanderAliasSource;

		private float _confidenceThreshold;
		private readonly List<(string key, string value)> _phraseValues = new();
		private Dictionary<string, string> _aliasToOriginalLookup;

		private SpeechManager()
		{
			Log.Debug("Initializing {Engine} with locale {Locale}", nameof(SpeechRecognitionEngine), CultureInfo.InstalledUICulture);
			_speechRecognition = new SpeechRecognitionEngine(CultureInfo.InstalledUICulture);
			_speechRecognition.SetInputToDefaultAudioDevice();
			_speechRecognition.SpeechRecognized += SpeechRecognitionOnSpeechRecognized;
			_speechRecognition.SpeechHypothesized += SpeechRecognitionOnSpeechHypothesized;
			_speechRecognition.SpeechRecognitionRejected += SpeechRecognitionOnSpeechRecognitionRejected;

			_synthesizer = new SpeechSynthesizer();
			_synthesizer.Volume = 40;
			_synthesizer.SetOutputToDefaultAudioDevice();

			_expanderAliasSource = new CompositeKeyValueSource();

			_voiceCommandRunner = new VoiceCommandRunner();
			_voiceCommandRunner.UseCommandBinders(this);

			_voiceCommandRegister = new VoiceCommandRegister();
			_voiceCommandRegister.UseCommandProviders(new AudioCommandProvider());
		}

		private IEnumerable<(string key, string value)> GetPhraseValues(UpdateVoiceRecognitionRequest request)
		{
			yield return ("{Master}", request.MasterPhrase);

			foreach (var offAlias in request.OffAliases)
			{
				yield return ("{Off}", offAlias);
			}

			foreach (var onAlias in request.OnAliases)
			{
				yield return ("{On}", onAlias);
			}

			yield return ("{Trigger}", request.TriggerPhrase);
			yield return ("{AudioTrigger}", request.AudioPhrase);

			foreach (var item in request.AudioRecognitionItems)
			{
				yield return ("{Application}", item.Alias);
			}
		}

		public void SetupRegister(IEnumerable<(string key, string value)> values)
		{
			_expanderAliasSource.Clear();
			_expanderAliasSource.AddRange(values);

			var lookup = _expanderAliasSource.Compose();

			_voiceCommandRegister.ClearExpanders();
			_voiceCommandRegister.UseCommandExpanders(
				new AudioApplicationOffExpander(lookup), 
				new AudioApplicationOnExpander(lookup), 
				new AudioMasterOffExpander(lookup), 
				new AudioMasterOnExpander(lookup));
		}

		private void SpeechRecognitionOnSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
		{	  
			Log.Debug("Speech rejected: {Text} - confidence: {Value}", e.Result.Text, e.Result.Confidence);
			if (e.Result.Confidence > _confidenceThreshold)
			{
				Log.Debug("Command {Text} was rejected but is above threshold {Threshold} - executing command anyway", e.Result.Text, _confidenceThreshold);
				TryExecuteVoiceCommand(e.Result.Text);
			}
		}

		private void SpeechRecognitionOnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
		{
			Log.Debug("Speech hypothesized: {Text}", e.Result.Text);
		}

		private void SpeechRecognitionOnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			TryExecuteVoiceCommand(e.Result.Text);
		}

		private bool TryExecuteVoiceCommand(string text)
		{
			if (_voiceCommandRunner.TryExecute(text, _voiceCommandRegister))
			{
				_synthesizer.SpeakAsync(_synthesizerConfirmMessage);
				return true;
			}
			else
			{
				_synthesizer.SpeakAsync(_synthesizerErrorMessage);
				return false;
			}
		}

		public void UpdateGrammar(UpdateVoiceRecognitionRequest voiceRecognitionRequest)
		{
			var phraseValues = GetPhraseValues(voiceRecognitionRequest).ToArray();
			_confidenceThreshold = voiceRecognitionRequest.RecognitionThreshold / 100f;
			if (phraseValues.SequenceEqual(_phraseValues))
			{
				Log.Debug("Same phrases as last update - skipping update");
				return;
			}

			_aliasToOriginalLookup = GetApplicationOriginalNames(voiceRecognitionRequest);

			_phraseValues.Clear();
			_phraseValues.AddRange(phraseValues);

			Log.Debug("Stopping current recognition");
			_speechRecognition.RecognizeAsyncStop();

			_synthesizerErrorMessage = voiceRecognitionRequest.SynthesizerErrorMessage ?? "Error";
			_synthesizerConfirmMessage = voiceRecognitionRequest.SynthesizerConfirmMessage ?? "OK";
			
			Log.Debug("Unloading current grammar");
			_speechRecognition.UnloadAllGrammars();
			Log.Debug("Loading grammar from command table");
			_speechRecognition.LoadGrammar(BuildGrammar(_phraseValues));
			Log.Debug("Starting recognition");

			StartVoiceRecognition();
		}

		private Dictionary<string, string> GetApplicationOriginalNames(UpdateVoiceRecognitionRequest request)
		{
			var all = request.AudioRecognitionItems.Select(d => (d.FeedName.ToLowerInvariant(), d.Alias.ToLowerInvariant()));
			var unique = new HashSet<(string feedName, string alias)>(all);
			return unique.ToDictionary(d => d.alias, d => d.feedName);
		}

		private Grammar BuildGrammar(List<(string key, string value)> phraseValues)
		{
			return PhrasesPairsToGrammar(phraseValues);
		}

		private Grammar PhrasesPairsToGrammar(List<(string key, string value)> phraseValues)
		{
			SetupRegister(phraseValues);
			var commands = _voiceCommandRegister.ReadAll();

			var commandTexts = commands.Select(d => d.Resolve());
			var commandSplits = commandTexts.Select(d => d.Split(' ')).ToArray();
			var splitLengths = commandSplits.Select(d => d.Length).ToArray();

			var rootBuilder = new GrammarBuilder();
			AppendChoices(rootBuilder, commandSplits, splitLengths, 0);

			Log.Info("Generated grammar: {Grammar}", rootBuilder.DebugShowPhrases);

			return new Grammar(rootBuilder);
		}

		private void AppendChoices(GrammarBuilder rootBuilder, string[][] commandSplits, int[] splitLengths, int index)
		{
			var choices = new Choices();
			var options = GetSplitOptions(commandSplits, splitLengths, index);
			var uniqueOptions = new HashSet<string>(options);
			if (uniqueOptions.Count > 0)
			{
				choices.Add(uniqueOptions.ToArray());
				rootBuilder.Append(choices);
			}

			if(splitLengths.Any(d => index < d))
				AppendChoices(rootBuilder, commandSplits, splitLengths, ++index);
		}

		private IEnumerable<string> GetSplitOptions(string[][] commandSplits, int[] splitLengths, int index)
		{
			for (int i = 0; i < splitLengths.Length; i++)
			{
				if (index < splitLengths[i])
					yield return commandSplits[i][index];
			}
		}

		public bool CanHandle(ExpandedVoiceCommand command)
		{
			if (command.CommandType == typeof(AudioApplicationOnCommand) || command.CommandType == typeof(AudioMasterOnCommand))
			{
				return true;
			}
			if (command.CommandType == typeof(AudioApplicationOffCommand) || command.CommandType == typeof(AudioMasterOffCommand))
			{
				return true;
			}

			return false;
		}

		public Task ExecuteAsync(ExpandedVoiceCommand command)
		{
			HandleApplicationAudio(command);
			HandleMasterAudio(command);

			return Task.CompletedTask;
		}

		private void HandleMasterAudio(ExpandedVoiceCommand command)
		{
			var onCommand = command.CommandType == typeof(AudioMasterOnCommand);
			var offCommand = command.CommandType == typeof(AudioMasterOffCommand);

			if ((onCommand || offCommand))
			{
				var toValue = offCommand ? "muted" : "unmuted";
				Log.Info("Setting master to {Value}", toValue);
				SimpleAudioManager.SetMasterVolumeMute(offCommand);
			}
			else
			{
				Log.Debug("Unable to process command");
			}
		}

		private void HandleApplicationAudio(ExpandedVoiceCommand command)
		{
			var onCommand = command.CommandType == typeof(AudioApplicationOnCommand);
			var offCommand = command.CommandType == typeof(AudioApplicationOffCommand);

			if ((onCommand || offCommand) && command.TryGetParameterValue("{Application}", out var application))
			{
				if (SimpleAudioManager.TryGetAudioFeeds(out var feeds))
				{
					var feedsWithNames = feeds.Where(d => !string.IsNullOrEmpty(d.Name.Trim())).ToArray();
					Log.Debug("Found the following processes: {@List}", feedsWithNames.Select(d => d.Name));

					if (_aliasToOriginalLookup.TryGetValue(application.ToLowerInvariant(), out var originalName))
					{
						var matchingFeed =
							feedsWithNames.FirstOrDefault(d => d.Name.Equals(originalName, StringComparison.OrdinalIgnoreCase));
						if (matchingFeed != null)
						{
							matchingFeed.Muted = offCommand;
							Log.Debug("Found matching feed, updating state.");
							var success = SimpleAudioManager.TryUpdateFeed(matchingFeed);
							Log.Info("Feed update result: {Result} for application {Name}", success, originalName);
						}
					}
					else
					{
						Log.Debug("Failed to find {Name} in alias lookup", application.ToLowerInvariant());
					}
				}
			}
			else
			{
				Log.Debug("Unable to process command");
			}
		}

		private bool _recognitionRunning;
		public void StartVoiceRecognition()
		{
			try
			{
				if (_recognitionRunning)
				{
					Log.Info("Voice recognition already running");
					return;
				}

				Log.Info("Enabling voice recognition");
				_speechRecognition.RecognizeAsync(RecognizeMode.Multiple);
			}
			catch (Exception e)
			{
				_recognitionRunning = true;
				Log.Error(e);
				throw;
			}
		}

		public void StopVoiceRecognition()
		{
			try
			{
				Log.Info("Disabling voice recognition");
				_speechRecognition.RecognizeAsyncStop();
			}
			catch (Exception e)
			{
				_recognitionRunning = false;
				Log.Error(e);
				throw;
			}
		}
	}
}
