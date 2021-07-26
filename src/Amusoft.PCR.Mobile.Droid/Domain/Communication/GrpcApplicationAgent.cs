using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Mobile.Droid.Domain.Common;
using Amusoft.PCR.Mobile.Droid.Helpers;
using Android.App;
using Android.Widget;
using Google.Protobuf.Collections;
using Grpc.Core;
using Grpc.Core.Interceptors;
using Grpc.Net.Client;
using Grpc.Net.Client.Configuration;
using Grpc.Net.Client.Web;
using Java.Security;
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
				GrpcRequestObserver.NotifyCallRunning();
				return await functionCall(_client);
			}
			catch (RpcException e) when (e.StatusCode == StatusCode.PermissionDenied)
			{
				ToastHelper.Display(Application.Context, "Permission denied", ToastLength.Long);
				return defaultValue;
			}
			catch (Exception e)
			{
				Log.Error(e, methodName + " failed.");
				GrpcRequestObserver.NotifyCallFailed(e);
				return defaultValue;
			}
			finally
			{
				GrpcRequestObserver.NotifyCallFinished();
			}
		}

		public async Task<bool> AuthenticateAsync()
		{
			return await SecuredCallAsync(async (d) => (await d.AuthenticateAsync(new AuthenticateRequest())).Success, false);
		}

		public async Task<bool> KillProcessByIdAsync(TimeSpan timeout, int processId)
		{
			return await SecuredCallAsync(async (d) => (await d.KillProcessByIdAsync(new KillProcessRequest(){ProcessId = processId}, deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}

		private static readonly IList<ProcessListResponseItem> EmptyProcessList = new List<ProcessListResponseItem>();

		public async Task<IList<ProcessListResponseItem>> GetProcessListAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async(d) => ((IList<ProcessListResponseItem>)(await d.GetProcessListAsync(new ProcessListRequest(), deadline: DateTime.UtcNow.Add(timeout))).Results), EmptyProcessList);
		}

		public async Task<bool> MonitorOnAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async(d) => (await d.MonitorOnAsync(new MonitorOnRequest(), deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}

		public async Task<bool> MonitorOffAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async(d) => (await d.MonitorOffAsync(new MonitorOffRequest(), deadline: DateTime.UtcNow.Add(timeout))).Success, false);
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
			return await SecuredCallAsync(
				async (d) =>
					(await d.ShutDownDelayedAsync(
						new ShutdownDelayedRequest() {
							Force = quitApplications,
							Seconds = (int) delay.TotalSeconds},
						deadline: DateTime.UtcNow.Add(timeout))).Success, false);
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

		public async Task<bool> InvokeHostCommand(TimeSpan timeout, string id)
		{
			return await SecuredCallAsync(async (d) => (await d.InvokeHostCommandAsync(new InvokeHostCommandRequest(){ Id = id}, deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}

		private static readonly IList<GetHostCommandResponseItem> GetHostCommandsAsyncEmpty = new List<GetHostCommandResponseItem>();
		public async Task<IList<GetHostCommandResponseItem>> GetHostCommandsAsync(TimeSpan timeout)
		{
			return await SecuredCallAsync(async (d) => (await d.GetHostCommandsAsync(new GetHostCommandRequest(), deadline: DateTime.UtcNow.Add(timeout))).Results as IList<GetHostCommandResponseItem>, GetHostCommandsAsyncEmpty);
		}

		public async Task<string> GetClipboardAsync(TimeSpan timeout, string requestee)
		{
			return await SecuredCallAsync(async (d) => (await d.GetClipboardAsync(new GetClipboardRequest(){ Requestee = requestee}, deadline: DateTime.UtcNow.Add(timeout))).Content, default);
		}

		public async Task<bool> SetClipboardAsync(TimeSpan timeout, string requestee, string content)
		{
			return await SecuredCallAsync(async (d) => (await d.SetClipboardAsync(new SetClipboardRequest(){ Requestee = requestee, Content = content}, deadline: DateTime.UtcNow.Add(timeout))).Success, false);
		}
	}
}