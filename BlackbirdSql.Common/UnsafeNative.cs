#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;




namespace BlackbirdSql.Common;


public static class UnsafeNative
{
	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);

	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
