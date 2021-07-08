using System;
using System.Net.Http;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;

namespace Amusoft.PCR.Grpc.Client
{
	public interface IAuthenticationSurface
	{
		Task UpdateTokenStoreAsync(JwtAuthenticationResult authenticationResult);
		Task<string> GetAccessTokenAsync();

		Uri GetAuthenticationUri();
		Task<HttpContent> CreateAuthenticationRequestContentAsync();

		Uri GetRefreshUri();
		Task<HttpContent> CreateRefreshRequestContentAsync();
	}
}