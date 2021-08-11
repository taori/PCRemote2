using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.ServiceProcess;
using System.Text;
using System.Windows.Forms;

namespace Amusoft.PCR.Installer.Custom
{
	public class CustomActions
	{
		[CustomAction]
		public static ActionResult UpdateAppSettings(Session session)
		{
			SetBreakpoint(nameof(UpdateAppSettings));

			session.Log("UpdateAppSettings Enter");

			session.Log("Accessing Port");
			if (!session.CustomActionData.TryGetValue("Port", out var port))
				return ActionResult.Failure;

			session.Log("Parsing int value");
			if (!int.TryParse(port, out var parsedPort))
				return ActionResult.Failure;

			if (!session.CustomActionData.TryGetValue("AppSettingsPath", out var appsettingsPath))
				return ActionResult.Failure;

			try
			{
				var content = File.ReadAllText(appsettingsPath, Encoding.UTF8);
				File.WriteAllText(appsettingsPath, content.Replace("https://*:5001", $"https://*:{port}"));
			}
			catch (Exception e)
			{
				session.Log(e.ToString());
				return ActionResult.Failure;
			}
			
			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult StopService(Session session)
		{
			SetBreakpoint(nameof(StopService));

			// required to shut down system processes
			if (!session.CustomActionData.TryGetValue("ServiceName", out var serviceName))
				return ActionResult.Failure;

			try
			{
				var controller = new ServiceController(serviceName);
				controller.Stop();
				controller.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(30));

				return ActionResult.Success;
			}
			catch (Exception e)
			{
				session.Log($"Service failed to start: {e.ToString()}");
				return ActionResult.Failure;
			}
		}

		private static bool InvokeServiceCommandUtil(Session session, string command)
		{
			try
			{
				var process = new Process();
				process.StartInfo = new ProcessStartInfo("sc.exe", command);
				process.StartInfo.Verb = "runas";
				process.StartInfo.UseShellExecute = true;
				process.Start();
				return process.WaitForExit((int)TimeSpan.FromSeconds(60).TotalMilliseconds);
			}
			catch (Exception e)
			{
				session.Log(e.ToString());
				return false;
			}
		}

		[CustomAction]
		public static ActionResult UninstallService(Session session)
		{
			SetBreakpoint(nameof(UninstallService));

			// required to shut down system processes
			if (!session.CustomActionData.TryGetValue("ServiceName", out var serviceName))
				return ActionResult.Failure;

			try
			{
				return InvokeServiceCommandUtil(session, $"delete \"{serviceName}\"")
					? ActionResult.Success
					: ActionResult.Failure;
			}
			catch (Exception e)
			{
				session.Log($"Service failed to start: {e.ToString()}");
				return ActionResult.Failure;
			}
		}

		[CustomAction]
		public static ActionResult InstallService(Session session)
		{
			SetBreakpoint(nameof(InstallService));

			if (!session.CustomActionData.TryGetValue("DisplayName", out var displayName))
				return ActionResult.Failure;
			if (!session.CustomActionData.TryGetValue("ServiceName", out var serviceName))
				return ActionResult.Failure;
			if (!session.CustomActionData.TryGetValue("ServiceDescription", out var description))
				return ActionResult.Failure;
			if (!session.CustomActionData.TryGetValue("ResetConfiguration", out var resetConfiguration))
				return ActionResult.Failure;
			if (!session.CustomActionData.TryGetValue("ServiceExePath", out var exePath))
				return ActionResult.Failure;

			if (!InvokeServiceCommandUtil(session, $"create \"{serviceName}\" binPath= \"{exePath}\" DisplayName= \"{displayName}\" start= auto"))
			{
				session.Log("Creation failed.");
				return ActionResult.Failure;
			}

			if (!InvokeServiceCommandUtil(session, $"description \"{serviceName}\" \"{description}\""))
			{
				session.Log("Setting description failed.");
				return ActionResult.Failure;
			}

			var fullFailureReset = TimeSpan.FromHours(24).TotalSeconds;
			if (!InvokeServiceCommandUtil(session, $"failure \"{serviceName}\" reset= {fullFailureReset} actions= {resetConfiguration}"))
			{
				session.Log("Setting failure configuration failed.");
				return ActionResult.Failure;
			}

			return ActionResult.Success;
		}

		[CustomAction]
		public static ActionResult StartService(Session session)
		{
			SetBreakpoint(nameof(StartService));

			// required to shut down system processes
			if (!session.CustomActionData.TryGetValue("ServiceName", out var serviceName))
				return ActionResult.Failure;

			try
			{
				if (InvokeServiceCommandUtil(session, $"start \"{serviceName}\""))
				{
					var controller = new ServiceController(serviceName);
					controller.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(30));
				}
				
				return ActionResult.Success;
			}
			catch (Exception e)
			{
				session.Log($"Service failed to start: {e.ToString()}");
				return ActionResult.Failure;
			}
		}

		[Conditional("DEBUG")]
		private static void SetBreakpoint(string name)
		{
			var test = MessageBox.Show($"Waiting for breakpoint ID: {Process.GetCurrentProcess().Id} in {name}");
		}
	}
}
