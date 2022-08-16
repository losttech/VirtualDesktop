using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Windows.Threading;
using WindowsDesktop.Internal;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	partial class VirtualDesktop
	{
		/// <summary>
		/// Occurs when a current virtual desktop is changed.
		/// </summary>
		public static event EventHandler<VirtualDesktopChangedEventArgs> CurrentChanged;

		internal static IDisposable RegisterListener() {
			Guid? desktopId = null;
			var timeLimit = TimeSpan.FromSeconds(30);
			var limitTimer = Stopwatch.StartNew();
			COMException exception = null;
			int attempts = 10;
			while (limitTimer.Elapsed < timeLimit || attempts > 0) {
				attempts = Math.Max(0, attempts - 1);
				try {
					desktopId = VirtualDesktop.IdFromHwnd(NativeMethods.GetForegroundWindow());
					exception = null;
				} catch (COMException ex) when (ex.Match(HResult.INVALID_STATE)) {
					Dispatcher.CurrentDispatcher.Invoke(DispatcherPriority.Background, new Action(() => {}));
					if (limitTimer.Elapsed >= timeLimit && attempts <= 0)
						throw;
					exception = ex;
				}
			}

			if (desktopId == null && exception != null && limitTimer.Elapsed >= timeLimit)
				throw exception;
				
			var timer = new DispatcherTimer(DispatcherPriority.Normal) {
				Interval = TimeSpan.FromMilliseconds(250),
				IsEnabled = true,
			};
			timer.Tick += delegate {
				var newId = VirtualDesktop.IdFromHwnd(NativeMethods.GetForegroundWindow());
				if (newId == null || newId == desktopId || newId == Guid.Empty)
					return;
				var newDesktop = new VirtualDesktop(newId.Value);
				var oldDesktop = desktopId == null || desktopId == Guid.Empty ? null : new VirtualDesktop(desktopId.Value);
				var changedArgs = new VirtualDesktopChangedEventArgs(oldDesktop, newDesktop);
				CurrentChanged?.Invoke(typeof(VirtualDesktop), changedArgs);
				desktopId = newId;
			};

			return Disposable.Create(() => {
				timer.Stop();
			});
		}
	}
}
