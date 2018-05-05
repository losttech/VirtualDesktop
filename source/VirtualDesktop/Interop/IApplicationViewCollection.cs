using System;
using System.Runtime.InteropServices;

namespace WindowsDesktop.Interop
{
	internal static class ApplicationViewCollection
	{
		public static IApplicationViewCollection Get() {
			object com = ImmersiveShellServices.Get<ApplicationViewCollection_2c08adf0_a386_4b35_9250_0fe183476fcc.IApplicationViewCollection>();
			if (com != null)
				return new ApplicationViewCollection_2c08adf0_a386_4b35_9250_0fe183476fcc(com);

			//disabled until this Windows version is supported
			//com = ImmersiveShellServices.Get<ApplicationViewCollection_1841c6d7_4f9d_42c0_af41_8747538f10e5.IApplicationViewCollection>();
			//if (com != null)
			//	return new ApplicationViewCollection_1841c6d7_4f9d_42c0_af41_8747538f10e5(com);

			return null;
		}
	}

	[Obsolete(VirtualDesktop.UnsupportedMessage)]
	public interface IApplicationViewCollection
	{
		int GetViews(out IObjectArray array);

		int GetViewsByZOrder(out IObjectArray array);

		int GetViewsByAppUserModelId(string id, out IObjectArray array);

		int GetViewForHwnd(IntPtr hwnd, out IApplicationView view);

		int GetViewForApplication(object application, out IApplicationView view);

		int GetViewForAppUserModelId(string id, out IApplicationView view);

		int GetViewInFocus(out IntPtr view);

		void outreshCollection();

		int RegisterForApplicationViewChanges(object listener, out int cookie);

		int RegisterForApplicationViewPositionChanges(object listener, out int cookie);

		int UnregisterForApplicationViewChanges(int cookie);
	}

	class ApplicationViewCollection_2c08adf0_a386_4b35_9250_0fe183476fcc : IApplicationViewCollection
	{
		public ApplicationViewCollection_2c08adf0_a386_4b35_9250_0fe183476fcc(object wrapped) {
			this.wrapped = (IApplicationViewCollection)wrapped ?? throw new ArgumentNullException(nameof(wrapped));
		}

		readonly IApplicationViewCollection wrapped;

		public int GetViewForApplication(object application, out IApplicationView view) => this.wrapped.GetViewForApplication(application, out view);
		public int GetViewForAppUserModelId(string id, out IApplicationView view) => this.wrapped.GetViewForAppUserModelId(id, out view);
		public int GetViewForHwnd(IntPtr hwnd, out IApplicationView view) => this.wrapped.GetViewForHwnd(hwnd, out view);
		public int GetViewInFocus(out IntPtr view) => this.wrapped.GetViewInFocus(out view);
		public int GetViews(out IObjectArray array) => this.wrapped.GetViews(out array);
		public int GetViewsByAppUserModelId(string id, out IObjectArray array) => this.wrapped.GetViewsByAppUserModelId(id, out array);
		public int GetViewsByZOrder(out IObjectArray array) => this.wrapped.GetViewsByZOrder(out array);
		public void outreshCollection() => this.wrapped.RefreshCollection();
		public int RegisterForApplicationViewChanges(object listener, out int cookie) => this.wrapped.RegisterForApplicationViewChanges(listener, out cookie);
		public int RegisterForApplicationViewPositionChanges(object listener, out int cookie) => this.wrapped.RegisterForApplicationViewPositionChanges(listener, out cookie);
		public int UnregisterForApplicationViewChanges(int cookie) => this.wrapped.UnregisterForApplicationViewChanges(cookie);

		[ComImport]
		[Guid("2c08adf0-a386-4b35-9250-0fe183476fcc")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IApplicationViewCollection
		{
			int GetViews(out IObjectArray array);

			int GetViewsByZOrder(out IObjectArray array);

			int GetViewsByAppUserModelId(string id, out IObjectArray array);

			int GetViewForHwnd(IntPtr hwnd, out IApplicationView view);

			int GetViewForApplication(object application, out IApplicationView view);

			int GetViewForAppUserModelId(string id, out IApplicationView view);

			int GetViewInFocus(out IntPtr view);

			void RefreshCollection();

			int RegisterForApplicationViewChanges(object listener, out int cookie);

			int RegisterForApplicationViewPositionChanges(object listener, out int cookie);

			int UnregisterForApplicationViewChanges(int cookie);
		}
	}

	class ApplicationViewCollection_1841c6d7_4f9d_42c0_af41_8747538f10e5 : IApplicationViewCollection
	{
		public ApplicationViewCollection_1841c6d7_4f9d_42c0_af41_8747538f10e5(object wrapped) {
			this.wrapped = (IApplicationViewCollection)wrapped ?? throw new ArgumentNullException(nameof(wrapped));
		}

		readonly IApplicationViewCollection wrapped;

		public int GetViewForApplication(object application, out IApplicationView view) => this.wrapped.GetViewForApplication(application, out view);
		public int GetViewForAppUserModelId(string id, out IApplicationView view) => this.wrapped.GetViewForAppUserModelId(id, out view);
		public int GetViewForHwnd(IntPtr hwnd, out IApplicationView view) => this.wrapped.GetViewForHwnd(hwnd, out view);
		public int GetViewInFocus(out IntPtr view) => this.wrapped.GetViewInFocus(out view);
		public int GetViews(out IObjectArray array) => this.wrapped.GetViews(out array);
		public int GetViewsByAppUserModelId(string id, out IObjectArray array) => this.wrapped.GetViewsByAppUserModelId(id, out array);
		public int GetViewsByZOrder(out IObjectArray array) => this.wrapped.GetViewsByZOrder(out array);
		public void outreshCollection() => this.wrapped.outreshCollection();
		public int RegisterForApplicationViewChanges(object listener, out int cookie) => this.wrapped.RegisterForApplicationViewChanges(listener, out cookie);
		public int RegisterForApplicationViewPositionChanges(object listener, out int cookie) => this.wrapped.RegisterForApplicationViewPositionChanges(listener, out cookie);
		public int UnregisterForApplicationViewChanges(int cookie) => this.wrapped.UnregisterForApplicationViewChanges(cookie);

		[ComImport]
		[Guid("1841c6d7-4f9d-42c0-af41-8747538f10e5")]
		[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
		internal interface IApplicationViewCollection
		{
			int GetViews(out IObjectArray array);

			int GetViewsByZOrder(out IObjectArray array);

			int GetViewsByAppUserModelId(string id, out IObjectArray array);

			int GetViewForHwnd(IntPtr hwnd, out IApplicationView view);

			int GetViewForApplication(object application, out IApplicationView view);

			int GetViewForAppUserModelId(string id, out IApplicationView view);

			int GetViewInFocus(out IntPtr view);

			void outreshCollection();

			int RegisterForApplicationViewChanges(object listener, out int cookie);

			int RegisterForApplicationViewPositionChanges(object listener, out int cookie);

			int UnregisterForApplicationViewChanges(int cookie);
		}
	}
}
