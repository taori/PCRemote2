using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using Amusoft.PCR.Grpc.Common;
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
			base.OnStartup(e);

			_runOnceMutex = new Mutex(true, Globals.InteropMutexName, out var mutexNew);
			if (!mutexNew)
			{
				Shutdown();
			}

			if (!TryLaunchInteropChannel())
			{
				_namedPipeServer?.Dispose();
			}
		}

		protected override void OnExit(ExitEventArgs e)
		{
			_runOnceMutex.ReleaseMutex();
			_namedPipeServer.Kill();
			_namedPipeServer?.Dispose();
			base.OnExit(e);
		}

		private bool TryLaunchInteropChannel()
		{
			_namedPipeServer = new NamedPipeServer(Globals.NamedPipeChannel);
			WindowsInteropService.BindService(_namedPipeServer.ServiceBinder, new WindowsInteropServiceImplementation());

			try
			{
				Log.Info("Starting service.");
				_namedPipeServer.Start();

				Log.Info("Service running.");
				return true;
			}
			catch (Exception ex)
			{
				Log.Error(ex);
				return false;
			}
		}

		private void NamedPipeThreadWork()
		{
		}
	}
}
