using System;
using System.Diagnostics;
using System.Threading;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.UI.Accessibility;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WindowsDesktop.Internal;

internal static class DesktopListener
{
	internal static void Start(Action foregroundChanged, Action poll, Action<Exception> reportError)
	{
		var thread = new Thread(Run) { Name = "Virtual desktop listener", IsBackground = true };
		thread.SetApartmentState(ApartmentState.STA);
		thread.Start();

		void Run()
		{
			var dispatcher = Dispatcher.CurrentDispatcher;
			WINEVENTPROC callback = (_, _, _, objectId, _, _, _) =>
			{
				if (objectId == (int)OBJECT_IDENTIFIER.OBJID_WINDOW)
					dispatcher.BeginInvoke(foregroundChanged);
			};
			var hook = default(HWINEVENTHOOK);
			var timer = new DispatcherTimer(DispatcherPriority.Background, dispatcher)
			{
				Interval = TimeSpan.FromMilliseconds(250),
			};
			void Stop(object? sender, EventArgs args) => dispatcher.BeginInvokeShutdown(DispatcherPriority.Send);

			try
			{
				AppDomain.CurrentDomain.ProcessExit += Stop;
				AppDomain.CurrentDomain.DomainUnload += Stop;
				hook = PInvoke.SetWinEventHook(PInvoke.EVENT_SYSTEM_FOREGROUND, PInvoke.EVENT_SYSTEM_FOREGROUND,
					HMODULE.Null, callback, 0, 0, PInvoke.WINEVENT_OUTOFCONTEXT);
				if (hook.IsNull)
					Trace.TraceWarning("The foreground hook is unavailable. Desktop changes will be detected by polling.");

				timer.Tick += (_, _) => poll();
				timer.Start();
				Dispatcher.Run();
			}
			catch (Exception ex)
			{
				reportError(ex);
			}
			finally
			{
				AppDomain.CurrentDomain.ProcessExit -= Stop;
				AppDomain.CurrentDomain.DomainUnload -= Stop;
				timer.Stop();
				if (!hook.IsNull)
					PInvoke.UnhookWinEvent(hook);
				GC.KeepAlive(callback);
			}
		}
	}
}
