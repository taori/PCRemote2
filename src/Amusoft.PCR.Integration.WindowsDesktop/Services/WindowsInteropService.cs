using System;
using System.Threading.Tasks;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Integration.WindowsDesktop.Helpers;
using Amusoft.PCR.Integration.WindowsDesktop.Interop;
using Amusoft.Toolkit.Impersonation;
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
			Log.Info("Executing [{Name}] [{Delay}s] [{Force}]", nameof(ShutDownDelayed), request.Seconds, request.Force);
			var success = MachineStateHelper.TryShutDownDelayed(TimeSpan.FromSeconds(request.Seconds), request.Force);
			return Task.FromResult(new ShutdownDelayedReply() { Success = success });
		}

		public override Task<RestartReply> Restart(RestartRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}] [{Delay}s] [{Force}]", nameof(Restart), request.Delay, request.Force);
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
				Log.Error(e, "ToggleMute failed.");
				AudioManager.SetMasterVolumeMute(!muteState);
				return Task.FromResult(new ToggleMuteReply() { Muted = muteState });
			}
		}

		public override Task<SetMasterVolumeReply> SetMasterVolume(SetMasterVolumeRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}] [{NewValue}]", nameof(SetMasterVolume), request.Value);
			var previousVolume = AudioManager.GetMasterVolume();
			var newVolume = Math.Max(Math.Min(100, request.Value), 0);
			if (MathHelper.IsEqual(newVolume, previousVolume, 1.01f))
			{
				return Task.FromResult(new SetMasterVolumeReply() { Value = (int)previousVolume });
			}
			else
			{
				Log.Debug("Changing volume from [{From}] to [{To}]", previousVolume, newVolume);
				AudioManager.SetMasterVolume(newVolume);
				var masterVolume = AudioManager.GetMasterVolume();
				return Task.FromResult(new SetMasterVolumeReply() { Value = (int)masterVolume });
			}
		}

		public override Task<GetMasterVolumeReply> GetMasterVolume(GetMasterVolumeRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}]", nameof(GetMasterVolume));
			return Task.FromResult(new GetMasterVolumeReply() { Value = (int)AudioManager.GetMasterVolume() });
		}

		public override Task<SendKeysReply> SendKeys(SendKeysRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}] {Message}", nameof(SendKeys), request.Message);
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
			Log.Info("Executing [{Name}] [{ProcessId}]", nameof(FocusWindow), request.ProcessId);
			var result = NativeMethods.SetForegroundWindow(request.ProcessId);
			return Task.FromResult(new FocusWindowResponse() {Success = result});
		}

		public override Task<KillProcessResponse> KillProcessById(KillProcessRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}] [{ProcessId}]", nameof(KillProcessById), request.ProcessId);
			var result = ProcessHelper.TryKillProcess(request.ProcessId);
			return Task.FromResult(new KillProcessResponse() {Success = result});
		}

		public override Task<LaunchProgramResponse> LaunchProgram(LaunchProgramRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}] [{Program}] [{Arguments}]", nameof(LaunchProgram), request.ProgramName, request.Arguments);
			// integration exe is already executed in user context, therefore no further impersonation is required.
			var result = ProcessHelper.TryLaunchProgram(request.ProgramName, request.Arguments);

			return Task.FromResult(new LaunchProgramResponse() {Success = result});
		}

		public override Task<SendMediaKeysReply> SendMediaKeys(SendMediaKeysRequest request, ServerCallContext context)
		{
			Log.Info("Executing [{Name}] [{MediaKey}]", nameof(SendMediaKeys), request.KeyCode);

			switch (request.KeyCode)
			{
				case SendMediaKeysRequest.Types.MediaKeyCode.NextTrack:
					NativeMethods.PressMediaKey(NativeMethods.MediaKeyCode.NextTrack);
					break;
				case SendMediaKeysRequest.Types.MediaKeyCode.PreviousTrack:
					NativeMethods.PressMediaKey(NativeMethods.MediaKeyCode.PreviousTrack);
					break;
				case SendMediaKeysRequest.Types.MediaKeyCode.PlayPause:
					NativeMethods.PressMediaKey(NativeMethods.MediaKeyCode.PlayPause);
					break;
				default:
					throw new ArgumentOutOfRangeException();
			}

			return Task.FromResult(new SendMediaKeysReply());
		}
	}
}