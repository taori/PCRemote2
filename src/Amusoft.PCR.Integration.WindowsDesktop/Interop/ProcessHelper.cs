﻿using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Amusoft.PCR.Grpc.Common;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop.Interop
{
	public static class ProcessHelper
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(ProcessHelper));

		public static bool TryKillProcess(int processId)
		{
			// Logger.Info($"Killing process [{concrete.ProcessId}].");
			var process = Process.GetProcesses().FirstOrDefault(d => d.Id == processId);
			if (process == null)
			{
				Log.Warn($"Process id [{processId}] not found.");
				return false;
			}

			try
			{
				process.Kill();
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e);
				return false;
			}
		}

		public static bool TryGetProcessList(out List<ProcessListResponseItem> items)
		{
			items = new List<ProcessListResponseItem>();
			try
			{
				var processes = Process.GetProcesses();
				foreach (var process in processes)
				{
					items.Add(new ProcessListResponseItem()
					{
						ProcessId = process.Id,
						ProcessName = process.ProcessName,
						MainWindowTitle = process.MainWindowTitle
					});
				}
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				return false;
			}
		}

		public static bool TryLaunchProgram(string program, string arguments = null)
		{
			try
			{
				var process = new Process();
				process.StartInfo = arguments == null
					? new ProcessStartInfo(program)
					: new ProcessStartInfo(program, arguments);

				process.Start();
				return true;
			}
			catch (Exception e)
			{
				Log.Error(e, $"Exception occured while calling \"TryLaunchProgram\".");
				return false;
			}
		}
	}
}