using System;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using Amusoft.PCR.Mobile.Droid.Toolkit;
using Amusoft.Toolkit.Networking;
using AndroidX.Core.Content;
using Newtonsoft.Json;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Updates
{
	public static class HttpClients
	{
		public static readonly HttpClient General = new HttpClient();
	}

	public static class UpdateHelper
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(UpdateHelper));

		private const string Owner = "taori";
		private const string Repository = "PCRemote2";

		public static async Task<string> GetLatestApkAssetUrlAsync()
		{
			//https://docs.github.com/en/rest/reference/repos#list-release-assets
			var requestUri = await GetLatestAssetsUrlAsync();
			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
			httpRequestMessage.Headers.Accept.TryParseAdd("application/vnd.github.v3+json");
			using var response = await HttpClients.General.SendAsync(httpRequestMessage);
			var responseContent = await response.Content.ReadAsStringAsync();
			var definition = new []{ new { url = "", content_type = "", name = ""} };
			var deserialized = JsonConvert.DeserializeAnonymousType(responseContent, definition);
			if (deserialized == null)
				throw new Exception("Invalid deserialization definition");

			return deserialized.FirstOrDefault(d => d.content_type.Equals("application/vnd.android.package-archive", StringComparison.OrdinalIgnoreCase))?.url;
		}

		private static async Task<string> GetLatestAssetsUrlAsync()
		{
			// https://docs.github.com/en/rest/reference/repos#get-the-latest-release
			var requestUri = $"https://api.github.com/repos/{Owner}/{Repository}/releases/latest";
			using var httpRequestMessage = new HttpRequestMessage(HttpMethod.Get, requestUri);
			httpRequestMessage.Headers.Accept.TryParseAdd("application/vnd.github.v3+json");
			using var response = await HttpClients.General.SendAsync(httpRequestMessage);
			var responseContent = await response.Content.ReadAsStringAsync();
			var definition = new {assets_url = ""};
			var deserialized = JsonConvert.DeserializeAnonymousType(responseContent, definition);
			if (deserialized == null)
				throw new Exception("Invalid deserialization definition");

			return deserialized.assets_url;
		}

		public static async Task<bool> DownloadApkAsync(string assetUrl, string downloadFilePath, DownloadProgressHandler progressHandler)
		{
			try
			{
				// https://docs.github.com/en/rest/reference/repos#get-a-release-asset
				await DownloadWithProgress.ExecuteAsync(HttpClients.General, assetUrl, downloadFilePath, progressHandler, () =>
				{
					var requestMessage = new HttpRequestMessage(HttpMethod.Get, assetUrl);
					requestMessage.Headers.Accept.TryParseAdd("application/octet-stream");
					return requestMessage;
				});

				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, "An error occured while downloading the file");
				return false;
			}
		}
	}
}