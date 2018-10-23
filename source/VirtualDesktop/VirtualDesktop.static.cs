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
		private static readonly ConcurrentDictionary<Guid, VirtualDesktop> _wrappers = new ConcurrentDictionary<Guid, VirtualDesktop>();

		/// <summary>
		/// Returns <c>true</c>, if the minimal stable functionality is supported
		/// </summary>
		public static bool HasMinimalSupport => ComObjects.VirtualDesktopManager != null;

		/// <summary>
		/// Gets a value indicating whether OS virtual desktop API is supported by this library.
		/// </summary>
		public static bool IsSupported =>
#if !DEBUG
			Environment.OSVersion.Version.Major >= 10 &&
#endif
			ComObjects.ApplicationViewCollection != null;

		/// <summary>
		/// Gets a value indicating whether virtual desktop API is present in the system.
		/// It might still not be supported. See <see cref="IsSupported"/>.
		/// </summary>
		public static bool IsPresent => ComObjects.VirtualDesktopManager != null;

		[EditorBrowsable(EditorBrowsableState.Never)]
		public static Exception InitializationException { get; }

		/// <summary>
		/// Gets the virtual desktop that is currently displayed.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public static VirtualDesktop Current
		{
			get
			{
				VirtualDesktopHelper.ThrowIfNotSupported();

				var current = ComObjects.VirtualDesktopManagerInternal.GetCurrentDesktop();
				var wrapper = _wrappers.GetOrAdd(current.GetID(), _ => new VirtualDesktop(current));

				return wrapper;
			}
		}

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
		/// Returns all the virtual desktops of currently valid.
		/// </summary>
		/// <returns></returns>
		[Obsolete(UnsupportedMessage)]
		public static VirtualDesktop[] GetDesktops()
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			return GetDesktopsInternal().ToArray();
		}

		[Obsolete(UnsupportedMessage)]
		internal static IEnumerable<VirtualDesktop> GetDesktopsInternal()
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			var desktops = ComObjects.VirtualDesktopManagerInternal.GetDesktops();
			var count = desktops.GetCount();

			for (var i = 0u; i < count; i++)
			{
				object ppvObject;
				desktops.GetAt(i, typeof(IVirtualDesktop).GUID, out ppvObject);

				var desktop = (IVirtualDesktop)ppvObject;
				var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));

				yield return wrapper;
			}
		}

		/// <summary>
		/// Creates a virtual desktop.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public static VirtualDesktop Create()
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			var desktop = ComObjects.VirtualDesktopManagerInternal.CreateDesktopW();
			var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));

			return wrapper;
		}

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete(UnsupportedMessage)]
		public static VirtualDesktop FromComObject(IVirtualDesktop desktop)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));
			return wrapper;
		}

		/// <summary>
		/// Returns the virtual desktop of the specified identifier.
		/// </summary>
		public static VirtualDesktop FromId(Guid desktopId)
		{
			if (desktopId == Guid.Empty)
				throw new ArgumentNullException(nameof(desktopId));

			if (HasMinimalSupport && !IsSupported) {
				var wrapper = _wrappers.GetOrAdd(desktopId, _ => new VirtualDesktop(desktopId));
				return wrapper;
			} else {
				VirtualDesktopHelper.ThrowIfNotSupported();

				IVirtualDesktop desktop;
				try {
					desktop = ComObjects.VirtualDesktopManagerInternal.FindDesktop(ref desktopId);
				} catch (COMException ex) when (ex.Match(HResult.TYPE_E_ELEMENTNOTFOUND)) {
					return null;
				}

				var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));

				return wrapper;
			}
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

		/// <summary>
		/// Returns the virtual desktop that the specified window is located.
		/// </summary>
		[Obsolete("Virtual Desktop object wrapper is unsupported. Use IDs when possible. E.g. IdFromHwnd")]
		public static VirtualDesktop FromHwnd(IntPtr hwnd)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			if (hwnd == IntPtr.Zero) return null;

			IVirtualDesktop desktop;
			try
			{
				var desktopId = ComObjects.VirtualDesktopManager.GetWindowDesktopId(hwnd);
				desktop = ComObjects.VirtualDesktopManagerInternal.FindDesktop(ref desktopId);
			}
			catch (COMException ex) when (ex.Match(HResult.REGDB_E_CLASSNOTREG, HResult.TYPE_E_ELEMENTNOTFOUND))
			{
				return null;
			}
			var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));

			return wrapper;
		}
	}
}
