using System;
using System.Net.Http;
using Grpc.Net.Client.Web;

namespace Amusoft.PCR.Grpc.Client
{
    public static class GrpcWebHttpClientFactory
    {
        public static HttpClient Create(Uri baseAddress, IAuthenticationSurface authenticationSurface)
        {
	        var webHandler = new GrpcWebHandler(GrpcWebMode.GrpcWebText, new GrpcClientHandler(authenticationSurface));
            var httpClient = new HttpClient(webHandler, true);
            httpClient.BaseAddress = baseAddress;
            return httpClient;
        }
    }
}