using System;
using System.Runtime.InteropServices;

namespace Amusoft.Toolkit.Impersonation
{
	[StructLayout(LayoutKind.Sequential)]
	internal struct ProcessInformation
	{
		public IntPtr ProcessHandle;
		public IntPtr ThreadHandle;
		public uint ProcessId;
		public uint ThreadId;
	}

	[StructLayout(LayoutKind.Sequential)]
	internal struct SecurityAttributes
	{
		public uint Length;
		public IntPtr SecurityDescriptor;
		public bool InheritHandle;
	}

	internal enum SecurityImpersonationLevel
	{
		SecurityAnonymous,
		SecurityIdentification,
		SecurityImpersonation,
		SecurityDelegation
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct StartupInfo
	{
		public uint cb;
		public string lpReserved;
		public string lpDesktop;
		public string lpTitle;
		public uint dwX;
		public uint dwY;
		public uint dwXSize;
		public uint dwYSize;
		public uint dwXCountChars;
		public uint dwYCountChars;
		public uint dwFillAttribute;
		public uint dwFlags;
		public short wShowWindow;
		public short cbReserved2;
		public IntPtr lpReserved2;
		public IntPtr hStdInput;
		public IntPtr hStdOutput;
		public IntPtr hStdError;
	}
	internal enum TokenType
	{
		TokenPrimary = 1,
		TokenImpersonation
	}
}