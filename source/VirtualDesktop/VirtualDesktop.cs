﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using WindowsDesktop.Interop;

namespace WindowsDesktop
{
	/// <summary>
	/// Encapsulates a virtual desktop on Windows 10.
	/// </summary>
	[DebuggerDisplay("{Id}")]
	public partial class VirtualDesktop
	{
		internal const string UnsupportedMessage = "This member is not supported and might stop working when a new version of Windows is released";
		/// <summary>
		/// Gets the unique identifier for the virtual desktop.
		/// </summary>
		public Guid Id { get; }

		[EditorBrowsable(EditorBrowsableState.Never)]
		[Obsolete("Use desktop ID tracking instead")]
		public IVirtualDesktop ComObject => ComObjects.GetVirtualDesktop(this.Id);

		private VirtualDesktop(IVirtualDesktop comObject)
		{
			ComObjects.Register(comObject);
			this.Id = comObject.GetID();
		}

		private VirtualDesktop(Guid id) {
			if (id == Guid.Empty)
				throw new ArgumentNullException(nameof(id));

			this.Id = id;
		}


		/// <summary>
		/// Display the virtual desktop.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public void Switch()
		{
			ComObjects.VirtualDesktopManagerInternal.SwitchDesktop(this.ComObject);
		}

		/// <summary>
		/// Remove the virtual desktop.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public void Remove()
		{
			this.Remove(GetDesktopsInternal().FirstOrDefault(x => x.Id != this.Id) ?? Create());
		}

		/// <summary>
		/// Remove the virtual desktop, specifying a virtual desktop that display after destroyed.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public void Remove(VirtualDesktop fallbackDesktop)
		{
			if (fallbackDesktop == null) throw new ArgumentNullException(nameof(fallbackDesktop));

			ComObjects.VirtualDesktopManagerInternal.RemoveDesktop(this.ComObject, fallbackDesktop.ComObject);
		}

		/// <summary>
		/// Returns a virtual desktop on the left.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public VirtualDesktop GetLeft()
		{
			IVirtualDesktop desktop;
			try
			{
				desktop = ComObjects.VirtualDesktopManagerInternal.GetAdjacentDesktop(this.ComObject, AdjacentDesktop.LeftDirection);
			}
			catch (COMException ex) when (ex.Match(HResult.TYPE_E_OUTOFBOUNDS))
			{
				return null;
			}
			var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));

			return wrapper;
		}

		/// <summary>
		/// Returns a virtual desktop on the right.
		/// </summary>
		[Obsolete(UnsupportedMessage)]
		public VirtualDesktop GetRight()
		{
			IVirtualDesktop desktop;
			try
			{
				desktop = ComObjects.VirtualDesktopManagerInternal.GetAdjacentDesktop(this.ComObject, AdjacentDesktop.RightDirection);
			}
			catch (COMException ex) when (ex.Match(HResult.TYPE_E_OUTOFBOUNDS))
			{
				return null;
			}
			var wrapper = _wrappers.GetOrAdd(desktop.GetID(), _ => new VirtualDesktop(desktop));

			return wrapper;
		}
	}
}
