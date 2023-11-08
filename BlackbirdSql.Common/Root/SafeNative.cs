#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;



namespace BlackbirdSql.Common;


[SuppressUnmanagedCodeSecurity]
public static class SafeNative
{

	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool DeleteObject(IntPtr hObject);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool ScrollWindow(IntPtr hWnd, int nXAmount, int nYAmount, ref Native.RECT rectScrollRegion, ref Native.RECT rectClip);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool InvalidateRect(IntPtr hWnd, Native.COMRECT rect, bool erase);

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern int GetSysColor(int nIndex);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);

	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool DrawFrameControl(IntPtr hDC, ref Native.RECT rect, int type, int state);
}
