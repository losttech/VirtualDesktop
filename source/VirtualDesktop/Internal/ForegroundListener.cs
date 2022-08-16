namespace WindowsDesktop.Internal;

using System;
using System.ComponentModel;
using System.Diagnostics;

using static WindowsDesktop.Interop.NativeMethods;

internal class ForegroundListener : TransparentWindow
{
	readonly WinEventProc proc;
	IntPtr hook;
	public ForegroundListener()
	{
		this.proc = this.Hook;
	}

	public event Action<WindowEventArgs>? OnForegroundChanged;

	public override void Show()
	{
		base.Show();

		this.hook = SetWinEventHook(WindowEvent.ForegroundChanged,
			WindowEvent.ForegroundChanged,
			moduleHandle: IntPtr.Zero,
			callback: this.proc,
			processID: 0, threadID: 0,
			flags: HookFlags.OutOfContext);

		if (this.hook == IntPtr.Zero)
			throw new Win32Exception();
	}

	void Hook(IntPtr hookHandle, WindowEvent @event,
			IntPtr hwnd,
			ObjectCategory category, int child,
			int threadID, int timestampMs)
	{
		if (category != ObjectCategory.Window)
			return;
		Debug.Assert(child == 0);
		this.OnForegroundChanged?.Invoke(new WindowEventArgs(hwnd));
	}

	public override void Close()
	{
		UnhookWinEvent(this.hook);
		base.Close();
	}
}
