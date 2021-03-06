﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Google.Protobuf.Collections;
using GrpcDotNetNamedPipes;
using Microsoft.Extensions.Logging;

namespace Amusoft.PCR.Server.Dependencies
{
	public interface IInteropService
	{
		Task<bool> ToggleMute();
		Task<bool> MonitorOn();
		Task<bool> MonitorOff();
		Task<bool> LockWorkStation();

		/// <summary>
		/// See https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?redirectedfrom=MSDN&view=net-5.0
		/// </summary>
		/// <param name="keys"></param>
		Task<bool> SendKeys(string keys);

		Task<int> SetMasterVolume(int value);
		Task<int> GetMasterVolume();
		Task<bool> Shutdown(TimeSpan delay, bool force);
		Task<bool> AbortShutdown();
		Task<bool> Hibernate();
		Task<bool> Restart(TimeSpan delay, bool force);
		Task<IList<ProcessListResponseItem>> GetProcessList();
		Task<bool> KillProcessById(int processId);
		Task<bool> FocusProcessWindow(int processId);
		Task<bool> LaunchProgram(string programName, string arguments = default);
		Task<bool> SendMediaKey(SendMediaKeysRequest.Types.MediaKeyCode code);
	}

	public class InteropService : IInteropService
	{
		private readonly ILogger<InteropService> _logger;
		private readonly DesktopIntegrationService.DesktopIntegrationServiceClient _service;

		public InteropService(NamedPipeChannel channel, ILogger<InteropService> logger)
		{
			_logger = logger;
			_service = new DesktopIntegrationService.DesktopIntegrationServiceClient(channel);
		}

		public async Task<bool> ToggleMute()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(ToggleMute));
				var reply = await _service.ToggleMuteAsync(new ToggleMuteRequest());
				return reply.Muted;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(ToggleMute));
				return false;
			}
		}

		public async Task<bool> MonitorOn()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(MonitorOn));
				await _service.MonitorOnAsync(new MonitorOnRequest());
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(MonitorOn));
				return false;
			}
		}

		public async Task<bool> MonitorOff()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(MonitorOff));
				await _service.MonitorOffAsync(new MonitorOffRequest());
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(MonitorOff));
				return false;
			}
		}

		public async Task<bool> LockWorkStation()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(LockWorkStation));
				await _service.LockWorkStationAsync(new LockWorkStationRequest());
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(LockWorkStation));
				return false;
			}
		}

		public async Task<bool> SendKeys(string keys)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(SendKeys));
				await _service.SendKeysAsync(new SendKeysRequest(){Message = keys});
				return true;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with [{Keys}]", nameof(SendKeys), keys);
				return false;
			}
		}

		public async Task<int> SetMasterVolume(int value)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(SetMasterVolume));
				var reply = await _service.SetMasterVolumeAsync(new SetMasterVolumeRequest() {Value = value});
				return reply.Value;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] to [{Value}]", nameof(SetMasterVolume), value);
				return -1;
			}
		}

		public async Task<int> GetMasterVolume()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(GetMasterVolume));
				var reply = await _service.GetMasterVolumeAsync(new GetMasterVolumeRequest());
				return reply.Value;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(GetMasterVolume));
				return -1;
			}
		}

		public async Task<bool> Shutdown(TimeSpan delay, bool force)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(Shutdown));
				var reply = await _service.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Seconds = (int)delay.TotalSeconds, Force = force});
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with delay [{Delay}] and force close [{Force}]", nameof(Shutdown), delay, force);
				return false;
			}
		}

		public async Task<bool> AbortShutdown()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(AbortShutdown));
				var reply = await _service.AbortShutDownAsync(new AbortShutdownRequest());
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(AbortShutdown));
				return false;
			}
		}

		public async Task<bool> Hibernate()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(Hibernate));
				var reply = await _service.HibernateAsync(new HibernateRequest());
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(Hibernate));
				return false;
			}
		}

		public async Task<bool> Restart(TimeSpan delay, bool force)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(Restart));
				var reply = await _service.RestartAsync(new RestartRequest(){ Delay = (int)delay.TotalSeconds, Force = force});
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with delay [{Delay}] and force close [{Force}]", nameof(Restart), delay, force);
				return false;
			}
		}

		private List<ProcessListResponseItem> EmptyProcessList = new();

		public async Task<IList<ProcessListResponseItem>> GetProcessList()
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(GetProcessList));
				var reply = await _service.GetProcessListAsync(new ProcessListRequest());
				return reply.Results;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(GetProcessList));
				return EmptyProcessList;
			}
		}

		public async Task<bool> KillProcessById(int processId)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(KillProcessById));
				var reply = await _service.KillProcessByIdAsync(new KillProcessRequest(){ProcessId = processId});
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with id [{Id}]", nameof(KillProcessById), processId);
				return false;
			}
		}

		public async Task<bool> FocusProcessWindow(int processId)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(FocusProcessWindow));
				var reply = await _service.FocusWindowAsync(new FocusWindowRequest(){ ProcessId = processId });
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with id [{Id}]", nameof(FocusProcessWindow), processId);
				return false;
			}
		}

		public async Task<bool> LaunchProgram(string programName, string arguments = null)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(LaunchProgram));
				var request = string.IsNullOrEmpty(arguments)
					? new LaunchProgramRequest() { ProgramName = programName }
					: new LaunchProgramRequest() { ProgramName = programName, Arguments = arguments };

				var reply = await _service.LaunchProgramAsync(request);
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] [{ProgramName}] with arguments [{Arguments}]", nameof(LaunchProgram), programName, arguments);
				return false;
			}
		}

		public async Task<bool> SendMediaKey(SendMediaKeysRequest.Types.MediaKeyCode code)
		{
			try
			{
				_logger.LogInformation("Calling method {Method}", nameof(SendMediaKey));
				var reply = await _service.SendMediaKeysAsync(new SendMediaKeysRequest()
				{
					KeyCode = code
				});
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with code [{Code}]", nameof(SendMediaKey), code);
				return false;
			}
		}
	}
}