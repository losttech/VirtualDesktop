using System;
using System.Diagnostics;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	public static class VirtualDesktopHelper
	{
		internal static IVirtualDesktopManager GetManagerOrThrow()
		{
			if (!VirtualDesktop.IsPresent)
				throw new NotSupportedException("Unsupported OS version, or need to include the app manifest in your project so as to target Windows 10. And, run without debugging.");

			return ComObjects.VirtualDesktopManager!;
		}


		public static bool IsCurrentVirtualDesktop(IntPtr handle)
		{
			return GetManagerOrThrow().IsWindowOnCurrentVirtualDesktop(handle);
		}

		public static void MoveToDesktop(IntPtr hWnd, VirtualDesktop virtualDesktop)
		{
			var manager = GetManagerOrThrow();

			int processId;
			NativeMethods.GetWindowThreadProcessId(hWnd, out processId);

			if (Process.GetCurrentProcess().Id == processId)
			{
				var guid = virtualDesktop.Id;
				manager.MoveWindowToDesktop(hWnd, ref guid);
			}

			throw new NotSupportedException("You can only move own windows.");
		}
	}
}
