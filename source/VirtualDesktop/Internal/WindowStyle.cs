namespace WindowsDesktop.Internal;

internal enum WindowStyle : int
{
	WS_BORDER = 0x00800000,
	WS_POPUP = unchecked((int)0x80000000),
	WS_VISIBLE = 0x10000000,
}
