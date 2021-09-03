using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Server.Dependencies;
using Amusoft.PCR.Server.Domain.Common;
using GrpcDotNetNamedPipes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.IPC
{
	public class VoiceRecognitionUpdateService : BackgroundService
	{
		private readonly ILogger<VoiceRecognitionUpdateService> _log;
		private readonly IServiceScopeFactory _serviceScopeFactory;
		private readonly NamedPipeChannel _namedPipeChannel;
		private readonly ApplicationStateTransmitter _applicationStateTransmitter;

		public VoiceRecognitionUpdateService(ILogger<VoiceRecognitionUpdateService> log, 
			IServiceScopeFactory serviceScopeFactory, 
			NamedPipeChannel namedPipeChannel, 
			ApplicationStateTransmitter applicationStateTransmitter)
		{
			_log = log;
			_serviceScopeFactory = serviceScopeFactory;
			_namedPipeChannel = namedPipeChannel;
			_applicationStateTransmitter = applicationStateTransmitter;
		}

		protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
			var voiceCommandServiceClient = new VoiceCommandService.VoiceCommandServiceClient(_namedPipeChannel);
			var desktopIntegrationServiceClient = new DesktopIntegrationService.DesktopIntegrationServiceClient(_namedPipeChannel);

			await _applicationStateTransmitter.ConfigurationDone;

			while (!stoppingToken.IsCancellationRequested)
			{
				try
				{
					_log.LogTrace("Attempting to update voice recognition");
					using var scope = _serviceScopeFactory.CreateScope();
					using var dbContext = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
					var keyValueSettingsManager = scope.ServiceProvider.GetRequiredService<KeyValueSettingsManager>();

					var feeds = await desktopIntegrationServiceClient.GetAudioFeedsAsync(new AudioFeedRequest());

					await UpdateAudioFeedTableAsync(dbContext, feeds);

					var recognitionEnabled = await keyValueSettingsManager.GetByKindAsBoolAsync(stoppingToken, KeyValueKind.VoiceRecognitionEnabled, false);
					if (recognitionEnabled)
						await voiceCommandServiceClient.UpdateVoiceRecognitionAsync(await BuildUpdateRequest(stoppingToken, dbContext, feeds));

					if(!recognitionEnabled)
						_log.LogTrace("Recognition is disabled");

					await Task.Delay(TimeSpan.FromSeconds(60), stoppingToken);
				}
				catch (Exception e)
				{
					_log.LogError(e, "ExecuteAsync");
				}
			}
		}

		private async Task<bool> UpdateAudioFeedTableAsync(ApplicationDbContext dbContext, AudioFeedResponse feeds)
		{
			if (!feeds.Success)
				return false;

			var feedNames = feeds.Items.Select(d => d.Name?.Trim()).Where(d => !string.IsNullOrEmpty(d));
			var dbFeeds = await dbContext.AudioFeeds.ToListAsync();
			var existingNames = new HashSet<string>(dbFeeds.Select(d => d.Name));
			bool anyChange = false;
			foreach (var feedName in feedNames)
			{
				if (!existingNames.Contains(feedName))
				{
					anyChange = true;
					_log.LogDebug("Adding {Name} to provide alias functionality", feedName);
					dbContext.AudioFeeds.Add(new AudioFeed() { Name = feedName });
				}
			}

			return !anyChange || await dbContext.SaveChangesAsync() > 0;
		}

		private async Task<UpdateVoiceRecognitionRequest> BuildUpdateRequest(CancellationToken cancellationToken,
			ApplicationDbContext dbContext, AudioFeedResponse audioFeedResponse)
		{
			var request = new UpdateVoiceRecognitionRequest();
			var settings = (await dbContext.KeyValueSettings.ToListAsync(cancellationToken)).ToDictionary(d => d.Key, d => d.Value);

			request.AudioPhrase =
				settings.TryGetValue(KeyValueKind.VoiceRecognitionTriggerWordAudio, out var audioPhrase)
					? audioPhrase
					: "Audio";

			request.TriggerPhrase =
				settings.TryGetValue(KeyValueKind.VoiceRecognitionTriggerWord, out var triggerWord)
					? triggerWord
					: "Computer";

			var offAliasList =
				settings.TryGetValue(KeyValueKind.VoiceRecognitionTriggerWordOffAliases, out var offAliases)
					? offAliases
					: "off";

			var onAliasList =
				settings.TryGetValue(KeyValueKind.VoiceRecognitionTriggerWordOnAliases, out var onAliases)
					? onAliases
					: "on";

			var confirmMessageText =
				settings.TryGetValue(KeyValueKind.VoiceRecognitionConfirmMessage, out var confirmMessage)
					? confirmMessage
					: "OK";

			var errorMessageText =
				settings.TryGetValue(KeyValueKind.VoiceRecognitionErrorMessage, out var errorMessage)
					? errorMessage
					: "Error";

			var feedAliases = await dbContext.AudioFeedAliases
				.Select(d => new ValueTuple<string, string>(d.Feed.Name, d.Alias)).ToListAsync(cancellationToken);

			var feedAliasDictionary = feedAliases.ToLookup(d => d.Item1, d => d.Item2);

			request.SynthesizerConfirmMessage = confirmMessageText;
			request.SynthesizerErrorMessage = errorMessageText;
			request.OffAliases.AddRange(offAliasList.Split('|'));
			request.OnAliases.AddRange(onAliasList.Split('|'));
			request.Items.AddRange(BuildPhraseList(audioFeedResponse, feedAliasDictionary));

			return request;
		}

		private IEnumerable<UpdateVoiceRecognitionRequestItem> BuildPhraseList(AudioFeedResponse feeds, ILookup<string, string> feedAliasLookup)
		{
			foreach (var item in feeds.Items)
			{
				if (string.IsNullOrEmpty(item.Name?.Trim()))
					continue;

				_log.LogDebug("Adding phrases for {Feed}", item.Name);
				_log.LogTrace("Adding {Feed} -> {Alias}", item.Name, item.Name);

				yield return new UpdateVoiceRecognitionRequestItem() {FeedName = item.Name, Alias = item.Name};

				if (feedAliasLookup.Contains(item.Name))
				{
					_log.LogDebug("Found aliases for {Feed}", item.Name);

					foreach (var alias in feedAliasLookup[item.Name])
					{
						_log.LogTrace("Adding {Feed} -> {Alias}", item.Name, alias);
						yield return new UpdateVoiceRecognitionRequestItem() {FeedName = item.Name, Alias = alias};
					}
				}
			}
		}
	}
}