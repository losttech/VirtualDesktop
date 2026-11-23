using System;
using System.Diagnostics;

namespace WindowsDesktop
{
	/// <summary>
	/// Encapsulates a virtual desktop on Windows 10.
	/// </summary>
	[DebuggerDisplay("{Id}")]
	public partial class VirtualDesktop
	{
		/// <summary>
		/// Gets the unique identifier for the virtual desktop.
		/// </summary>
		public Guid Id { get; }

		public VirtualDesktop(Guid id) {
			if (id == Guid.Empty)
				throw new ArgumentNullException(nameof(id));

			this.Id = id;
		}
	}
}
