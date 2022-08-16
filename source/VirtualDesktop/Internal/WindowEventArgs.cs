namespace WindowsDesktop.Internal;

using System;

// <summary>
/// The window event arguments.
/// </summary>
public struct WindowEventArgs
{
	public WindowEventArgs(IntPtr handle)
	{
		this.Handle = handle;
	}
	public IntPtr Handle { get; }
}
