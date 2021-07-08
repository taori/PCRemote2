using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Net.Mime;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Client;
using Amusoft.PCR.Grpc.Common;
using Grpc.Net.Client;
using NLog;

namespace grpcclient
{
    class Program
    {
        private static readonly ILogger Log = LogManager.GetLogger(nameof(Program));

        static async Task Main(string[] args)
        {
            try
            {
                Log.Debug("Starting execution");

                // var targetIp = "https://localhost:44365";
                var targetIp = "https://192.168.0.135:5001";

                var unsafeHttpClient = UnsafeHttpClientFactory.Create(new Uri(targetIp), new AuthenticationSurfaceBase(targetIp));
                var authentication = new StringContent(JsonSerializer.Serialize(new JwtLoginCredentials()
                { User = "admin@admin.com", Password = "123456" }), Encoding.UTF8, MediaTypeNames.Application.Json);
                // var authentication = new StringContent(@"{ ""user"" : ""admin@admin.com"", ""password"": ""123456""}", Encoding.UTF8, MediaTypeNames.Application.Json);
                var grpcChannelOptions = GetChannelOptions(unsafeHttpClient);
                using var channel = GrpcChannel.ForAddress(targetIp, grpcChannelOptions);

                // var response = await grpcChannelOptions.HttpClient.PostAsync($"{targetIp}/Jwt/Authenticate", authentication);
                // var authenticationValue = await response.Content.ReadAsStringAsync();
                // var authenticationModel = JsonSerializer.Deserialize<JwtAuthenticationResult>(authenticationValue, new JsonSerializerOptions(){PropertyNameCaseInsensitive = true});
                // var token = authenticationModel.AccessToken;
                // Console.WriteLine(token);
                // // var token =
                // //     "eyJhbGciOiJIUzUxMiIsInR5cCI6IkpXVCJ9.eyJodHRwOi8vc2NoZW1hcy54bWxzb2FwLm9yZy93cy8yMDA1LzA1L2lkZW50aXR5L2NsYWltcy9uYW1lIjoiYWRtaW5AYWRtaW4uY29tIiwiYXVkIjoiUEMgUmVtb3RlIDIiLCJodHRwOi8vc2NoZW1hcy5taWNyb3NvZnQuY29tL3dzLzIwMDgvMDYvaWRlbnRpdHkvY2xhaW1zL3JvbGUiOiJhZG1pbmlzdHJhdG9yIiwiZXhwIjoxNjI1NzIxMzQ5LCJpc3MiOiJQQyBSZW1vdGUgMiJ9.kyQY4B-ZvfsIaBEtd-bGz-0GjrDOlbZwPsJ1zza8zvjwTJJIZ7AvMTLxRzrx1QuQSX45K-_ktrZd7jzr4I-haA";
                //
                // var requestMessage = new HttpRequestMessage(HttpMethod.Post, $"{targetIp}/Jwt/TokenVerify");
                // requestMessage.Headers.Add("Authorization", $"Bearer " + token);
                // var testResponse = await grpcChannelOptions.HttpClient.SendAsync(requestMessage);
                // var testReply = await testResponse.Content.ReadAsStringAsync();
                // Console.WriteLine($"{(int)testResponse.StatusCode} => {testReply}");
                //
                // return;
                var client = new DesktopIntegrationService.DesktopIntegrationServiceClient(channel);

                Log.Info("Executing shutdown");
                await client.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Seconds = 60 });
                await Task.Delay(5000);

                Log.Info("Execution shutdown abort");
                await client.AbortShutDownAsync(new AbortShutdownRequest());

                Log.Info("Execution complete.");
            }
            catch (Exception e)
            {
                Log.Error(e.ToString());
            }
        }

        private static GrpcChannelOptions GetChannelOptions(HttpClient httpClient)
        {
            var options = new GrpcChannelOptions();
            options.HttpClient = httpClient;
            return options;
        }
    }

    public class AuthenticationSurfaceBase : IAuthenticationSurface
    {
        private readonly string _baseUri;
        private string _accessToken;
        private string _refreshToken;

        public AuthenticationSurfaceBase(string baseUri)
        {
            _baseUri = baseUri;
        }

        public Task UpdateTokenStoreAsync(JwtAuthenticationResult authenticationResult)
        {
            _accessToken = authenticationResult.AccessToken;
            _refreshToken = authenticationResult.RefreshToken;
            return Task.CompletedTask;
        }

        public Task<string> GetAccessTokenAsync()
        {
            return Task.FromResult(_accessToken);
        }

        public Uri GetAuthenticationUri()
        {
            return new($"{_baseUri}/Jwt/Authenticate", UriKind.Absolute);
        }

        public Task<HttpContent> CreateAuthenticationRequestContentAsync()
        {
            return Task.FromResult<HttpContent>(new StringContent(JsonSerializer.Serialize(new JwtLoginCredentials() { User = "admin@admin.com", Password = "123456" }), Encoding.UTF8, MediaTypeNames.Application.Json));
        }

        public Uri GetRefreshUri()
        {
            return new($"{_baseUri}/Jwt/RefreshToken", UriKind.Absolute);
        }

        public Task<HttpContent> CreateRefreshRequestContentAsync()
        {
            // todo fixme
            var authenticationResult = new JwtAuthenticationResult();
            authenticationResult.RefreshToken = _refreshToken;
            authenticationResult.AccessToken = _accessToken;
            return Task.FromResult<HttpContent>(new StringContent(JsonSerializer.Serialize(authenticationResult), Encoding.UTF8, MediaTypeNames.Application.Json));
        }
    }
}
