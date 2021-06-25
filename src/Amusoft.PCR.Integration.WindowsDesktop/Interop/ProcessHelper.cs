using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using Amusoft.PCR.Grpc.Common;

namespace Amusoft.PCR.Integration.WindowsDesktop.Interop
{
	public static class ProcessHelper
	{
		public static bool TryKillProcess(int processId)
		{
			// Logger.Info($"Killing process [{concrete.ProcessId}].");
			var process = Process.GetProcesses().FirstOrDefault(d => d.Id == processId);
			if (process == null)
			{
				// Logger.Warn($"Process id [{concrete.ProcessId}] not found.");
				return false;
			}

			try
			{
				process.Kill();
				return true;
			}
			catch (Exception e)
			{
				Debug.WriteLine(e);
				return false;
				// Logger.Error(e);
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
	}
}