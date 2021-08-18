using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Amusoft.PCR.Grpc.Common;
using Amusoft.PCR.Integration.WindowsDesktop.Managers;
using Amusoft.PCR.Integration.WindowsDesktop.Services;
using GrpcDotNetNamedPipes;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(App));

		private NamedPipeServer _namedPipeServer;
		private Mutex _runOnceMutex;

		protected override void OnStartup(StartupEventArgs e)
		{
			Log.Info("Launching Windows desktop integration");
			base.OnStartup(e);

			Log.Debug("Setting up event handler to check for parent process");
			ProcessExitListenerManager.ProcessExited += ProcessExitListenerManagerOnProcessExited;

#if !DEBUG
			_runOnceMutex = new Mutex(true, Globals.InteropMutexName, out var mutexNew);
			if (!mutexNew)
			{
				Shutdown();
			}
#endif

			VerifySimpleAudioManager();
			if (!TryLaunchInteropChannel())
			{
				Log.Fatal("Failed to launch named pipe for IPC with web application");
				_namedPipeServer?.Dispose();
			}
		}

		private void VerifySimpleAudioManager()
		{
			return;
			Task.Run(() =>
			{
				if (!SimpleAudioManager.TryGetAudioFeeds(out var feeds))
				{
					Log.Error("Failed to get audio feeds");
					return;
				}
				else
				{
					foreach (var feed in feeds)
					{
						Log.Debug($"Feed: {feed.Id} {feed.Volume}% name: {feed.Name}");
					}
				}

				Log.Debug("Muted: {Muted}", SimpleAudioManager.GetMasterVolumeMute());
				Thread.Sleep(1000);
				Log.Debug("Inverting Mute: {Muted}", SimpleAudioManager.SetMasterVolumeMute(!SimpleAudioManager.GetMasterVolumeMute()));
				Thread.Sleep(1000);
				Log.Debug("Reverting Mute: {Muted}", SimpleAudioManager.SetMasterVolumeMute(!SimpleAudioManager.GetMasterVolumeMute()));
				Thread.Sleep(1000);


				var startVolume = SimpleAudioManager.GetMasterVolume();
				Log.Debug("Current volume {Volume}", startVolume);
				Thread.Sleep(1000);
				SimpleAudioManager.SetMasterVolume(60);
				Thread.Sleep(1000);
				Log.Debug("Current volume {Volume}", SimpleAudioManager.GetMasterVolume());
				Thread.Sleep(1000);
				SimpleAudioManager.SetMasterVolume(startVolume);
			});
		}

		private void ProcessExitListenerManagerOnProcessExited(object sender, int e)
		{
			Log.Info("Parent process {Id} shut down - exiting program", e);
			Shutdown(0);
		}

		protected override void OnExit(ExitEventArgs e)
		{
			Log.Info("Windows desktop integration shutting down");
#if !DEBUG
			Log.Debug("Releasing mutex");
			_runOnceMutex.ReleaseMutex();
#endif

			Log.Debug("Shutting down named pipe server");
			_namedPipeServer.Kill();
			_namedPipeServer?.Dispose();
			base.OnExit(e);
		}

		private bool TryLaunchInteropChannel()
		{
			_namedPipeServer = new NamedPipeServer(Globals.NamedPipeChannel);
			DesktopIntegrationService.BindService(_namedPipeServer.ServiceBinder, new WindowsDesktopIntegrationService());

			try
			{
				Log.Info("Starting IPC");
				_namedPipeServer.Start();

				Log.Info("IPC running");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}
	}
}
