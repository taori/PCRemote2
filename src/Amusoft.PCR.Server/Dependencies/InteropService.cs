using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Google.Protobuf.Collections;
using GrpcDotNetNamedPipes;

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
		private readonly WindowsInteropService.WindowsInteropServiceClient _service;

		public InteropService(NamedPipeChannel channel)
		{
			_channel = channel;
			_service = new WindowsInteropService.WindowsInteropServiceClient(_channel);
		}

		public async Task<bool> ToggleMute()
		{
			var reply = await _service.ToggleMuteAsync(new ToggleMuteRequest());
			return reply.Muted;
		}

		public async Task MonitorOn()
		{
			await _service.MonitorOnAsync(new MonitorOnRequest());
		}

		public async Task MonitorOff()
		{
			await _service.MonitorOffAsync(new MonitorOffRequest());
		}

		public async Task LockWorkStation()
		{
			await _service.LockWorkStationAsync(new LockWorkStationRequest());
		}

		public async Task SendKeys(string keys)
		{
			await _service.SendKeysAsync(new SendKeysRequest(){Message = keys});
		}

		public async Task<int> SetMasterVolume(int value)
		{
			var reply = await _service.SetMasterVolumeAsync(new SetMasterVolumeRequest() {Value = value});
			return reply.Value;
		}

		public async Task<int> GetMasterVolume()
		{
			var reply = await _service.GetMasterVolumeAsync(new GetMasterVolumeRequest());
			return reply.Value;
		}

		public async Task<bool> Shutdown(TimeSpan delay, bool force)
		{
			var reply = await _service.ShutDownDelayedAsync(new ShutdownDelayedRequest() { Seconds = (int)delay.TotalSeconds, Force = force});
			return reply.Success;
		}

		public async Task<bool> AbortShutdown()
		{
			var reply = await _service.AbortShutDownAsync(new AbortShutdownRequest());
			return reply.Success;
		}

		public async Task<bool> Hibernate()
		{
			var reply = await _service.HibernateAsync(new HibernateRequest());
			return reply.Success;
		}

		public async Task<bool> Restart(TimeSpan delay, bool force)
		{
			var reply = await _service.RestartAsync(new RestartRequest(){ Delay = (int)delay.TotalSeconds, Force = force});
			return reply.Success;
		}

		public async Task<IList<ProcessListResponseItem>> GetProcessList()
		{
			var reply = await _service.GetProcessListAsync(new ProcessListRequest());
			return reply.Results;
		}

		public async Task<bool> KillProcessById(int processId)
		{
			var reply = await _service.KillProcessByIdAsync(new KillProcessRequest(){ProcessId = processId});
			return reply.Success;
		}

		public async Task<bool> FocusProcessWindow(int processId)
		{
			var reply = await _service.FocusWindowAsync(new FocusWindowRequest(){ ProcessId = processId });
			return reply.Success;
		}

		public async Task<bool> ExecuteCommandAsCurrentUser(string command)
		{
			var reply = await _service.ExecuteCommandAsCurrentUserAsync(new ExecuteCommandAsCurrentUserRequest(){ Command = command});
			return reply.Success;
		}
	}
}