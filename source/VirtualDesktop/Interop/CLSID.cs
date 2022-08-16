using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
// ReSharper disable InconsistentNaming

namespace WindowsDesktop.Interop
{
	public static class CLSID
	{
		public static Guid ImmersiveShell { get; } = new Guid("c2f03a33-21f5-47fa-b4bb-156362a2f239");

		public static Guid VirtualDesktopManager { get; } = new Guid("aa509086-5ca9-4c25-8f95-589d3c07b48a");
	}
}
