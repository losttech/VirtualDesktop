using System;
using System.Diagnostics;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	public static class VirtualDesktopHelper
	{
		internal static void ThrowIfNotSupported()
		{
			if (!VirtualDesktop.IsSupported)
			{
				throw new NotSupportedException("Need to include the app manifest in your project so as to target Windows 10. And, run without debugging.");
			}
		}

		internal static void ThrowIfNoMinimalSupport()
		{
			if (!VirtualDesktop.HasMinimalSupport)
				throw new NotSupportedException("Unsupported OS version, or need to include the app manifest in your project so as to target Windows 10. And, run without debugging.");
		}


		public static bool IsCurrentVirtualDesktop(IntPtr handle)
		{
			ThrowIfNoMinimalSupport();

			return ComObjects.VirtualDesktopManager.IsWindowOnCurrentVirtualDesktop(handle);
		}

		public static void MoveToDesktop(IntPtr hWnd, VirtualDesktop virtualDesktop)
		{
			ThrowIfNoMinimalSupport();

			int processId;
			NativeMethods.GetWindowThreadProcessId(hWnd, out processId);

			if (Process.GetCurrentProcess().Id == processId)
			{
				var guid = virtualDesktop.Id;
				ComObjects.VirtualDesktopManager.MoveWindowToDesktop(hWnd, ref guid);
			}
			else
			{
				if (!VirtualDesktop.IsSupported)
					throw new NotSupportedException("This version of OS has only minimal support. You can only move own windows with it.");

				IApplicationView view;
				ComObjects.ApplicationViewCollection.GetViewForHwnd(hWnd, out view);
				ComObjects.VirtualDesktopManagerInternal.MoveViewToDesktop(view, virtualDesktop.ComObject);
			}
		}
	}
}
