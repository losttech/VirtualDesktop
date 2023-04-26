using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	partial class VirtualDesktop
	{
		/// <summary>
		/// Gets a value indicating whether virtual desktop API is present in the system.
		/// </summary>
		public static bool IsPresent => ComObjects.VirtualDesktopManager is not null;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Exception? InitializationException { get; }

		static VirtualDesktop()
		{
			try
			{
				ComObjects.Initialize();
			}
			catch (Exception ex)
			{
				InitializationException = ex;
			}

			try
			{
				ComObjects.RegisterListener();
			} catch { }

			AppDomain.CurrentDomain.ProcessExit += (sender, args) => ComObjects.Terminate();
		}

		static VirtualDesktop? FromId(Guid? id) => id is { } v ? new (v) : null;
		public static VirtualDesktop? FromHwnd(IntPtr hwnd) => FromId(IdFromHwnd(hwnd));

		/// <summary>
		/// Returns ID of the virtual desktop, where specified window is located.
		/// </summary>
		public static Guid? IdFromHwnd(IntPtr hwnd) {
			if (hwnd == IntPtr.Zero) return null;

			var result = VirtualDesktopHelper.GetManagerOrThrow().GetWindowDesktopId(hwnd, out var desktopID);
			if (result.Succeeded)
				return desktopID;

			if (result.Match(HResult.REGDB_E_CLASSNOTREG, HResult.TYPE_E_ELEMENTNOTFOUND, HResult.INVALID_STATE))
				return null;

			throw result.GetException();
		}

		/// <summary>
		/// Gets ID of the virtual desktop, where specified window is located.
		/// </summary>
		public static PInvoke.HResult TryGetIdFromHwnd(IntPtr hwnd, out Guid id)
			=> VirtualDesktopHelper.GetManagerOrThrow().GetWindowDesktopId(hwnd, out id);

		public static bool? IsWindowOnCurrentVirtualDesktop(IntPtr hwnd)
		{
			if (hwnd == IntPtr.Zero) return false;

			try
			{
				return VirtualDesktopHelper.GetManagerOrThrow().IsWindowOnCurrentVirtualDesktop(hwnd);
			}
			catch (COMException ex) when (ex.Match(HResult.REGDB_E_CLASSNOTREG, HResult.TYPE_E_ELEMENTNOTFOUND, HResult.INVALID_STATE))
			{
				return null;
			}
		}
	}
}
