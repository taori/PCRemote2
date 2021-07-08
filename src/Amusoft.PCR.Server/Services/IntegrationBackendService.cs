using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model.Statics;
using Amusoft.PCR.Server.Dependencies;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Services
{
	[Authorize(Policy = PolicyNames.ApiPolicy)]
	public class IntegrationBackendService : WindowsInteropService.WindowsInteropServiceBase
	{
		public IInteropService InteropService { get; }
		public ILogger<IntegrationBackendService> Logger { get; }

		public IntegrationBackendService(IInteropService interopService, ILogger<IntegrationBackendService> logger)
		{
			InteropService = interopService;
			Logger = logger;
		}

		public override Task<ToggleMuteReply> ToggleMute(ToggleMuteRequest request, ServerCallContext context)
		{
			// context.GetHttpContext().User
			return base.ToggleMute(request, context);
		}

		public override Task<MonitorOnReply> MonitorOn(MonitorOnRequest request, ServerCallContext context)
		{
			return base.MonitorOn(request, context);
		}

		public override Task<MonitorOffReply> MonitorOff(MonitorOffRequest request, ServerCallContext context)
		{
			return base.MonitorOff(request, context);
		}

		public override async Task<AbortShutdownReply> AbortShutDown(AbortShutdownRequest request, ServerCallContext context)
		{
			Logger.LogDebug("Calling {Name}", nameof(AbortShutDown));
			var success = await InteropService.AbortShutdown();
			return new AbortShutdownReply()
			{
				Success = success
			};
		}

		public override async Task<ShutdownDelayedReply> ShutDownDelayed(ShutdownDelayedRequest request, ServerCallContext context)
		{
			Logger.LogDebug("Calling {Name}", nameof(ShutDownDelayed));
			var success = await InteropService.Shutdown(TimeSpan.FromSeconds(request.Seconds), request.Force);
			return new ShutdownDelayedReply()
			{
				Success = success
			};
		}

		public override Task<RestartReply> Restart(RestartRequest request, ServerCallContext context)
		{
			return base.Restart(request, context);
		}

		public override Task<HibernateReply> Hibernate(HibernateRequest request, ServerCallContext context)
		{
			return base.Hibernate(request, context);
		}

		public override Task<SetMasterVolumeReply> SetMasterVolume(SetMasterVolumeRequest request, ServerCallContext context)
		{
			return base.SetMasterVolume(request, context);
		}

		public override Task<GetMasterVolumeReply> GetMasterVolume(GetMasterVolumeRequest request, ServerCallContext context)
		{
			return base.GetMasterVolume(request, context);
		}

		public override Task<SendKeysReply> SendKeys(SendKeysRequest request, ServerCallContext context)
		{
			return base.SendKeys(request, context);
		}

		public override Task<SendMediaKeysReply> SendMediaKeys(SendMediaKeysRequest request, ServerCallContext context)
		{
			return base.SendMediaKeys(request, context);
		}

		public override Task<LockWorkStationReply> LockWorkStation(LockWorkStationRequest request, ServerCallContext context)
		{
			return base.LockWorkStation(request, context);
		}

		public override Task<ProcessListResponse> GetProcessList(ProcessListRequest request, ServerCallContext context)
		{
			return base.GetProcessList(request, context);
		}

		public override Task<KillProcessResponse> KillProcessById(KillProcessRequest request, ServerCallContext context)
		{
			return base.KillProcessById(request, context);
		}

		public override Task<FocusWindowResponse> FocusWindow(FocusWindowRequest request, ServerCallContext context)
		{
			return base.FocusWindow(request, context);
		}

		public override Task<LaunchProgramResponse> LaunchProgram(LaunchProgramRequest request, ServerCallContext context)
		{
			return base.LaunchProgram(request, context);
		}
	}
}