using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Integration.WindowsDesktop.Interop;
using Amusoft.PCR.Integration.WindowsDesktop.Interop.Impersonation;
using Google.Protobuf.Collections;
using Grpc.Core;
using NLog;
using NativeMethods = Amusoft.PCR.Integration.WindowsDesktop.Interop.NativeMethods;

namespace Amusoft.PCR.Integration.WindowsDesktop.Services
{
	public class WindowsInteropServiceImplementation : WindowsInteropService.WindowsInteropServiceBase
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(WindowsInteropServiceImplementation));

		public override Task<HibernateReply> Hibernate(HibernateRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(Hibernate));
			var success = MachineStateHelper.TryHibernate();
			return Task.FromResult(new HibernateReply(){ Success = success });
		}

		public override Task<AbortShutdownReply> AbortShutDown(AbortShutdownRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(AbortShutDown));
			var success = MachineStateHelper.TryAbortShutDown();
			return Task.FromResult(new AbortShutdownReply() { Success = success });
		}

		public override Task<ShutdownDelayedReply> ShutDownDelayed(ShutdownDelayedRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(ShutDownDelayed));
			var success = MachineStateHelper.TryShutDownDelayed(TimeSpan.FromSeconds(request.Seconds), request.Force);
			return Task.FromResult(new ShutdownDelayedReply() { Success = success });
		}

		public override Task<RestartReply> Restart(RestartRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(Restart));
			var success = MachineStateHelper.TryRestart(TimeSpan.FromSeconds(request.Delay), request.Force);
			return Task.FromResult(new RestartReply() { Success = success });
		}

		public override Task<ToggleMuteReply> ToggleMute(ToggleMuteRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(ToggleMute));
			var muteState = AudioManager.GetMasterVolumeMute();
			try
			{
				AudioManager.SetMasterVolumeMute(!muteState);
				return Task.FromResult(new ToggleMuteReply() { Muted = !muteState });
			}
			catch (Exception e)
			{
				AudioManager.SetMasterVolumeMute(!muteState);
				return Task.FromResult(new ToggleMuteReply() { Muted = muteState });
			}
		}

		public override Task<SetMasterVolumeReply> SetMasterVolume(SetMasterVolumeRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(SetMasterVolume));
			AudioManager.SetMasterVolume(Math.Max(Math.Min(100, request.Value), 0));
			var masterVolume = AudioManager.GetMasterVolume();
			return Task.FromResult(new SetMasterVolumeReply() { Value = (int)masterVolume });
		}

		public override Task<GetMasterVolumeReply> GetMasterVolume(GetMasterVolumeRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(GetMasterVolume));
			return Task.FromResult(new GetMasterVolumeReply() { Value = (int)AudioManager.GetMasterVolume() });
		}

		public override Task<SendKeysReply> SendKeys(SendKeysRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(SendKeys));
			NativeMethods.SendKeys(request.Message);
			return Task.FromResult(new SendKeysReply());
		}

		public override Task<MonitorOnReply> MonitorOn(MonitorOnRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(MonitorOn));
			NativeMethods.Monitor.On();
			return Task.FromResult(new MonitorOnReply());
		}

		public override Task<MonitorOffReply> MonitorOff(MonitorOffRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(MonitorOff));
			NativeMethods.Monitor.Off();
			return Task.FromResult(new MonitorOffReply());
		}

		public override Task<LockWorkStationReply> LockWorkStation(LockWorkStationRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(LockWorkStation));
			NativeMethods.LockWorkStation();
			return Task.FromResult(new LockWorkStationReply());
		}

		public override Task<ProcessListResponse> GetProcessList(ProcessListRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(GetProcessList));
			var result = ProcessHelper.TryGetProcessList(out var processList);
			var response = new ProcessListResponse();
			if (result)
				response.Results.AddRange(processList);

			return Task.FromResult(response);
		}

		public override Task<FocusWindowResponse> FocusWindow(FocusWindowRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(FocusWindow));
			var result = NativeMethods.SetForegroundWindow(request.ProcessId);
			return Task.FromResult(new FocusWindowResponse() {Success = result});
		}

		public override Task<KillProcessResponse> KillProcessById(KillProcessRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(KillProcessById));
			var result = ProcessHelper.TryKillProcess(request.ProcessId);
			return Task.FromResult(new KillProcessResponse() {Success = result});
		}

		public override Task<ExecuteCommandAsCurrentUserResponse> ExecuteCommandAsCurrentUser(ExecuteCommandAsCurrentUserRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(ExecuteCommandAsCurrentUser));
			var result = ProcessImpersonation.Launch(request.Command);
			return Task.FromResult(new ExecuteCommandAsCurrentUserResponse() { Success = result });
		}
	}
}