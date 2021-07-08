using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model.Statics;
using Amusoft.PCR.Server.BackgroundServices;
using Amusoft.PCR.Server.Dependencies;
using Grpc.Core;
using GrpcDotNetNamedPipes;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Services
{
	[Authorize(Policy = PolicyNames.ApiPolicy)]
	public class BackendIntegrationService : DesktopIntegrationService.DesktopIntegrationServiceBase
	{
		public IInteropService InteropService { get; }
		public ILogger<BackendIntegrationService> Logger { get; }

		public BackendIntegrationService(IInteropService interopService, ILogger<BackendIntegrationService> logger)
		{
			InteropService = interopService;
			Logger = logger;
		}

		[Authorize(Roles = RoleNames.Audio)]
		public override async Task<ToggleMuteReply> ToggleMute(ToggleMuteRequest request, ServerCallContext context)
		{
			var result = await InteropService.ToggleMute();
			return new ToggleMuteReply()
			{
				Muted = result
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<MonitorOnReply> MonitorOn(MonitorOnRequest request, ServerCallContext context)
		{
			var result = await InteropService.MonitorOn();
			return new MonitorOnReply()
			{
				Success = result
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<MonitorOffReply> MonitorOff(MonitorOffRequest request, ServerCallContext context)
		{
			var result = await InteropService.MonitorOff();
			return new MonitorOffReply()
			{
				Success = result
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<AbortShutdownReply> AbortShutDown(AbortShutdownRequest request, ServerCallContext context)
		{
			var success = await InteropService.AbortShutdown();
			return new AbortShutdownReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<ShutdownDelayedReply> ShutDownDelayed(ShutdownDelayedRequest request, ServerCallContext context)
		{
			var success = await InteropService.Shutdown(TimeSpan.FromSeconds(request.Seconds), request.Force);
			return new ShutdownDelayedReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<RestartReply> Restart(RestartRequest request, ServerCallContext context)
		{
			var success = await InteropService.Restart(TimeSpan.FromSeconds(request.Delay), request.Force);
			return new RestartReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<HibernateReply> Hibernate(HibernateRequest request, ServerCallContext context)
		{
			var success = await InteropService.Hibernate();
			return new HibernateReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Audio)]
		public override async Task<SetMasterVolumeReply> SetMasterVolume(SetMasterVolumeRequest request, ServerCallContext context)
		{
			var value = await InteropService.SetMasterVolume(request.Value);
			return new SetMasterVolumeReply()
			{
				Value = value
			};
		}

		[Authorize(Roles = RoleNames.Audio)]
		public override async Task<GetMasterVolumeReply> GetMasterVolume(GetMasterVolumeRequest request, ServerCallContext context)
		{
			var value = await InteropService.GetMasterVolume();
			return new GetMasterVolumeReply()
			{
				Value = value
			};
		}

		[Authorize(Roles = RoleNames.ActiveWindow)]
		public override async Task<SendKeysReply> SendKeys(SendKeysRequest request, ServerCallContext context)
		{
			var success = await InteropService.SendKeys(request.Message);
			return new SendKeysReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.ActiveWindow)]
		public override async Task<SendMediaKeysReply> SendMediaKeys(SendMediaKeysRequest request, ServerCallContext context)
		{
			var success = await InteropService.SendMediaKey(request.KeyCode);
			return new SendMediaKeysReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Computer)]
		public override async Task<LockWorkStationReply> LockWorkStation(LockWorkStationRequest request, ServerCallContext context)
		{
			var success = await InteropService.LockWorkStation();
			return new LockWorkStationReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Processes)]
		public override async Task<ProcessListResponse> GetProcessList(ProcessListRequest request, ServerCallContext context)
		{
			var results = await InteropService.GetProcessList();
			var processListResponse = new ProcessListResponse();
			processListResponse.Results.AddRange(results);
			return processListResponse;
		}

		[Authorize(Roles = RoleNames.Processes)]
		public override async Task<KillProcessResponse> KillProcessById(KillProcessRequest request, ServerCallContext context)
		{
			var success = await InteropService.KillProcessById(request.ProcessId);
			return new KillProcessResponse()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.ActiveWindow)]
		public override async Task<FocusWindowResponse> FocusWindow(FocusWindowRequest request, ServerCallContext context)
		{
			var success = await InteropService.FocusProcessWindow(request.ProcessId);
			return new FocusWindowResponse()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.Processes)]
		public override async Task<LaunchProgramResponse> LaunchProgram(LaunchProgramRequest request, ServerCallContext context)
		{
			if (request.Arguments == null)
			{
				var success = await InteropService.LaunchProgram(request.ProgramName);
				return new LaunchProgramResponse()
				{
					Success = success
				};
			}
			else
			{
				var success = await InteropService.LaunchProgram(request.ProgramName, request.Arguments);
				return new LaunchProgramResponse()
				{
					Success = success
				};
			}
		}
	}
}