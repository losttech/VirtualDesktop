namespace WindowsDesktop.Interop
{
	using System;
	static class ImmersiveShellServices
	{
		public static T Get<T>() {
			var shellType = Type.GetTypeFromCLSID(CLSID.ImmersiveShell);
			var shell = (IServiceProvider)Activator.CreateInstance(shellType);

			object ppvObject;
			shell.QueryService(typeof(T).GUID, typeof(T).GUID, out ppvObject);

			return (T)ppvObject;
		}
	}
}
