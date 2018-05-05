namespace WindowsDesktop.Interop
{
	using System;
	class MissingCOMInterfaceException : NotSupportedException
	{
		public MissingCOMInterfaceException(Type type):base(MakeMessage(type)) { }

		static string MakeMessage(Type type) {
			string name = type?.Name ?? throw new ArgumentNullException(nameof(type));
			return $"COM interface type {name} can't be found. Did the CLSID change in this Windows build?";
		}

		internal static T Ensure<T>(T comInstance) where T : class => comInstance ?? throw new MissingCOMInterfaceException(typeof(T));
	}
}
