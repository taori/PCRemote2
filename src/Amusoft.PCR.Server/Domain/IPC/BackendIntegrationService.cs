using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.NetworkInformation;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Model;
using Amusoft.PCR.Model.Entities;
using Amusoft.PCR.Model.Statics;
using Amusoft.PCR.Server.Dependencies;
using Grpc.Core;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.IPC
{
	[Authorize(Policy = PolicyNames.ApiPolicy)]
	public class BackendIntegrationService : DesktopIntegrationService.DesktopIntegrationServiceBase
	{
		private readonly ApplicationDbContext _dbContext;
		private readonly IAuthorizationService _authorizationService;
		public IUserContextChannel InteropService { get; }
		public ILogger<BackendIntegrationService> Logger { get; }

		public BackendIntegrationService(IUserContextChannel interopService, ILogger<BackendIntegrationService> logger, ApplicationDbContext dbContext, IAuthorizationService authorizationService)
		{
			_dbContext = dbContext;
			_authorizationService = authorizationService;
			InteropService = interopService;
			Logger = logger;
		}

		[Authorize(Roles = RoleNames.FunctionReadClipboard)]
		public override async Task<GetClipboardResponse> GetClipboard(GetClipboardRequest request, ServerCallContext context)
		{
			var result = await InteropService.GetClipboardAsync(request.Requestee);
			return new GetClipboardResponse()
			{
				Content = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionWriteClipboard)]
		public override async Task<SetClipboardResponse> SetClipboard(SetClipboardRequest request, ServerCallContext context)
		{
			var result = await InteropService.SetClipboardAsync(request.Requestee, request.Content);
			return new SetClipboardResponse()
			{
				Success = result
			};
		}

		[Authorize]
		public override Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, ServerCallContext context)
		{
			return Task.FromResult(new AuthenticateResponse() {Success = true});
		}

		[Authorize(Roles = RoleNames.FunctionToggleMute)]
		public override async Task<ToggleMuteReply> ToggleMute(ToggleMuteRequest request, ServerCallContext context)
		{
			var result = await InteropService.ToggleMute();
			return new ToggleMuteReply()
			{
				Muted = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionMonitorControl)]
		public override async Task<MonitorOnReply> MonitorOn(MonitorOnRequest request, ServerCallContext context)
		{
			var result = await InteropService.MonitorOn();
			return new MonitorOnReply()
			{
				Success = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionMonitorControl)]
		public override async Task<MonitorOffReply> MonitorOff(MonitorOffRequest request, ServerCallContext context)
		{
			var result = await InteropService.MonitorOff();
			return new MonitorOffReply()
			{
				Success = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionShutdownCancel)]
		public override async Task<AbortShutdownReply> AbortShutDown(AbortShutdownRequest request, ServerCallContext context)
		{
			var success = await InteropService.AbortShutdown();
			return new AbortShutdownReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionShutdown)]
		public override async Task<ShutdownDelayedReply> ShutDownDelayed(ShutdownDelayedRequest request, ServerCallContext context)
		{
			var success = await InteropService.Shutdown(TimeSpan.FromSeconds(request.Seconds), request.Force);
			return new ShutdownDelayedReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionRestart)]
		public override async Task<RestartReply> Restart(RestartRequest request, ServerCallContext context)
		{
			var success = await InteropService.Restart(TimeSpan.FromSeconds(request.Delay), request.Force);
			return new RestartReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionHibernate)]
		public override async Task<HibernateReply> Hibernate(HibernateRequest request, ServerCallContext context)
		{
			var success = await InteropService.Hibernate();
			return new HibernateReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionMasterVolumeControl)]
		public override async Task<SetMasterVolumeReply> SetMasterVolume(SetMasterVolumeRequest request, ServerCallContext context)
		{
			var value = await InteropService.SetMasterVolume(request.Value);
			return new SetMasterVolumeReply()
			{
				Value = value
			};
		}

		[Authorize(Roles = RoleNames.FunctionMasterVolumeControl)]
		public override async Task<GetMasterVolumeReply> GetMasterVolume(GetMasterVolumeRequest request, ServerCallContext context)
		{
			var value = await InteropService.GetMasterVolume();
			return new GetMasterVolumeReply()
			{
				Value = value
			};
		}

		[Authorize(Roles = RoleNames.FunctionSendInput)]
		public override async Task<SendKeysReply> SendKeys(SendKeysRequest request, ServerCallContext context)
		{
			var success = await InteropService.SendKeys(request.Message);
			return new SendKeysReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionSendInput)]
		public override async Task<SendMediaKeysReply> SendMediaKeys(SendMediaKeysRequest request, ServerCallContext context)
		{
			var success = await InteropService.SendMediaKey(request.KeyCode);
			return new SendMediaKeysReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionLockWorkstation)]
		public override async Task<LockWorkStationReply> LockWorkStation(LockWorkStationRequest request, ServerCallContext context)
		{
			var success = await InteropService.LockWorkStation();
			return new LockWorkStationReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionGetProcessList)]
		public override async Task<ProcessListResponse> GetProcessList(ProcessListRequest request, ServerCallContext context)
		{
			var results = await InteropService.GetProcessList();
			var processListResponse = new ProcessListResponse();
			processListResponse.Results.AddRange(results);
			return processListResponse;
		}

		[Authorize(Roles = RoleNames.FunctionKillProcessById)]
		public override async Task<KillProcessResponse> KillProcessById(KillProcessRequest request, ServerCallContext context)
		{
			var success = await InteropService.KillProcessById(request.ProcessId);
			return new KillProcessResponse()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionFocusWindow)]
		public override async Task<FocusWindowResponse> FocusWindow(FocusWindowRequest request, ServerCallContext context)
		{
			var success = await InteropService.FocusProcessWindow(request.ProcessId);
			return new FocusWindowResponse()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionLaunchProgram)]
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

		[Authorize(Roles = RoleNames.FunctionLaunchProgram)]
		public override async Task<GetHostCommandResponse> GetHostCommands(GetHostCommandRequest request, ServerCallContext context)
		{
			var commands = await _dbContext.HostCommands.ToListAsync();
			var response = new GetHostCommandResponse();
			
			foreach (var command in commands)
			{
				var authorizeResult = await _authorizationService.AuthorizeAsync(context.GetHttpContext().User, command, PolicyNames.ApplicationPermissionPolicy);
				Logger.LogDebug("User {User} authorization status for command {Id} is {Status}", context.GetHttpContext().User, command.Id, authorizeResult.Succeeded);
				if (authorizeResult.Succeeded)
				{
					response.Results.Add(new GetHostCommandResponseItem()
					{
						CommandId = command.Id,
						Title = command.CommandName
					});
				}
			}

			return response;
		}

		[Authorize(Roles = RoleNames.FunctionLaunchProgram)]
		public override async Task<InvokeHostCommandResponse> InvokeHostCommand(InvokeHostCommandRequest request, ServerCallContext context)
		{
			var command = await _dbContext.HostCommands.FindAsync(request.Id);
			if (command == null)
			{
				throw new RpcException(new Status(StatusCode.NotFound, $"Command {request.Id} not found"));
			}

			var success = await InteropService.LaunchProgram(command.ProgramPath, command.Arguments);

			return new InvokeHostCommandResponse()
			{
				Success = success
			};
		}

		[AllowAnonymous]
		public override Task<GetHostNameResponse> GetHostName(GetHostNameRequest request, ServerCallContext context)
		{
			return Task.FromResult(new GetHostNameResponse() {Content = Environment.MachineName});
		}

		[Authorize(Roles = RoleNames.FunctionWakeOnLan)]
		public override Task<GetNetworkMacAddressesResponse> GetNetworkMacAddresses(GetNetworkMacAddressesRequest request, ServerCallContext context)
		{
			var response = new GetNetworkMacAddressesResponse();
			var interfaces = NetworkInterface.GetAllNetworkInterfaces()
				.Where(d => d.OperationalStatus == OperationalStatus.Up
				            && d.NetworkInterfaceType == NetworkInterfaceType.Ethernet);
			response.Results.AddRange(interfaces.Select(d => d.GetPhysicalAddress().ToString()).Select(d => new GetNetworkMacAddressesResponseItem(){ MacAddress = d}));

			return Task.FromResult(response);
		}
	}
}