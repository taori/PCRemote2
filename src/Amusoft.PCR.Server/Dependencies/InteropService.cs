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
		Task<bool> ExecuteCommandAsCurrentUser(string command);
	}

	public class InteropService : IInteropService
	{
		private readonly NamedPipeChannel _channel;
		private readonly ILogger<InteropService> _logger;
		private readonly WindowsInteropService.WindowsInteropServiceClient _service;

		public InteropService(NamedPipeChannel channel, ILogger<InteropService> logger)
		{
			_channel = channel;
			_logger = logger;
			_service = new WindowsInteropService.WindowsInteropServiceClient(_channel);
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
				_logger.LogError(e, "Exception occured while calling \"ToggleMute\".");
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
				_logger.LogError(e, "Exception occured while calling \"MonitorOn\".");
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
				_logger.LogError(e, "Exception occured while calling \"MonitorOff\".");
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
				_logger.LogError(e, "Exception occured while calling \"LockWorkStation\".");
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
				_logger.LogError(e, "Exception occured while calling \"SendKeys\".");
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
				_logger.LogError(e, "Exception occured while calling \"SetMasterVolume\".");
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
				_logger.LogError(e, $"Exception occured while calling \"GetMasterVolume\".");
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
				_logger.LogError(e, $"Exception occured while calling \"Shutdown\".");
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
				_logger.LogError(e, $"Exception occured while calling \"AbortShutdown\".");
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
				_logger.LogError(e, $"Exception occured while calling \"Hibernate\".");
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
				_logger.LogError(e, $"Exception occured while calling \"Restart\".");
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
				_logger.LogError(e, $"Exception occured while calling \"GetProcessList\".");
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
				_logger.LogError(e, $"Exception occured while calling \"KillProcessById\".");
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
				_logger.LogError(e, $"Exception occured while calling \"FocusProcessWindow\".");
				return false;
			}
		}

		public async Task<bool> ExecuteCommandAsCurrentUser(string command)
		{
			try
			{
				var reply = await _service.ExecuteCommandAsCurrentUserAsync(new ExecuteCommandAsCurrentUserRequest(){ Command = command});
				return reply.Success;
			}
			catch (Exception e)
			{
				_logger.LogError(e, $"Exception occured while calling \"ExecuteCommandAsCurrentUser\".");
				return false;
			}
		}
	}
}