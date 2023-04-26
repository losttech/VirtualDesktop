using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Interop;

using WindowsDesktop;

namespace VirtualDesktopShowcase
{
	partial class MainWindow
	{
		public MainWindow()
		{
			this.InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			VirtualDesktop.CurrentChanged += this.VirtualDesktop_CurrentChanged;
			this.CurrentDesktop.Text = VirtualDesktop.CurrentID.ToString();
		}

		private void VirtualDesktop_CurrentChanged(object sender, VirtualDesktopChangedEventArgs e)
		{
			this.CurrentDesktop.Text = e.NewDesktop.Id.ToString();
			UpdateWindowDesktop();
		}

		private void UpdateWindowDesktop()
		{
			IntPtr hwnd = new WindowInteropHelper(this).Handle;
			var result = VirtualDesktop.TryGetIdFromHwnd(hwnd, out var desktopID);
			if (result.Succeeded)
				this.Title = desktopID.ToString();
			else
				this.Title = result.GetException().Message;
		}

		protected override void OnSourceInitialized(EventArgs e)
		{
			base.OnSourceInitialized(e);
			this.UpdateWindowDesktop();
		}

		private void GetID_Click(object sender, RoutedEventArgs e)
		{
			this.UpdateWindowDesktop();
		}
	}
}
