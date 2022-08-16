using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Threading;
using WindowsDesktop.Internal;

namespace WindowsDesktop.Interop
{
	public static class ComObjects
	{
		internal const int RPC_S_SERVER_UNAVAILABLE = unchecked((int)0x800706BA);

		private static IDisposable? _listener;
		private static ExplorerRestartListenerWindow? _listenerWindow;

		internal static IVirtualDesktopManager? VirtualDesktopManager { get; private set; }

		internal static void Initialize()
		{
			VirtualDesktopManager = MissingCOMInterfaceException.Ensure(GetVirtualDesktopManager());

			if (_listenerWindow == null)
			{
				_listenerWindow = new ExplorerRestartListenerWindow(() => {
					try {
						Initialize();
					} catch (NotSupportedException) { }
				});
				_listenerWindow.Show();
			}
		}

		internal static void RegisterListener() {
			// this requires at least VirtualDesktopManager to be set
			// VirtualDesktopNotificationService is a bonus
			_listener = VirtualDesktop.RegisterListener();
		}

		internal static void Terminate()
		{
			_listener?.Dispose();
			_listenerWindow?.Close();
		}

		private class ExplorerRestartListenerWindow : TransparentWindow
		{
			private uint _explorerRestertedMessage;
			private readonly Action _action;

			public ExplorerRestartListenerWindow(Action action)
			{
				this.Name = nameof(ExplorerRestartListenerWindow);
				this._action = action;
			}

			public override void Show()
			{
				base.Show();
				this._explorerRestertedMessage = NativeMethods.RegisterWindowMessage("TaskbarCreated");
			}

			protected override IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
			{
				if (msg == this._explorerRestertedMessage)
				{
					this._action();
					return IntPtr.Zero;
				}

				return base.WndProc(hwnd, msg, wParam, lParam, ref handled);
			}
		}


		#region public methods

		public static IVirtualDesktopManager GetVirtualDesktopManager()
		{
			var vdmType = Type.GetTypeFromCLSID(CLSID.VirtualDesktopManager);
			var instance = Activator.CreateInstance(vdmType)!;

			return (IVirtualDesktopManager)instance;
		}

		#endregion
	}
}
