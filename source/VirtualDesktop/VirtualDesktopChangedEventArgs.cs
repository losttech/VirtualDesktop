using System;

namespace WindowsDesktop
{
	/// <summary>
	/// Provides data for the <see cref="VirtualDesktop.CurrentChanged"/> event.
	/// </summary>
	public class VirtualDesktopChangedEventArgs : EventArgs
	{
		/// <summary>Gets the previous desktop, or null when the first desktop is identified.</summary>
		public VirtualDesktop? OldDesktop { get; }
		public VirtualDesktop NewDesktop { get; }

		public VirtualDesktopChangedEventArgs(VirtualDesktop? oldDesktop, VirtualDesktop newDesktop)
		{
			this.OldDesktop = oldDesktop;
			this.NewDesktop = newDesktop;
		}
	}
}
