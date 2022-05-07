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
using Amusoft.PCR.Server.Domain.Authorization;
using Grpc.Core;
using Grpc.Core.Utils;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Domain.IPC
{
	[Authorize(Policy = PolicyNames.ApiPolicy)]
	public class BackendIntegrationService : DesktopIntegrationService.DesktopIntegrationServiceBase
	{
		private readonly IJwtTokenService _jwtTokenService;
		private readonly ApplicationDbContext _dbContext;
		private readonly IAuthorizationService _authorizationService;
		private readonly IUserContextChannel _interopService;
		private readonly ILogger<BackendIntegrationService> _logger;

		public BackendIntegrationService(IUserContextChannel interopService, 
			IJwtTokenService jwtTokenService,
			ILogger<BackendIntegrationService> logger,
			ApplicationDbContext dbContext, 
			IAuthorizationService authorizationService)
		{
			_jwtTokenService = jwtTokenService;
			_dbContext = dbContext;
			_authorizationService = authorizationService;
			_interopService = interopService;
			_logger = logger;
		}


		[Authorize(Roles = RoleNames.FunctionMouseControl)]
		public override async Task<SendMouseMoveResponse> SendMouseMove(IAsyncStreamReader<SendMouseMoveRequestItem> requestStream, ServerCallContext context)
		{
			await _interopService.SendMouseMoveAsync(requestStream, context.CancellationToken);
			return new SendMouseMoveResponse();
		}

		[Authorize(Roles = RoleNames.FunctionMouseControl)]
		public override async Task<DefaultResponse> SendLeftMouseButtonClick(DefaultRequest request, ServerCallContext context)
		{
			var result = await _interopService.SendLeftMouseClickAsync();
			return new DefaultResponse() { Success = result };
		}

		[Authorize(Roles = RoleNames.FunctionMouseControl)]
		public override async Task<DefaultResponse> SendRightMouseButtonClick(DefaultRequest request, ServerCallContext context)
		{
			var result = await _interopService.SendRightMouseClickAsync();
			return new DefaultResponse() { Success = result };
		}

		[Authorize(Roles = RoleNames.FunctionReadClipboard)]
		public override async Task<GetClipboardResponse> GetClipboard(GetClipboardRequest request, ServerCallContext context)
		{
			var result = await _interopService.GetClipboardAsync(request.Requestee);
			return new GetClipboardResponse()
			{
				Content = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionWriteClipboard)]
		public override async Task<SetClipboardResponse> SetClipboard(SetClipboardRequest request, ServerCallContext context)
		{
			var result = await _interopService.SetClipboardAsync(request.Requestee, request.Content);
			return new SetClipboardResponse()
			{
				Success = result
			};
		}

		[AllowAnonymous]
		public override async Task<LoginResponse> Login(LoginRequest request, ServerCallContext context)
		{
			var tokenData = await _jwtTokenService.CreateAuthenticationAsync(request.User, request.Password);
			return new LoginResponse()
			{
				AccessToken = tokenData.AccessToken,
				RefreshToken = tokenData.RefreshToken,
				InvalidCredentials = tokenData.InvalidCredentials
			};
		}

		[Authorize]
		public override async Task<CheckIsAuthenticatedResponse> CheckIsAuthenticated(CheckIsAuthenticatedRequest request, ServerCallContext context)
		{
			var authenticated = await IsContextAuthenticated(context);
			return new CheckIsAuthenticatedResponse() { Result = authenticated };
		}

		private static Task<bool> IsContextAuthenticated(ServerCallContext context)
		{
			return Task.FromResult(context.GetHttpContext()?.User?.Identity?.IsAuthenticated ?? false);
		}

		[Authorize]
		public override Task<AuthenticateResponse> Authenticate(AuthenticateRequest request, ServerCallContext context)
		{
			return Task.FromResult(new AuthenticateResponse() {Success = true});
		}

		[Authorize(Roles = RoleNames.FunctionToggleMute)]
		public override async Task<ToggleMuteReply> ToggleMute(ToggleMuteRequest request, ServerCallContext context)
		{
			var result = await _interopService.ToggleMute();
			return new ToggleMuteReply()
			{
				Muted = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionMonitorControl)]
		public override async Task<MonitorOnReply> MonitorOn(MonitorOnRequest request, ServerCallContext context)
		{
			var result = await _interopService.MonitorOn();
			return new MonitorOnReply()
			{
				Success = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionMonitorControl)]
		public override async Task<MonitorOffReply> MonitorOff(MonitorOffRequest request, ServerCallContext context)
		{
			var result = await _interopService.MonitorOff();
			return new MonitorOffReply()
			{
				Success = result
			};
		}

		[Authorize(Roles = RoleNames.FunctionShutdownCancel)]
		public override async Task<AbortShutdownReply> AbortShutDown(AbortShutdownRequest request, ServerCallContext context)
		{
			var success = await _interopService.AbortShutdown();
			return new AbortShutdownReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionShutdown)]
		public override async Task<ShutdownDelayedReply> ShutDownDelayed(ShutdownDelayedRequest request, ServerCallContext context)
		{
			var success = await _interopService.Shutdown(TimeSpan.FromSeconds(request.Seconds), request.Force);
			return new ShutdownDelayedReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionRestart)]
		public override async Task<RestartReply> Restart(RestartRequest request, ServerCallContext context)
		{
			var success = await _interopService.Restart(TimeSpan.FromSeconds(request.Delay), request.Force);
			return new RestartReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionHibernate)]
		public override async Task<HibernateReply> Hibernate(HibernateRequest request, ServerCallContext context)
		{
			var success = await _interopService.Hibernate();
			return new HibernateReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionMasterVolumeControl)]
		public override async Task<SetMasterVolumeReply> SetMasterVolume(SetMasterVolumeRequest request, ServerCallContext context)
		{
			var value = await _interopService.SetMasterVolume(request.Value);
			return new SetMasterVolumeReply()
			{
				Value = value
			};
		}

		[Authorize(Roles = RoleNames.FunctionMasterVolumeControl)]
		public override async Task<GetMasterVolumeReply> GetMasterVolume(GetMasterVolumeRequest request, ServerCallContext context)
		{
			var value = await _interopService.GetMasterVolume();
			return new GetMasterVolumeReply()
			{
				Value = value
			};
		}

		[Authorize(Roles = RoleNames.FunctionMasterVolumeControl)]
		public override async Task<AudioFeedResponse> GetAudioFeeds(AudioFeedRequest request, ServerCallContext context)
		{
			var value = await _interopService.GetAudioFeedsResponse();
			return value;
		}

		public override async Task<DefaultResponse> UpdateAudioFeed(UpdateAudioFeedRequest request, ServerCallContext context)
		{
			var value = await _interopService.UpdateAudioFeed(request);
			return value;
		}

		[Authorize(Roles = RoleNames.FunctionSendInput)]
		public override async Task<SendKeysReply> SendKeys(SendKeysRequest request, ServerCallContext context)
		{
			var success = await _interopService.SendKeys(request.Message);
			return new SendKeysReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionSendInput)]
		public override async Task<SendMediaKeysReply> SendMediaKeys(SendMediaKeysRequest request, ServerCallContext context)
		{
			var success = await _interopService.SendMediaKey(request.KeyCode);
			return new SendMediaKeysReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionLockWorkstation)]
		public override async Task<LockWorkStationReply> LockWorkStation(LockWorkStationRequest request, ServerCallContext context)
		{
			var success = await _interopService.LockWorkStation();
			return new LockWorkStationReply()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionGetProcessList)]
		public override async Task<ProcessListResponse> GetProcessList(ProcessListRequest request, ServerCallContext context)
		{
			var results = await _interopService.GetProcessList();
			var processListResponse = new ProcessListResponse();
			processListResponse.Results.AddRange(results);
			return processListResponse;
		}

		[Authorize(Roles = RoleNames.FunctionKillProcessById)]
		public override async Task<KillProcessResponse> KillProcessById(KillProcessRequest request, ServerCallContext context)
		{
			var success = await _interopService.KillProcessById(request.ProcessId);
			return new KillProcessResponse()
			{
				Success = success
			};
		}

		[Authorize(Roles = RoleNames.FunctionFocusWindow)]
		public override async Task<FocusWindowResponse> FocusWindow(FocusWindowRequest request, ServerCallContext context)
		{
			var success = await _interopService.FocusProcessWindow(request.ProcessId);
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
				var success = await _interopService.LaunchProgram(request.ProgramName);
				return new LaunchProgramResponse()
				{
					Success = success
				};
			}
			else
			{
				var success = await _interopService.LaunchProgram(request.ProgramName, request.Arguments);
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
				_logger.LogDebug("User {User} authorization status for command {Id} is {Status}", context.GetHttpContext().User, command.Id, authorizeResult.Succeeded);
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

			var success = await _interopService.LaunchProgram(command.ProgramPath, command.Arguments);

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

		[AllowAnonymous]
		public override async Task<StringResponse> SetUserPassword(ChangeUserPasswordRequest request, ServerCallContext context)
		{
			var result = await _interopService.SetUserPassword(request);
			if (result.Success)
				return new StringResponse() { Content = string.Empty, Success = false };

			return result;
		}
	}
}