﻿using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Newtonsoft.Json;

namespace Amusoft.PCR.Grpc.Client
{
	public class GrpcClientHandler : HttpClientHandler
	{
		private static readonly NLog.Logger Log = NLog.LogManager.GetLogger(nameof(GrpcClientHandler));

		public IAuthenticationSurface AuthenticationSurface { get; }

		public GrpcClientHandler(IAuthenticationSurface authenticationSurface)
		{
			AuthenticationSurface = authenticationSurface;
			Log.Trace("Enabling DangerousAcceptAnyServerCertificateValidator");
			ServerCertificateCustomValidationCallback = DangerousAcceptAnyServerCertificateValidator;
		}

		private static readonly SemaphoreSlim Semaphore = new SemaphoreSlim(1);

		protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
		{
			try
			{
				Log.Trace("Sending request to {Uri}", request.RequestUri);
				var currentAccessToken = await AuthenticationSurface.GetAccessTokenAsync();
				if (currentAccessToken == null)
				{
					Log.Trace("No access token available - sending request as is");
					return await base.SendAsync(request, cancellationToken);
				}
				else
				{
					Log.Trace("Using {Token} to authenticate current request", currentAccessToken);
					return await HandleExistingAuthenticationAsync(request, cancellationToken, currentAccessToken);
				}
			}
			catch (Exception e)
			{
				Log.Error(e, "Unexpected error occured");
				return new HttpResponseMessage(HttpStatusCode.BadRequest);
			}
		}

		private async Task<HttpResponseMessage> HandleExistingAuthenticationAsync(HttpRequestMessage request, CancellationToken cancellationToken, string currentAccessToken)
		{
			request.Headers.Add("Authorization", "Bearer " + currentAccessToken);
			var response = await base.SendAsync(request, cancellationToken);

			var expired = response.Headers.Contains("X-Token-Expired");

			//test for 403 and actual bearer token in initial request
			if (response.StatusCode == HttpStatusCode.Unauthorized
			    && request.Headers.Any(d => d.Key == "Authorization" && d.Value.Any(d => d.StartsWith("Bearer"))))
			{
				Log.Trace("Found Bearer authentication and current request was unauthorized - Authenticating ...");
				try
				{
					await Semaphore.WaitAsync(cancellationToken);

					var authRequest = await GetAuthenticationRequestMessage(response);
					Log.Debug("Requesting authentication response using {Url}", authRequest.RequestUri);
					using (var authResponse = await base.SendAsync(authRequest, cancellationToken))
					{
						if (!authResponse.IsSuccessStatusCode)
						{
							Log.Error("Invalid status code {StatusCode}, Reason: {Reason}", authResponse.StatusCode, authResponse.ReasonPhrase);
							throw new Exception(string.Format("Invalid status code {0}, Reason: {1}", authResponse.StatusCode, authResponse.ReasonPhrase));
						}

						var authResponseString = await authResponse.Content.ReadAsStringAsync();
						var authResponseResult = JsonConvert.DeserializeObject<JwtAuthenticationResult>(authResponseString);

						if (authResponseResult == null || authResponseResult.AuthenticationRequired)
						{
							Log.Debug("RemoteEnd forced reauthentication");
							return await HandleNoAuthenticationAsync(request, cancellationToken, true);
						}

						Log.Debug("Updating token store");
						await AuthenticationSurface.UpdateTokenStoreAsync(authResponseResult);

						Log.Trace("Assigning accessToken to request");
						request.Headers.Remove("Authorization");
						request.Headers.Add("Authorization", "Bearer " + authResponseResult.AccessToken);

						response.Dispose();
						// Retry with updated token
						Log.Trace("Executing original request");
						response = await base.SendAsync(request, cancellationToken);
					}
				}
				catch (Exception e)
				{
					Log.Error(e, "Failed to handle original request");
					throw;
				}
				finally
				{
					Semaphore.Release();
				}
			}

			return response;
		}

		private async Task<HttpResponseMessage> HandleNoAuthenticationAsync(HttpRequestMessage request, CancellationToken cancellationToken, bool skipSemaphore)
		{
			try
			{
				if (!skipSemaphore)
					await Semaphore.WaitAsync(cancellationToken);

				var authRequest = new HttpRequestMessage(HttpMethod.Post, AuthenticationSurface.GetAuthenticationUri())
					{Content = await AuthenticationSurface.CreateAuthenticationRequestContentAsync()};

				Log.Trace("Requesting authentication response");
				using (var authResponse = await base.SendAsync(authRequest, cancellationToken))
				{
					if (!authResponse.IsSuccessStatusCode)
					{
						Log.Error("Invalid status code {StatusCode}, Reason: {Reason}", authResponse.StatusCode, authResponse.ReasonPhrase);
						throw new Exception(string.Format("Invalid status code {0}, Reason: {1}",
							authResponse.StatusCode, authResponse.ReasonPhrase));
					}

					var authResponseString = await authResponse.Content.ReadAsStringAsync();
					var authResponseResult = JsonConvert.DeserializeObject<JwtAuthenticationResult>(authResponseString);

					Log.Debug("Updating token store");
					await AuthenticationSurface.UpdateTokenStoreAsync(authResponseResult);

					Log.Trace("Assigning accessToken to request");
					request.Headers.Remove("Authorization");
					request.Headers.Add("Authorization", "Bearer " + authResponseResult.AccessToken);

					Log.Trace("Executing original request");
					return await base.SendAsync(request, cancellationToken);
				}
			}
			catch (Exception e)
			{
				Log.Error(e);
				throw;
			}
			finally
			{
				if (!skipSemaphore)
					Semaphore.Release();
			}
		}

		private async Task<HttpRequestMessage> GetAuthenticationRequestMessage(HttpResponseMessage httpResponseMessage)
		{
			if (httpResponseMessage.Headers.Contains("X-Token-Expired"))
			{
				return new HttpRequestMessage(HttpMethod.Post, AuthenticationSurface.GetRefreshUri()) { Content = await AuthenticationSurface.CreateRefreshRequestContentAsync() };
			}
			else
			{
				return new HttpRequestMessage(HttpMethod.Post, AuthenticationSurface.GetAuthenticationUri()) { Content = await AuthenticationSurface.CreateAuthenticationRequestContentAsync() };
			}
		}
	}
}