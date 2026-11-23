using System;
using Windows.Win32;
using Windows.Win32.Foundation;

namespace WindowsDesktop;

public static class VirtualDesktopHelper
{
	/// <summary>Returns true when the shell confirms the window is on the current desktop.</summary>
	public static bool IsCurrentVirtualDesktop(nint handle) => VirtualDesktop.IsWindowOnCurrentVirtualDesktop(handle) == true;

	public static void MoveToDesktop(nint hWnd, VirtualDesktop virtualDesktop)
	{
		var manager = VirtualDesktop.CreateManager();
		var window = (HWND)hWnd;
		if (PInvoke.GetWindowThreadProcessId(window, out uint processId) == 0)
			throw new ArgumentException("The handle does not identify a window.", nameof(hWnd));
		if (processId != PInvoke.GetCurrentProcessId())
			throw new NotSupportedException("You can only move your own windows.");

		manager.MoveWindowToDesktop(window, virtualDesktop.Id);
	}
}
