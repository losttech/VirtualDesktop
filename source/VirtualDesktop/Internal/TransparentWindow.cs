using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Interop;

namespace WindowsDesktop.Internal
{
	internal class TransparentWindow : RawWindow
	{
		public override void Show()
		{
			var parameters = new HwndSourceParameters(this.Name, width: 1, height: 1)
			{
				WindowStyle = (int)(WindowStyle.WS_BORDER),
			};

			parameters.ExtendedWindowStyle |= WS_EX_TOOLWINDOW;

			this.Show(parameters);
		}
	}
}
