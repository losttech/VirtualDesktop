using System;
using System.Linq;
// ReSharper disable InconsistentNaming

namespace WindowsDesktop.Interop
{
	public enum HResult : uint
	{
		/// <summary>
		/// You can not call shell functions from a WndProc handler,
		/// when the message needs synchronous processing (e.g. sent by SendMessage).
		/// </summary>
		RPC_E_CANTCALLOUT_ININPUTSYNCCALL = 0x8001010D,
		TYPE_E_OUTOFBOUNDS = 0x80028CA1,
		TYPE_E_ELEMENTNOTFOUND = 0x8002802B,
		REGDB_E_CLASSNOTREG = 0x80040154,
		INVALID_STATE = 0x8007139F,
	}

	public static class HResultExtensions
	{
		public static bool Match(this Exception ex, params HResult[] hResult)
		{
			return hResult.Select(x => (uint)x).Any(x => ((uint)ex.HResult) == x);
		}
	}
}
