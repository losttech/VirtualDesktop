using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Threading;
using System.Windows.Threading;
using Windows.Win32;
using Windows.Win32.Foundation;
using Windows.Win32.Graphics.Dwm;
using Windows.Win32.UI.Shell;
using Windows.Win32.UI.WindowsAndMessaging;

namespace WindowsDesktop;

partial class VirtualDesktop
{
	private sealed record Subscription(EventHandler<VirtualDesktopChangedEventArgs> Handler, SynchronizationContext? Context);
	private static readonly List<Subscription> subscriptions = [];

	/// <summary>Occurs when the current desktop is first identified or changes.</summary>
	/// <remarks>Handlers run on the subscribing synchronization context, or on the listener thread when none exists.</remarks>
	public static event EventHandler<VirtualDesktopChangedEventArgs>? CurrentChanged
	{
		add
		{
			if (value is null) return;
			var context = SynchronizationContext.Current;
			if (context is null && Dispatcher.FromThread(Thread.CurrentThread) is { } dispatcher)
				context = new DispatcherSynchronizationContext(dispatcher);
			lock (subscriptions)
			{
				foreach (EventHandler<VirtualDesktopChangedEventArgs> handler in value.GetInvocationList())
					subscriptions.Add(new(handler, context));
			}
		}
		remove
		{
			if (value is null) return;
			var handlers = value.GetInvocationList();
			lock (subscriptions)
			{
				// Delegate.Remove removes the last matching invocation sequence.
				for (int i = subscriptions.Count - handlers.Length; i >= 0; i--)
				{
					if (!subscriptions.Skip(i).Take(handlers.Length).Select(s => s.Handler).SequenceEqual(handlers)) continue;
					subscriptions.RemoveRange(i, handlers.Length);
					break;
				}
			}
		}
	}

	private static volatile VirtualDesktop? currentDesktop;

	/// <summary>Gets the last observed desktop ID, or an empty GUID until a desktop is identified.</summary>
	public static Guid CurrentID => currentDesktop?.Id ?? Guid.Empty;

	private static readonly Guid AppOnAllDesktops = new("BB64D5B7-4DE3-4AB2-A87C-DB7601AEA7DC");
	private static readonly Guid WindowOnAllDesktops = new("C2DDEA68-66F2-4CF9-8264-1BFD00FBBBAC");

	private static long nextObserverActivation;
	private static long nextLookupWarning;

	private static IVirtualDesktopManager? CreateObserverManager()
	{
		if (Stopwatch.GetTimestamp() < nextObserverActivation) return null;
		var manager = TryCreateManager(out var error);
		if (manager is null)
		{
			var delaySeconds = error is NotSupportedException ? 60 : 1;
			nextObserverActivation = Stopwatch.GetTimestamp() + delaySeconds * Stopwatch.Frequency;
		}
		return manager;
	}

	private static void ObserveForegroundDesktop()
	{
		if (CreateObserverManager() is { } manager && ReadObservedDesktop(manager, PInvoke.GetForegroundWindow()) is { } id)
			DesktopIdReceived(id);
	}

	private static void ObserveCurrentDesktop()
	{
		if (CreateObserverManager() is not { } manager) return;
		Guid? id = ReadObservedDesktop(manager, PInvoke.GetForegroundWindow());
		if (id is null)
		{
			PInvoke.EnumWindows((hwnd, _) =>
			{
				if (PInvoke.IsWindowVisible(hwnd))
				{
					var style = (WINDOW_EX_STYLE)PInvoke.GetWindowLong(hwnd, WINDOW_LONG_PTR_INDEX.GWL_EXSTYLE);
					if ((style & WINDOW_EX_STYLE.WS_EX_TOOLWINDOW) == 0 && !IsCloaked(hwnd))
						id = ReadObservedDesktop(manager, hwnd);
				}
				return id is null;
			}, default);
		}
		// An empty desktop or a restarting shell may yield no usable window this tick.
		if (id is { } observed)
			DesktopIdReceived(observed);
	}

	private static unsafe bool IsCloaked(HWND hwnd)
	{
		uint cloaked = 0;
		return PInvoke.DwmGetWindowAttribute(hwnd, DWMWINDOWATTRIBUTE.DWMWA_CLOAKED, &cloaked, sizeof(uint)).Succeeded
			&& cloaked != 0;
	}

	private static Guid? ReadObservedDesktop(IVirtualDesktopManager manager, HWND hwnd)
	{
		if (hwnd.IsNull) return null;

		try
		{
			if (IsCurrent(manager, hwnd) && GetDesktopId(manager, hwnd) is { } id &&
				id != AppOnAllDesktops && id != WindowOnAllDesktops)
				return id;
		}
		catch (Exception ex) when (ex is UnauthorizedAccessException || IsDesktopUnavailable(ex))
		{
			// Inaccessible or temporarily unavailable windows cannot identify the desktop.
		}
		catch (Exception ex)
		{
			// A failed window query must not escape the native enumeration callback.
			if (Stopwatch.GetTimestamp() >= nextLookupWarning)
			{
				nextLookupWarning = Stopwatch.GetTimestamp() + 60 * Stopwatch.Frequency;
				Trace.TraceWarning("Desktop lookup for {0} failed: {1}", hwnd, ex);
			}
		}
		return null;
	}

	private static void DesktopIdReceived(Guid id)
	{
		var previous = currentDesktop;
		if (previous?.Id == id) return;

		var desktop = new VirtualDesktop(id);
		currentDesktop = desktop;
		var args = new VirtualDesktopChangedEventArgs(previous, desktop);
		Subscription[] snapshot;
		lock (subscriptions) snapshot = subscriptions.ToArray();
		foreach (var subscription in snapshot)
		{
			try
			{
				if (subscription.Context is { } context)
					context.Post(_ => subscription.Handler(typeof(VirtualDesktop), args), null);
				else
					subscription.Handler(typeof(VirtualDesktop), args);
			}
			catch (Exception ex)
			{
				Trace.TraceWarning("Could not deliver a desktop change: {0}", ex);
			}
		}
	}
}
