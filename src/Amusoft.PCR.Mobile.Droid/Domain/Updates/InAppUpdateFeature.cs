using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Amusoft.PCR.Mobile.Droid.Toolkit;
using Android.Content;
using Android.Net;
using Android.Widget;
using AndroidX.Core.Content;
using Newtonsoft.Json;
using Xamarin.Essentials;
using FileProvider = AndroidX.Core.Content.FileProvider;

namespace Amusoft.PCR.Mobile.Droid.Domain.Updates
{
	public class InAppUpdateFeatureContext
	{
		public bool UserInitiated { get; set; }

		public DownloadProgressHandler DownloadProgressHandler { get; set; }
	}

	public class InAppUpdateFeature
	{
		private readonly Context _context;
		private readonly InAppUpdateFeatureContext _featureContext;

		public InAppUpdateFeature(Context context, InAppUpdateFeatureContext featureContext)
		{
			_context = context;
			_featureContext = featureContext;
		}

		public async Task ExecuteAsync()
		{
			var latestAssetUrl = await UpdateHelper.GetLatestApkAssetUrlAsync();
			var decision = await RequestUpdateDecisionAsync(latestAssetUrl);
			if (!decision)
			{
				if (_featureContext.UserInitiated)
					ToastHelper.Display(_context, "Update aborted", ToastLength.Long);

				return;
			}

			var downloadFilePath = Path.Combine(FileSystem.CacheDirectory, "latest.apk");
			var downloadSuccess = await UpdateHelper.DownloadApkAsync(latestAssetUrl, downloadFilePath, _featureContext.DownloadProgressHandler);
			if (downloadSuccess)
			{
				var providerPath = FileProvider.GetUriForFile(_context, "amusoft.pcr.mobile.droid.provider", new Java.IO.File(downloadFilePath));
				var upgradeIntent = new Intent(Intent.ActionView);
				upgradeIntent.SetDataAndType(providerPath, "application/vnd.android.package-archive");
				upgradeIntent.AddFlags(ActivityFlags.GrantReadUriPermission);
				_context.StartActivity(upgradeIntent);
			}
		}

		private async Task<bool> RequestUpdateDecisionAsync(string assetUrl)
		{
			if (_featureContext.UserInitiated)
				return true;

			var historicDecision = await GetHistoricDecisionAsync(assetUrl);
			if (historicDecision.HasValue)
				return false;

			var userDecision = await AskUserForUpdateConsentAsync();
			if (userDecision.HasValue)
			{
				await SaveHistoryDecisionAsync(assetUrl, userDecision.Value);
				return userDecision.Value;
			}
			else
			{
				return false;
			}
		}

		private async Task SaveHistoryDecisionAsync(string assetUrl, bool userDecision)
		{
			var history = await GetCurrentHistory();
			history.Add(assetUrl, userDecision);
			await File.WriteAllTextAsync(GetHistoryFilePath(), JsonConvert.SerializeObject(history));
		}

		private async Task<Dictionary<string, bool>> GetCurrentHistory()
		{
			var historyPath = GetHistoryFilePath();
			if (!File.Exists(historyPath))
			{
				var history = new Dictionary<string, bool>();
				return history;
			}
			else
			{
				return JsonConvert.DeserializeObject<Dictionary<string, bool>>(await File.ReadAllTextAsync(historyPath));
			}
		}

		private async Task<bool?> AskUserForUpdateConsentAsync()
		{
			return await DialogHelper.Prompt(_context, "Question", "An update is available. Do you want to update?", "Yes", "No");
		}

		private async Task<bool?> GetHistoricDecisionAsync(string assetUrl)
		{
			var historyPath = GetHistoryFilePath();
			if (!File.Exists(historyPath))
				return null;

			var deserialized = JsonConvert.DeserializeObject<Dictionary<string, bool>>(await File.ReadAllTextAsync(historyPath));
			if (deserialized.TryGetValue(assetUrl, out var historicDecision))
				return historicDecision;

			return null;
		}

		private static string GetHistoryFilePath()
		{
			return Path.Combine(Xamarin.Essentials.FileSystem.AppDataDirectory, "update-history.json");
		}
	}
}