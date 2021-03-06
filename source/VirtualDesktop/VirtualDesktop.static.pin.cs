﻿using System;
using System.Collections.Generic;
using System.Linq;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	partial class VirtualDesktop
	{
		[Obsolete(UnsupportedMessage)]
		public static bool IsPinnedWindow(IntPtr hWnd)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			return ComObjects.VirtualDesktopPinnedApps.IsViewPinned(hWnd.GetApplicationView());
		}

		[Obsolete(UnsupportedMessage)]
		public static void PinWindow(IntPtr hWnd)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			var view = hWnd.GetApplicationView();

			if (!ComObjects.VirtualDesktopPinnedApps.IsViewPinned(view))
			{
				ComObjects.VirtualDesktopPinnedApps.PinView(view);
			}
		}

		[Obsolete(UnsupportedMessage)]
		public static void UnpinWindow(IntPtr hWnd)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			var view = hWnd.GetApplicationView();

			if (ComObjects.VirtualDesktopPinnedApps.IsViewPinned(view))
			{
				ComObjects.VirtualDesktopPinnedApps.UnpinView(view);
			}
		}

		[Obsolete(UnsupportedMessage)]
		public static bool IsPinnedApplication(string appId)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			return ComObjects.VirtualDesktopPinnedApps.IsAppIdPinned(appId);
		}

		[Obsolete(UnsupportedMessage)]
		public static void PinApplication(string appId)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			if (!ComObjects.VirtualDesktopPinnedApps.IsAppIdPinned(appId))
			{
				ComObjects.VirtualDesktopPinnedApps.PinAppID(appId);
			}
		}

		[Obsolete(UnsupportedMessage)]
		public static void UnpinApplication(string appId)
		{
			VirtualDesktopHelper.ThrowIfNotSupported();

			if (ComObjects.VirtualDesktopPinnedApps.IsAppIdPinned(appId))
			{
				ComObjects.VirtualDesktopPinnedApps.UnpinAppID(appId);
			}
		}
	}
}
