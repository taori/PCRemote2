﻿using System;
using System.Buffers;
using System.IO;
using System.Net.Http;
using System.Threading.Tasks;

namespace Amusoft.Toolkit.Networking
{
	public delegate void DownloadProgressHandler(long? totalFileSize, long totalBytesDownloaded, double? progressPercentage);

	public static class DownloadWithProgress
	{
		public static async Task ExecuteAsync(HttpClient httpClient, string downloadPath, string destinationPath, DownloadProgressHandler progress, Func<HttpRequestMessage> requestMessageBuilder = null)
		{
			requestMessageBuilder ??= GetDefaultRequestBuilder(downloadPath);
			var download = new HttpClientDownloadWithProgress(httpClient, destinationPath, requestMessageBuilder);
			download.ProgressChanged += progress;
			await download.StartDownload();
			download.ProgressChanged -= progress;
		}

		private static Func<HttpRequestMessage> GetDefaultRequestBuilder(string downloadPath)
		{
			return () => new HttpRequestMessage(HttpMethod.Get, downloadPath);
		}
	}

	internal class HttpClientDownloadWithProgress
	{
		private readonly HttpClient _httpClient;
		private readonly string _destinationFilePath;
		private readonly Func<HttpRequestMessage> _requestMessageBuilder;
		private int _bufferSize = 8192;

		public event DownloadProgressHandler ProgressChanged;

		public HttpClientDownloadWithProgress(HttpClient httpClient, string destinationFilePath, Func<HttpRequestMessage> requestMessageBuilder)
		{
			_httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
			_destinationFilePath = destinationFilePath ?? throw new ArgumentNullException(nameof(destinationFilePath));
			_requestMessageBuilder = requestMessageBuilder ?? throw new ArgumentNullException(nameof(requestMessageBuilder));
		}

		public async Task StartDownload()
		{
			using var requestMessage = _requestMessageBuilder.Invoke();
			using var response = await _httpClient.SendAsync(requestMessage, HttpCompletionOption.ResponseHeadersRead);
			await DownloadAsync(response);
		}

		private async Task DownloadAsync(HttpResponseMessage response)
		{
			response.EnsureSuccessStatusCode();

			var totalBytes = response.Content.Headers.ContentLength;

			using (var contentStream = await response.Content.ReadAsStreamAsync())
				await ProcessContentStream(totalBytes, contentStream);
		}

		private async Task ProcessContentStream(long? totalDownloadSize, Stream contentStream)
		{
			var totalBytesRead = 0L;
			var readCount = 0L;
			var buffer = ArrayPool<byte>.Shared.Rent(_bufferSize);
			var isMoreToRead = true;

			using (var fileStream = new FileStream(_destinationFilePath, FileMode.Create, FileAccess.Write, FileShare.None, _bufferSize, true))
			{
				do
				{
					var bytesRead = await contentStream.ReadAsync(buffer, 0, buffer.Length);
					if (bytesRead == 0)
					{
						isMoreToRead = false;
						ReportProgress(totalDownloadSize, totalBytesRead);
						continue;
					}

					await fileStream.WriteAsync(buffer, 0, bytesRead);

					totalBytesRead += bytesRead;
					readCount += 1;

					if (readCount % 100 == 0)
						ReportProgress(totalDownloadSize, totalBytesRead);
				}
				while (isMoreToRead);
			}

			ArrayPool<byte>.Shared.Return(buffer);
		}

		private void ReportProgress(long? totalDownloadSize, long totalBytesRead)
		{
			double? progressPercentage = null;
			if (totalDownloadSize.HasValue)
				progressPercentage = Math.Round((double)totalBytesRead / totalDownloadSize.Value * 100, 2);

			ProgressChanged?.Invoke(totalDownloadSize, totalBytesRead, progressPercentage);
		}
	}
}
