using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace Amusoft.PCR.Integration.WindowsDesktop.Interop
{
	internal class NativeMethods
	{
		[DllImport("user32.dll")]
		static extern bool SetForegroundWindow(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern void LockWorkStation();

		/// <summary>
		/// See https://docs.microsoft.com/en-us/dotnet/api/system.windows.forms.sendkeys?redirectedfrom=MSDN&view=net-5.0
		/// </summary>
		/// <param name="keys"></param>
		public static void SendKeys(string keys)
		{
			System.Windows.Forms.SendKeys.SendWait(keys);
		}
		
		private static unsafe IntPtr GetProcessPointer()
		{
			var p = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle;
			var intPtr = new IntPtr(p.ToPointer());
			return intPtr;
		}

		public static bool SetForegroundWindow(int processId)
		{
			var matchProcess = Process.GetProcesses().FirstOrDefault(d => d.Id == processId);
			if (matchProcess != null)
			{
				return SetForegroundWindow(matchProcess.MainWindowHandle);
			}

			return false;
		}

		public static class Monitor
		{
			[DllImport("user32.dll")]
			static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
			[DllImport("user32.dll")]
			static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);

			private const int WmSyscommand = 0x0112;
			private const int ScMonitorpower = 0xF170;
			private const int MonitorShutoff = 2;
			private const int MouseeventfMove = 0x0001;

			public static void Off()
			{
				SendMessage(GetProcessPointer(), WmSyscommand, (IntPtr)ScMonitorpower, (IntPtr)MonitorShutoff);
			}

			public static void On()
			{
				mouse_event(MouseeventfMove, 0, 1, 0, UIntPtr.Zero);
				Thread.Sleep(40);
				mouse_event(MouseeventfMove, 0, -1, 0, UIntPtr.Zero);
			}
		}
	}
}
