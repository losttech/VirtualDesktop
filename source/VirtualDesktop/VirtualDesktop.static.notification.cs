using System;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows.Threading;

using WindowsDesktop.Internal;
using WindowsDesktop.Interop;

using static WindowsDesktop.Interop.NativeMethods;

namespace WindowsDesktop
{
	partial class VirtualDesktop
	{
		/// <summary>
		/// Occurs when a current virtual desktop is changed.
		/// </summary>
		public static event EventHandler<VirtualDesktopChangedEventArgs>? CurrentChanged;

		static Guid currentID;
		public static Guid CurrentID => currentID;

		internal static IDisposable RegisterListener()
		{
			currentID = GetCurrentDesktopID();

			var window = new ForegroundListener
			{
				Name = "VirtualDesktopListener",
			};
			window.OnForegroundChanged += args =>
			{
				if (GetForegroundDesktopID() is { } newId)
					DesktopIDReceived(newId, ref currentID);
			};

			var timer = new DispatcherTimer(DispatcherPriority.Normal)
			{
				Interval = TimeSpan.FromMilliseconds(250),
				IsEnabled = true,
			};
			timer.Tick += delegate
			{
				DesktopIDReceived(GetCurrentDesktopID(), ref currentID);
			};
			window.Show();

			return Disposable.Create(() =>
			{
				timer.Stop();
				window.Close();
			});
		}

		static void DesktopIDReceived(Guid newId, ref Guid currentId)
		{
			if (newId == currentId) return;

			var newDesktop = new VirtualDesktop(newId);
			var oldDesktop = new VirtualDesktop(currentId);
			var changedArgs = new VirtualDesktopChangedEventArgs(oldDesktop, newDesktop);
			CurrentChanged?.Invoke(typeof(VirtualDesktop), changedArgs);
			currentId = newId;
		}

		static readonly Guid AppOnAllDesktops = new("BB64D5B7-4DE3-4AB2-A87C-DB7601AEA7DC");
		static readonly Guid WindowOnAllDesktops = new("C2DDEA68-66F2-4CF9-8264-1BFD00FBBBAC");
		static readonly Guid[] IgnoreDesktops =
		{
			Guid.Empty,
			AppOnAllDesktops,
			WindowOnAllDesktops,
		};

		static Guid? GetForegroundDesktopID()
		{
			IntPtr foregroundWindow = GetForegroundWindow();
			if (foregroundWindow == IntPtr.Zero || IsWindowOnCurrentVirtualDesktop(foregroundWindow) != true)
				return null;
			if (IdFromHwnd(foregroundWindow) is { } id && !IgnoreDesktops.Contains(id))
				return id;
			return null;
		}

		static Guid GetCurrentDesktopID()
		{
			Guid result = Guid.Empty;

			var callback = new EnumWindowsProc((hwnd, _) =>
			{
				if (!IsWindowVisible(hwnd)) return true;

				if (((WindowStyleEx)GetWindowLong(hwnd, GWL_EXSTYLE)).HasFlag(WindowStyleEx.WS_EX_TOOLWINDOW))
					return true;

				if (IsWindowOnCurrentVirtualDesktop(hwnd) != true)
					return true;

				if (IdFromHwnd(hwnd) is { } id && !IgnoreDesktops.Contains(id))
				{
					result = id;
					return false;
				}

				return true;
			});

			while (IgnoreDesktops.Contains(result))
			{
				if (GetForegroundDesktopID() is { } id)
					return id;

				EnumWindows(callback, 0);

				DoEvents();
			}

			GC.KeepAlive(callback);
			return result;
		}

		static void DoEvents()
		{
			var frame = new DispatcherFrame();
			Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background,
				new DispatcherOperationCallback(
					delegate (object f)
					{
						((DispatcherFrame)f).Continue = false;
						return null;
					}), frame);
			Dispatcher.PushFrame(frame);
		}
	}
}
