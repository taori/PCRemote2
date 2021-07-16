using System;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client.Web;
using NLog;

namespace Amusoft.PCR.Mobile.Droid.Domain.Communication
{
	public class GrpcApplicationAgent : IDisposable
	{
		private readonly GrpcChannel _channel;

		public GrpcApplicationAgent(GrpcChannel channel)
		{
			_channel = channel;
			_desktopIntegrationClient = new DesktopIntegrationService.DesktopIntegrationServiceClient(_channel);
			DesktopClient = new SimpleDesktopClient(_desktopIntegrationClient);
		}

		public SimpleDesktopClient DesktopClient { get; }

		private DesktopIntegrationService.DesktopIntegrationServiceClient _desktopIntegrationClient;

		public void Dispose()
		{
			_channel?.Dispose();
		}
	}

	public class SimpleDesktopClient
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(SimpleDesktopClient));

		private readonly DesktopIntegrationService.DesktopIntegrationServiceClient _client;

		public SimpleDesktopClient(DesktopIntegrationService.DesktopIntegrationServiceClient client)
		{
			_client = client;
		}

		private async Task<TResult> SecuredCallAsync<TResult>(Func<DesktopIntegrationService.DesktopIntegrationServiceClient, Task<TResult>> functionCall, TResult defaultValue, [CallerMemberName] string methodName = default)
		{
			try
			{
				return await functionCall(_client);
			}
			catch (Exception e)
			{
				Log.Error(e, methodName + " failed.");
				return defaultValue;
			}
		}

		public async Task<int> GetMasterVolumeAsync(TimeSpan timeout, int defaultValue)
		{
			return await SecuredCallAsync(async(d) => (await d.GetMasterVolumeAsync(new GetMasterVolumeRequest(), deadline: DateTime.UtcNow.Add(timeout))).Value, defaultValue);
		}

		public async Task<int> SetMasterVolumeAsync(TimeSpan timeout, int value)
		{
			return await SecuredCallAsync(async (d) => (await d.SetMasterVolumeAsync(new SetMasterVolumeRequest() { Value = value }, deadline: DateTime.UtcNow.Add(timeout))).Value, 0);
		}

		public async Task<bool> ShutDownDelayedAsync(TimeSpan timeout, bool quitApplications, TimeSpan delay)
		{
			return await SecuredCallAsync(async (d) => (await d.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Force = quitApplications, Seconds = (int)delay.TotalSeconds }, deadline: DateTime.UtcNow.Add(timeout))).Success, false);

		}

		public async Task<bool> AbortShutDownAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async (d) => (await d.AbortShutDownAsync(new AbortShutdownRequest(), deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}

		public async Task<bool?> ToggleMuteAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async (d) => (bool?)(await d.ToggleMuteAsync(new ToggleMuteRequest(), deadline: DateTime.UtcNow.Add(timeout))).Muted, (bool?)null);
		}

		public async Task<bool> RestartAsync(TimeSpan timeout, bool quitApplications, TimeSpan delay)
		{
			return await SecuredCallAsync(async (d) => (await d.RestartAsync(new RestartRequest(){ Force = quitApplications, Delay = (int)delay.TotalSeconds}, deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}

		public async Task<bool> LockWorkStationAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async (d) => (await d.LockWorkStationAsync(new LockWorkStationRequest(), deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}
	}
}