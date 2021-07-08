using System;
using System.Net.Http;

namespace Amusoft.PCR.Grpc.Client
{
    public static class UnsafeHttpClientFactory
    {
        public static HttpClient Create(Uri baseAddress, IAuthenticationSurface authenticationSurface)
        {
            var httpMessageHandler = new GrpcClientHandler(authenticationSurface);
            var httpClient = new HttpClient(httpMessageHandler, true);
            httpClient.BaseAddress = baseAddress;
            return httpClient;
        }
    }
}