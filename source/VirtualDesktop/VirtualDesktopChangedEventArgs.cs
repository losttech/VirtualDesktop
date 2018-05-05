using System;

namespace WindowsDesktop
{
	/// <summary>
	/// Provides data for the <see cref="VirtualDesktop.CurrentChanged"/> event.
	/// </summary>
	[Obsolete(VirtualDesktop.UnsupportedMessage)]
	public class VirtualDesktopChangedEventArgs : EventArgs
	{
		[Obsolete(VirtualDesktop.UnsupportedMessage)]
		public VirtualDesktop OldDesktop { get; }
		[Obsolete(VirtualDesktop.UnsupportedMessage)]
		public VirtualDesktop NewDesktop { get; }

		public VirtualDesktopChangedEventArgs(VirtualDesktop oldDesktop, VirtualDesktop newDesktop)
		{
			this.OldDesktop = oldDesktop;
			this.NewDesktop = newDesktop;
		}
	}
}
