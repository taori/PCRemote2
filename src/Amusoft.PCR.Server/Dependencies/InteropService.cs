using System;
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
		Task MonitorOn();
		Task MonitorOff();
		Task LockWorkStation();

		/// <summary>
		/// See https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?redirectedfrom=MSDN&view=net-5.0
		/// </summary>
		/// <param name="keys"></param>
		Task SendKeys(string keys);

		Task<int> SetMasterVolume(int value);
		Task<int> GetMasterVolume();
		Task<bool> Shutdown(TimeSpan delay, bool force);
		Task<bool> AbortShutdown();
		Task<bool> Hibernate();
		Task<bool> Restart(TimeSpan delay, bool force);
		Task<IList<ProcessListResponseItem>> GetProcessList();
		Task<bool> KillProcessById(int processId);
		Task<bool> FocusProcessWindow(int processId);
		Task<bool> StartImpersonatedProcess(string programName, int impersonatedProcessId = 0);
		Task<bool> SendMediaKey(SendMediaKeysRequest.Types.MediaKeyCode code);
	}

	public class InteropService : IInteropService
	{
		private readonly ILogger<InteropService> _logger;
		private readonly WindowsInteropService.WindowsInteropServiceClient _service;

		public InteropService(NamedPipeChannel channel, ILogger<InteropService> logger)
		{
			_logger = logger;
			_service = new WindowsInteropService.WindowsInteropServiceClient(channel);
		}

		public async Task<bool> ToggleMute()
		{
			try
			{
				var reply = await _service.ToggleMuteAsync(new ToggleMuteRequest());
				return reply.Muted;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(ToggleMute));
				return false;
			}
		}

		public async Task MonitorOn()
		{
			try
			{
				await _service.MonitorOnAsync(new MonitorOnRequest());
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(MonitorOn));
			}
		}

		public async Task MonitorOff()
		{
			try
			{
				await _service.MonitorOffAsync(new MonitorOffRequest());
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(MonitorOff));
			}
		}

		public async Task LockWorkStation()
		{
			try
			{
				await _service.LockWorkStationAsync(new LockWorkStationRequest());
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}]", nameof(LockWorkStation));
			}
		}

		public async Task SendKeys(string keys)
		{
			try
			{
				await _service.SendKeysAsync(new SendKeysRequest(){Message = keys});
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with [{Keys}]", nameof(SendKeys), keys);
			}
		}

		public async Task<int> SetMasterVolume(int value)
		{
			try
			{
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
				var reply = await _service.FocusWindowAsync(new FocusWindowRequest(){ ProcessId = processId });
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] with id [{Id}]", nameof(FocusProcessWindow), processId);
				return false;
			}
		}

		public async Task<bool> StartImpersonatedProcess(string programName, int impersonatedProcessId = 0)
		{
			try
			{
				var reply = await _service.StartImpersonatedProcessAsync(new StartImpersonatedProcessRequest(){ ProgramName = programName, ImpersonatedProcessId = impersonatedProcessId });
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, "Exception occured while calling [{Name}] [{ProgramName}] as user similar to process [{ProcessId}]", nameof(StartImpersonatedProcess), programName, impersonatedProcessId);
				return false;
			}
		}

		public async Task<bool> SendMediaKey(SendMediaKeysRequest.Types.MediaKeyCode code)
		{
			try
			{
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