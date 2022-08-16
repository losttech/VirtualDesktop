using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using System.Windows;
using WindowsDesktop;

namespace VirtualDesktopShowcase
{
	partial class MainWindow
	{
		public MainWindow()
		{
			this.InitializeComponent();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e) {
            VirtualDesktop.CurrentChanged += this.VirtualDesktop_CurrentChanged;
		}

        private void VirtualDesktop_CurrentChanged(object sender, VirtualDesktopChangedEventArgs e) {
            this.CurrentDesktop.Text = e.NewDesktop.Id.ToString();
        }
    }
}
