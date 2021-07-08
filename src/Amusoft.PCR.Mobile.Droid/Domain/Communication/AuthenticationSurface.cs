using System;
using System.Net.Http;
using System.Net.Mime;
using System.Text;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Grpc.Common;
using Newtonsoft.Json;
using Xamarin.Essentials;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public class AuthenticationSurface : IAuthenticationSurface
	{
		private readonly string _uriString;

		public AuthenticationSurface(string uriString)
		{
			_uriString = uriString;
		}

		public async Task UpdateTokenStoreAsync(JwtAuthenticationResult authenticationResult)
		{
			await SecureStorage.SetAsync($"{_uriString}.AccessToken", authenticationResult.AccessToken);
			await SecureStorage.SetAsync($"{_uriString}.RefreshToken", authenticationResult.RefreshToken);
		}

		public async Task<string> GetAccessTokenAsync()
		{
			return await SecureStorage.GetAsync($"{_uriString}.AccessToken");
		}


		public Uri GetAuthenticationUri()
		{
			return new Uri($"{_uriString}/Jwt/Authenticate", UriKind.Absolute);
		}

		public async Task<HttpContent> CreateAuthenticationRequestContentAsync()
		{
			var credentials = await LoginDialog.GetInputAsync("Login");
			// var credentials = new JwtLoginCredentials() { User = "admin@admin.com", Password = "123456" };
			return new StringContent(JsonConvert.SerializeObject(credentials), Encoding.UTF8, MediaTypeNames.Application.Json);
		}

		public Uri GetRefreshUri()
		{
			return new Uri($"{_uriString}/Jwt/RefreshToken", UriKind.Absolute);
		}

		public async Task<HttpContent> CreateRefreshRequestContentAsync()
		{
			var authenticationResult = new JwtAuthenticationResult();
			authenticationResult.AccessToken = await SecureStorage.GetAsync($"{_uriString}.AccessToken");
			authenticationResult.RefreshToken = await SecureStorage.GetAsync($"{_uriString}.RefreshToken");
			return new StringContent(JsonConvert.SerializeObject(authenticationResult), Encoding.UTF8, MediaTypeNames.Application.Json);
		}
    }
}