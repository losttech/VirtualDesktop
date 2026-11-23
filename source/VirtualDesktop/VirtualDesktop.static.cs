using System;
using System.ComponentModel;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.System.Rpc;
using Windows.Win32.UI.Shell;
using WindowsDesktop.Internal;

namespace WindowsDesktop;

partial class VirtualDesktop
{
	private static volatile Exception? activationError;
	private static volatile Exception? listenerError;

	/// <summary>Gets whether the shell's virtual desktop manager is available.</summary>
	public static bool IsPresent => TryCreateManager(out _) is not null;

	[EditorBrowsable(EditorBrowsableState.Never)]
	public static Exception? InitializationException => activationError ?? listenerError;

	static VirtualDesktop()
	{
		try
		{
			ObserveCurrentDesktop();
			DesktopListener.Start(ObserveForegroundDesktop, ObserveCurrentDesktop, ex => listenerError = ex);
		}
		catch (Exception ex)
		{
			listenerError = ex;
		}
	}

	internal static IVirtualDesktopManager CreateManager()
	{
		try
		{
			var manager = (IVirtualDesktopManager)new VirtualDesktopManager();
			activationError = null;
			return manager;
		}
		catch (Exception ex)
		{
			activationError = ex;
			if ((HRESULT)ex.HResult == HRESULT.REGDB_E_CLASSNOTREG)
				throw new NotSupportedException("The shell's virtual desktop manager is not registered.", ex);
			throw;
		}
	}

	private static IVirtualDesktopManager? TryCreateManager(out Exception? error)
	{
		error = null;
		try { return CreateManager(); }
		catch (Exception ex)
		{
			error = ex;
			return null;
		}
	}

	public static VirtualDesktop? FromHwnd(nint hwnd) =>
		IdFromHwnd(hwnd) is { } id ? new VirtualDesktop(id) : null;

	/// <summary>Returns the desktop ID for a window, or null if the shell cannot locate its desktop.</summary>
	/// <exception cref="NotSupportedException">The virtual desktop manager is not registered.</exception>
	/// <remarks>Other shell failures propagate as their corresponding COM interop exceptions.</remarks>
	public static Guid? IdFromHwnd(nint hwnd)
	{
		if (hwnd == 0) return null;

		try
		{
			return GetDesktopId(CreateManager(), (HWND)hwnd);
		}
		catch (Exception ex) when (IsDesktopUnavailable(ex))
		{
			return null;
		}
	}

	/// <summary>Returns whether the window is on the current desktop, or null if the shell cannot determine that.</summary>
	/// <exception cref="NotSupportedException">The virtual desktop manager is not registered.</exception>
	/// <remarks>Other shell failures propagate as their corresponding COM interop exceptions.</remarks>
	public static bool? IsWindowOnCurrentVirtualDesktop(nint hwnd)
	{
		if (hwnd == 0) return false;

		try
		{
			return IsCurrent(CreateManager(), (HWND)hwnd);
		}
		catch (Exception ex) when (IsDesktopUnavailable(ex))
		{
			return null;
		}
	}

	private static Guid? GetDesktopId(IVirtualDesktopManager manager, HWND hwnd)
	{
		manager.GetWindowDesktopId(hwnd, out Guid id);
		return id == Guid.Empty ? null : id;
	}

	private static bool IsCurrent(IVirtualDesktopManager manager, HWND hwnd)
	{
		manager.IsWindowOnCurrentVirtualDesktop(hwnd, out var current);
		return current;
	}

	private static bool IsDesktopUnavailable(Exception ex)
	{
		// Match the HRESULT because COM can also project it to a managed exception during a shell restart.
		var hr = (HRESULT)ex.HResult;
		return hr == HRESULT.E_NOTIMPL || hr == HRESULT.REGDB_E_CLASSNOTREG ||
			hr == HRESULT.TYPE_E_ELEMENTNOTFOUND || hr == HRESULT.RPC_E_DISCONNECTED ||
			hr == PInvoke.HRESULT_FROM_WIN32(WIN32_ERROR.ERROR_INVALID_STATE) ||
			hr == PInvoke.HRESULT_FROM_WIN32((WIN32_ERROR)RPC_STATUS.RPC_S_SERVER_UNAVAILABLE);
	}
}
