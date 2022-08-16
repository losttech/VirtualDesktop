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
		private static readonly ConcurrentDictionary<Guid, VirtualDesktop> _wrappers = new();

		/// <summary>
		/// Returns <c>true</c>, if the minimal stable functionality is supported
		/// </summary>
		public static bool HasMinimalSupport => ComObjects.VirtualDesktopManager != null;

		/// <summary>
		/// Gets a value indicating whether virtual desktop API is present in the system.
		/// It might still not be supported. See <see cref="IsSupported"/>.
		/// </summary>
		public static bool IsPresent => ComObjects.VirtualDesktopManager is not null;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Exception InitializationException { get; }

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

		/// <summary>
		/// Returns ID of the virtual desktop, where specified window is located.
		/// </summary>
		public static Guid? IdFromHwnd(IntPtr hwnd) {
			VirtualDesktopHelper.ThrowIfNoMinimalSupport();

			if (hwnd == IntPtr.Zero) return null;

			try {
				return ComObjects.VirtualDesktopManager.GetWindowDesktopId(hwnd);
			} catch (COMException ex) when (ex.Match(HResult.REGDB_E_CLASSNOTREG, HResult.TYPE_E_ELEMENTNOTFOUND)) {
				return null;
			}
		}
	}
}
