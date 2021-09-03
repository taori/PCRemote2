using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Speech.Recognition;
using System.Speech.Synthesis;
using Amusoft.PCR.Domain.VoiceRecognition;
using Amusoft.PCR.Grpc.Common;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop.Feature.VoiceCommands
{
	internal class SpeechManager
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SpeechManager));

		public static readonly SpeechManager Instance = new SpeechManager();

		private readonly SpeechRecognitionEngine _speechRecognition;
		private readonly SpeechSynthesizer _synthesizer;
		private VoiceCommandTable _commandTable;
		private string _synthesizerErrorMessage;
		private string _synthesizerConfirmMessage;

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
		}

		private void SpeechRecognitionOnSpeechRecognitionRejected(object sender, SpeechRecognitionRejectedEventArgs e)
		{
			Log.Debug("Speech rejected: {Text}", e.Result.Text);
		}

		private void SpeechRecognitionOnSpeechHypothesized(object sender, SpeechHypothesizedEventArgs e)
		{
			Log.Debug("Speech hypothesized: {Text}", e.Result.Text);
		}

		private void SpeechRecognitionOnSpeechRecognized(object sender, SpeechRecognizedEventArgs e)
		{
			if (VoiceCommandRunner.TryExecute(e.Result.Text, _commandTable))
			{
				_synthesizer.SpeakAsync(_synthesizerConfirmMessage);
			}
			else
			{
				_synthesizer.SpeakAsync(_synthesizerErrorMessage);
			}
		}

		public void UpdateGrammar(UpdateVoiceRecognitionRequest voiceRecognitionRequest)
		{
			var commandTable = RequestToCommandTable(voiceRecognitionRequest);
			if (EqualityComparer<VoiceCommandTable>.Default.Equals(commandTable, _commandTable))
			{
				Log.Debug("Request generated same commandTable - Grammar update skipped");
				return;
			}

			Log.Debug("Stopping current recognition");
			_speechRecognition.RecognizeAsyncStop();

			_synthesizerErrorMessage = voiceRecognitionRequest.SynthesizerErrorMessage ?? "Error";
			_synthesizerConfirmMessage = voiceRecognitionRequest.SynthesizerConfirmMessage ?? "OK";

			_commandTable = commandTable;

			Log.Debug("Unloading current grammar");
			_speechRecognition.UnloadAllGrammars();
			Log.Debug("Loading grammar from command table");
			_speechRecognition.LoadGrammar(BuildGrammar(commandTable));
			Log.Debug("Starting recognition");
			_speechRecognition.RecognizeAsync(RecognizeMode.Multiple);
		}

		private static VoiceCommandTable RequestToCommandTable(UpdateVoiceRecognitionRequest voiceRecognitionRequest)
		{
			var commandTable = new VoiceCommandTable();
			commandTable.Populate(voiceRecognitionRequest);
			return commandTable;
		}

		private Grammar BuildGrammar(VoiceCommandTable commandTable)
		{
			_commandTable = commandTable;
			return CommandTableToGrammar(commandTable);
		}

		private static Grammar CommandTableToGrammar(VoiceCommandTable commandTable)
		{
			var rootBuilder = new GrammarBuilder(commandTable.TriggerPhrase);

			var audioBuilder = new GrammarBuilder(commandTable.AudioPhrase);
			var choices = new Choices();
			foreach (var command in commandTable.PhraseCommands)
			{
				var phraseJoin = string.Join(" ", command.Phrases);
				Log.Debug("Adding command {PhraseJoin}", phraseJoin);
				choices.Add(new GrammarBuilder(phraseJoin));
			}

			audioBuilder.Append(choices);
			rootBuilder.Append(audioBuilder);

			// var onOffChoices = new Choices();
			// onOffChoices.Add(new SemanticResultValue("an", 1));
			// onOffChoices.Add(new SemanticResultValue("on", 1));
			// onOffChoices.Add(new SemanticResultValue("aus", 0));
			// onOffChoices.Add(new SemanticResultValue("off", 0));
			// grammar.Append(onOffChoices);
			// grammar.Append(new Choices("Mozilla Firefox", "Explorer", "steam"));

			return new Grammar(rootBuilder);
		}
	}
}