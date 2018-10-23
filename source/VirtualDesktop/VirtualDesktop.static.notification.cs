using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Threading;
using WindowsDesktop.Internal;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	using System.Diagnostics;
	using System.Runtime.InteropServices;
	using System.Threading;

	partial class VirtualDesktop
	{
		private static uint? dwCookie;
		private static VirtualDesktopNotificationListener listener;

		/// <summary>
		/// Occurs when a virtual desktop is created.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public static event EventHandler<VirtualDesktop> Created;
		[Obsolete(UnsupportedMessage)]
		public static event EventHandler<VirtualDesktopDestroyEventArgs> DestroyBegin;
		[Obsolete(UnsupportedMessage)]
		public static event EventHandler<VirtualDesktopDestroyEventArgs> DestroyFailed;

		/// <summary>
		/// Occurs when a virtual desktop is destroyed.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public static event EventHandler<VirtualDesktopDestroyEventArgs> Destroyed;

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete(UnsupportedMessage)]
		public static event EventHandler ApplicationViewChanged;

		/// <summary>
		/// Occurs when a current virtual desktop is changed.
		/// </summary>
		public static event EventHandler<VirtualDesktopChangedEventArgs> CurrentChanged;

		internal static IDisposable RegisterListener() {
			if (ComObjects.VirtualDesktopNotificationService != null)
				try {
					return RegisterAdvancedListener();
				} catch {
					return RegisterMinimalListener();
				}
			else
				return RegisterMinimalListener();
		}

		static IDisposable RegisterMinimalListener() {
			var window = new TransparentWindow();
			window.Show();
			Guid? desktopId = null;
			var timeLimit = TimeSpan.FromSeconds(30);
			var limitTimer = Stopwatch.StartNew();
			COMException exception = null;
			int attempts = 10;
			while (limitTimer.Elapsed < timeLimit || attempts > 0) {
				attempts = Math.Max(0, attempts - 1);
				try {
					desktopId = VirtualDesktop.IdFromHwnd(window.Handle);
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
				var newId = VirtualDesktop.IdFromHwnd(window.Handle);
				if (newId == null || newId == desktopId)
					return;
				var newDesktop = VirtualDesktop.FromId(newId.Value);
				var oldDesktop = desktopId == null ? null : VirtualDesktop.FromId(desktopId.Value);
				var changedArgs = new VirtualDesktopChangedEventArgs(oldDesktop, newDesktop);
				CurrentChanged?.Invoke(typeof(VirtualDesktop), changedArgs);
				desktopId = newId;
			};

			return Disposable.Create(() => {
				timer.Stop();
				window.Close();
			});
		}

		static IDisposable RegisterAdvancedListener()
		{
			var service = ComObjects.VirtualDesktopNotificationService;
			listener = new VirtualDesktopNotificationListener();
			dwCookie = service.Register(listener);

			return Disposable.Create(() => {
				try {
					service.Unregister(dwCookie.Value);
				} catch (COMException e) when (e.HResult == ComObjects.RPC_S_SERVER_UNAVAILABLE) {
					// no need to unregister when service is gone
				}
			});
		}

		private class VirtualDesktopNotificationListener : IVirtualDesktopNotification
		{
			static VirtualDesktop FromComObject(IVirtualDesktop virtualDesktop) => 
				IsSupported 
					? VirtualDesktop.FromComObject(virtualDesktop)
					: VirtualDesktop.FromId(virtualDesktop.GetID());

			void IVirtualDesktopNotification.VirtualDesktopCreated(IVirtualDesktop pDesktop)
			{
				Created?.Invoke(this, FromComObject(pDesktop));
			}

			void IVirtualDesktopNotification.VirtualDesktopDestroyBegin(IVirtualDesktop pDesktopDestroyed, IVirtualDesktop pDesktopFallback)
			{
				var args = new VirtualDesktopDestroyEventArgs(FromComObject(pDesktopDestroyed), FromComObject(pDesktopFallback));
				DestroyBegin?.Invoke(this, args);
			}

			void IVirtualDesktopNotification.VirtualDesktopDestroyFailed(IVirtualDesktop pDesktopDestroyed, IVirtualDesktop pDesktopFallback)
			{
				var args = new VirtualDesktopDestroyEventArgs(FromComObject(pDesktopDestroyed), FromComObject(pDesktopFallback));
				DestroyFailed?.Invoke(this, args);
			}

			void IVirtualDesktopNotification.VirtualDesktopDestroyed(IVirtualDesktop pDesktopDestroyed, IVirtualDesktop pDesktopFallback)
			{
				var args = new VirtualDesktopDestroyEventArgs(FromComObject(pDesktopDestroyed), FromComObject(pDesktopFallback));
				Destroyed?.Invoke(this, args);
			}

			void IVirtualDesktopNotification.ViewVirtualDesktopChanged(IntPtr pView)
			{
				ApplicationViewChanged?.Invoke(this, EventArgs.Empty);
			}

			void IVirtualDesktopNotification.CurrentVirtualDesktopChanged(IVirtualDesktop pDesktopOld, IVirtualDesktop pDesktopNew)
			{
				var args = new VirtualDesktopChangedEventArgs(FromComObject(pDesktopOld), FromComObject(pDesktopNew));
				CurrentChanged?.Invoke(this, args);
			}
		}
	}
}
