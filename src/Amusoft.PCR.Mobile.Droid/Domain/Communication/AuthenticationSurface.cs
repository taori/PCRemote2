using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Newtonsoft.Json;
using NLog;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public class AuthenticationStorage
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AuthenticationStorage));

		public HostEndpointAddress Address { get; }

		public AuthenticationStorage(HostEndpointAddress address)
		{
			Address = address;
		}

		public void Clear()
		{
			Log.Debug("Removing {Key} from {Place}", $"{Address}.AccessToken", nameof(SecureStorage));
			SecureStorage.Remove($"{Address}.AccessToken");

			Log.Debug("Removing {Key} from {Place}", $"{Address}.RefreshToken", nameof(SecureStorage));
			SecureStorage.Remove($"{Address}.RefreshToken");
		}

		public async Task UpdateAsync(string accessToken, string refreshToken)
		{
			await SecureStorage.SetAsync($"{Address}.AccessToken", accessToken);
			await SecureStorage.SetAsync($"{Address}.RefreshToken", refreshToken);
		}

		public async Task<string> GetAccessTokenAsync()
		{
			return await SecureStorage.GetAsync($"{Address}.AccessToken");
		}

		public async Task<string> GetRefreshTokenAsync()
		{
			return await SecureStorage.GetAsync($"{Address}.RefreshToken");
		}
	}

	public class AuthenticationSurface : IAuthenticationSurface
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(AuthenticationSurface));

		private readonly AuthenticationStorage _authenticationStorage;
		private readonly string _address;

		public AuthenticationSurface(HostEndpointAddress address, AuthenticationStorage authenticationStorage)
		{
			_authenticationStorage = authenticationStorage;
			_address = address.FullAddress;
		}

		public async Task UpdateTokenStoreAsync(JwtAuthenticationResult authenticationResult)
		{
			if (authenticationResult == null || authenticationResult.AuthenticationRequired)
			{
				_authenticationStorage.Clear();
			}
			else
			{
				await _authenticationStorage.UpdateAsync(authenticationResult.AccessToken, authenticationResult.RefreshToken);
			}
		}

		public async Task<string> GetAccessTokenAsync()
		{
			return await _authenticationStorage.GetAccessTokenAsync();
		}

		public Uri GetAuthenticationUri()
		{
			return new Uri($"{_address}/Jwt/Authenticate", UriKind.Absolute);
		}

		public async Task<HttpContent> CreateAuthenticationRequestContentAsync()
		{
			var credentials = await LoginDialog.GetInputAsync("Login");
			return new StringContent(JsonConvert.SerializeObject(credentials), Encoding.UTF8, MediaTypeNames.Application.Json);
		}

		public Uri GetRefreshUri()
		{
			return new Uri($"{_address}/Jwt/RefreshToken", UriKind.Absolute);
		}

		public async Task<HttpContent> CreateRefreshRequestContentAsync()
		{
			var authenticationResult = new JwtAuthenticationResult();
			authenticationResult.AccessToken = await _authenticationStorage.GetAccessTokenAsync();
			authenticationResult.RefreshToken = await _authenticationStorage.GetRefreshTokenAsync();
			return new StringContent(JsonConvert.SerializeObject(authenticationResult), Encoding.UTF8, MediaTypeNames.Application.Json);
		}
    }
}