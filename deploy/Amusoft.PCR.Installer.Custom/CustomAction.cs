using Microsoft.Deployment.WindowsInstaller;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
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

		public static ActionResult VerifyConfiguration(Session session)
		{
			SetBreakpoint(nameof(VerifyConfiguration));

			if (session["ConfigurationIsValid"] is var value && int.TryParse(value, out var parsedValue))
				return ActionResult.Success;

			return ActionResult.Failure;
		}

		[Conditional("DEBUG")]
		private static void SetBreakpoint(string name)
		{
			var test = MessageBox.Show($"Waiting for breakpoint ID: {Process.GetCurrentProcess().Id} in {name}");
			// if (!Debugger.IsAttached)
			// 	Debugger.Launch();
		}
	}
}
