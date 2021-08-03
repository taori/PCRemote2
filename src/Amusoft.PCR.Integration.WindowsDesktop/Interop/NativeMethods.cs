using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using NLog;

namespace Amusoft.PCR.Integration.WindowsDesktop.Interop
{
	internal class NativeMethods
	{
		private static readonly Logger Log = LogManager.GetLogger(nameof(NativeMethods));

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
			try
			{
				var matchProcess = Process.GetProcessById(processId);
				return SetForegroundWindow(matchProcess.MainWindowHandle);
			}
			catch (Exception e)
			{
				Log.Error(e, "An error occured while calling {Method}", nameof(SetForegroundWindow));
				return false;
			}
		}

		public enum MediaKeyCode : byte
		{
			NextTrack = 0xb0,
			PreviousTrack = 0xb3,
			PlayPause = 0xb1
		}

		[DllImport("user32.dll", SetLastError = true)]
		public static extern void keybd_event(byte virtualKey, byte scanCode, uint flags, IntPtr extraInfo);
		public const int KEYEVENTF_EXTENDEDKEY = 0x0001; //Key down flag
		public const int KEYEVENTF_KEYUP = 0x0002; //Key up flag

		public static void PressMediaKey(MediaKeyCode code)
		{
			keybd_event((byte)code, 0, KEYEVENTF_EXTENDEDKEY, IntPtr.Zero);
			keybd_event((byte)code, 0, KEYEVENTF_KEYUP, IntPtr.Zero);
		}

		public static class Monitor
		{
			[DllImport("user32.dll")]
			static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
			[DllImport("user32.dll")]
			static extern void mouse_event(Int32 dwFlags, Int32 dx, Int32 dy, Int32 dwData, UIntPtr dwExtraInfo);

			private const int WmSyscommand = 0x0112;
			private const int ScMonitorpower = 0xF170;
			private const int MonitorShutoff = 0x0002;
			private const int MouseEventfMove = 0x0001;
			private const int Broadcast = 0xffff;

			public static void Off()
			{
				SendMessage((IntPtr)Broadcast, WmSyscommand, (IntPtr)ScMonitorpower, (IntPtr)MonitorShutoff);
			}

			public static void On()
			{
				mouse_event(MouseEventfMove, 0, 1, 0, UIntPtr.Zero);
				Thread.Sleep(40);
				mouse_event(MouseEventfMove, 0, -1, 0, UIntPtr.Zero);
			}
		}
	}
}
