#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Runtime.InteropServices;
using System.Security;

namespace BlackbirdSql.Core;

[SuppressUnmanagedCodeSecurity]
public static class UnsafeNative
{
	[Flags]
	public enum EnBrowseInfos
	{
		ReturnOnlyFSDirs = 0x1,
		DontGoBelowDomain = 0x2,
		StatusText = 0x4,
		ReturnFSAncestors = 0x8,
		EditBox = 0x10,
		Validate = 0x20,
		NewDialogStyle = 0x40,
		UseNewUI = 0x50,
		AllowUrls = 0x80,
		BrowseForComputer = 0x1000,
		BrowseForPrinter = 0x2000,
		BrowseForEverything = 0x4000,
		ShowShares = 0x8000
	}



	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
	public class BROWSEINFO
	{
		public IntPtr hwndOwner;

		public IntPtr pidlRoot;

		public IntPtr pszDisplayName;

		public string lpszTitle;

		public int ulFlags;

		public IntPtr lpfn;

		public IntPtr lParam;

		public int iImage;
	}


	[ComImport]
	[Guid("00000002-0000-0000-c000-000000000046")]
	[InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMalloc
	{
		[PreserveSig]
		IntPtr Alloc(int cb);

		[PreserveSig]
		IntPtr Realloc(IntPtr pv, int cb);

		[PreserveSig]
		void Free(IntPtr pv);

		[PreserveSig]
		int GetSize(IntPtr pv);

		[PreserveSig]
		int DidAlloc(IntPtr pv);

		[PreserveSig]
		void HeapMinimize();
	}

	public class Shell32
	{
		[DllImport("shell32.dll")]
		public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);


		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SHBrowseForFolder([In] BROWSEINFO lpbi);

		[DllImport("shell32.dll")]
		public static extern int SHGetMalloc([Out][MarshalAs(UnmanagedType.LPArray)] IMalloc[] ppMalloc);
	}


	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetActiveWindow();

	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);

	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);
}
