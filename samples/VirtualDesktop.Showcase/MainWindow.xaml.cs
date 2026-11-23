using System;
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
			try
			{
				this.Title = VirtualDesktop.IdFromHwnd(hwnd)?.ToString() ?? "Desktop unavailable";
			}
			catch (Exception ex)
			{
				this.Title = ex.Message;
			}
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
