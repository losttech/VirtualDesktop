using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;

namespace WindowsDesktop.Interop
{
	internal static class NativeMethods
	{
		[DllImport("user32.dll")]
		public static extern int GetWindowThreadProcessId(IntPtr hWnd, out int lpdwProcessId);

		[DllImport("user32.dll", CharSet = CharSet.Unicode)]
		public static extern uint RegisterWindowMessage(string lpProcName);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern bool CloseWindow(IntPtr hWnd);

		#region Hooks
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern IntPtr SetWinEventHook(WindowEvent hookMin, WindowEvent hookMax,
			IntPtr moduleHandle,
			WinEventProc callback, int processID, int threadID, HookFlags flags);

		[Flags]
		public enum HookFlags : int
		{
			OutOfContext = 0,
		}

		public enum WindowEvent
		{
			ForegroundChanged = 0x03,
			ObjectDestroy = 0x8001,
			NameChanged = 0x800C,
			Minimized = 0x0016,
			Unmiminized = 0x0017,
		}

		public enum ObjectCategory : uint
		{
			Window = 0x00000000,
			SysMenu = 0xFFFFFFFF,
			Titlebar = 0xFFFFFFFE,
			Menu = 0xFFFFFFFD,
			Client = 0xFFFFFFFC,
			VScroll = 0xFFFFFFFB,
			HScroll = 0xFFFFFFFA,
			SizeGrip = 0xFFFFFFF9,
			Caret = 0xFFFFFFF8,
			Cursor = 0xFFFFFFF7,
			Alert = 0xFFFFFFF6,
			Sound = 0xFFFFFFF5,
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool UnhookWinEvent(IntPtr hhk);

		public delegate void WinEventProc(IntPtr hookHandle, WindowEvent @event,
			IntPtr hwnd,
			ObjectCategory category, int child,
			int threadID, int timestampMs);
		#endregion
	}
}
