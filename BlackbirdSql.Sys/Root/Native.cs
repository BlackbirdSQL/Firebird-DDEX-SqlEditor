
using System;
using System.Runtime.InteropServices;
using System.Security;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

using OLERECT = Microsoft.VisualStudio.OLE.Interop.RECT;



namespace BlackbirdSql.Sys;

[SuppressUnmanagedCodeSecurity]


// =========================================================================================================
//											Native Class
//
/// <summary>
/// Central location for accessing of native members. 
/// </summary>
// =========================================================================================================
public abstract class Native
{

	// ---------------------------------------------------------------------------------
	#region Constants and Static Fields - Native
	// ---------------------------------------------------------------------------------


	// Constants for sending messages to a Tree-View Control.
	public const int TV_FIRST = 0x1100;
	public const int TVM_GETNEXTITEM = (TV_FIRST + 10);
	public const int TVGN_ROOT = 0x0;
	public const int TVGN_CHILD = 0x4;
	public const int TVGN_NEXTVISIBLE = 0x6;
	public const int TVGN_CARET = 0x9;

	// Constants defining scroll bar parameters to set or retrieve.
	public const int SIF_RANGE = 0x1;
	public const int SIF_PAGE = 0x2;
	public const int SIF_POS = 0x4;

	// Identifies Vertical Scrollbar.
	public const int SB_VERT = 0x1;

	// Used for vertical scroll bar message.
	public const int SB_LINEUP = 0;
	public const int SB_LINEDOWN = 1;

	// Windows messages
	public const int WM_CREATE = 1;
	public const int WM_MOVE = 3;
	public const int WM_SIZE = 5;
	public const int WM_ACTIVATE = 6;
	public const int WM_SETFOCUS = 7;
	public const int WM_SETTEXT = 12;
	public const int WM_GETTEXT = 13;
	public const int WM_GETTEXTLENGTH = 14;
	public const int WM_PAINT = 15;
	public const int WM_ERASEBKGND = 20;
	public const int WM_SHOWWINDOW = 24;
	public const int WM_MOUSEACTIVATE = 33;
	public const int WM_GETMINMAXINFO = 36;
	public const int WM_SETFONT = 48;
	public const int WM_WINDOWPOSCHANGING = 70;
	public const int WM_WINDOWPOSCHANGED = 71;
	public const int WM_NOTIFY = 78;
	public const int WM_NOTIFYFORMAT = 85;
	public const int WM_CONTEXTMENU = 123;
	public const int WM_STYLECHANGING = 124;
	public const int WM_STYLECHANGED = 125;
	public const int WM_SETICON = 128;
	public const int WM_NCCREATE = 129;
	public const int WM_NCPAINT = 133;
	public const int WM_NCACTIVATE = 134;
	public const int WM_KEYFIRST = 256;
	public const int WM_CHAR = 258;
	public const int WM_KEYLAST = 264;
	public const int WM_VSCROLL = 277;
	public const int WM_CHANGEUISTATE = 295;
	public const int WM_UPDATEUISTATE = 296;
	public const int WM_QUERYUISTATE = 297;
	public const int WM_CTLCOLORBTN = 309;
	public const int WM_PARENTNOTIFY = 528;
	public const int WM_PRINTCLIENT = 792;


	#endregion Constants and Static Fields





	// =========================================================================================================
	#region Static Methods - Native
	// =========================================================================================================



	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	/// <summary>
	/// <see cref="ErrorHandler.Succeeded"/> token.
	/// </summary>
	protected static bool __(int hr) => ErrorHandler.Succeeded(hr);



	// IntCreateBitmap
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, short[] lpvBits);


	// IntCreateBrushIndirect
	[DllImport("gdi32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr CreateBrushIndirect(LOGBRUSHEx lb);


	// CreatePen
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr CreatePen(int nStyle, int nWidth, int crColor);


	// CreateStdAccessibleObject
	[DllImport("oleacc.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int CreateStdAccessibleObject(IntPtr hWnd, int objID, ref Guid refiid, [In][Out][MarshalAs(UnmanagedType.Interface)] ref object pAcc);


	// DefWindowProc
	[DllImport("User32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr DefWindowProc(IntPtr hWnd, int msg, IntPtr wParam, IntPtr lParam);


	// DeleteObject
	[DllImport("gdi32.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteObject(HandleRef hObject);


	// DeleteObject
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteObject(IntPtr hObject);


	// FindWindowEx
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);


	// GetActiveWindow
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetActiveWindow();


	// GetClientRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClientRect(IntPtr hwnd, out OLERECT lpRect);


	// GetDCEx
	[DllImport("User32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);


	// GetFocus
	[DllImport("user32.dll")]
	public static extern IntPtr GetFocus();


	// GetForegroundWindow
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetForegroundWindow();


	// GetParent
	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetParent(HandleRef hWnd);

	// GetParent
	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr GetParent(IntPtr hWnd);



	// GetScrollInfo
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool GetScrollInfo(IntPtr hWnd, int fnBar, [In][Out] SCROLLINFOEx si);


	// GetSysColor
	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern uint GetSysColor(int sysColor);


	// GetWindow
	[DllImport("user32.dll")]
	public static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);


	// GetWindowLongPtr32
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetWindowLong(IntPtr hWnd, int nIndex);


	// GetWindowLongPtr64
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr GetWindowLongPtr(IntPtr hWnd, int nIndex);


	// GetWindowRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hwnd, out RECTEx lpRect);


	// IsWindow
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindow(IntPtr hWnd);


	// InvalidateRect
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool InvalidateRect(IntPtr hWnd, COMRECTEx rect, bool erase);


	// LineTo
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool LineTo(HandleRef hdc, int x, int y);


	// MoveToEx
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool MoveToEx(HandleRef hdc, int x, int y, POINT pt);


	// PatBlt
	[DllImport("gdi32", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PatBlt(IntPtr hdc, int left, int top, int width, int height, int rop);


	// PostMessage
	[DllImport("user32.dll")]
	public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


	// ReleaseDC
	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);


	// ScrollWindow
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool ScrollWindow(IntPtr hWnd, int nXAmount, int nYAmount, ref RECTEx rectScrollRegion, ref RECTEx rectClip);


	// SelectObject
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

	// SelectObject
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);


	// SendMessage
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

	// SendMessage
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, IntPtr lParam);

	// SendMessage
	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);


	// SetScrollInfo
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int SetScrollInfo(IntPtr hWnd, int fnBar, SCROLLINFOEx si, bool redraw);


	// SetFocus
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SetFocus(IntPtr hWnd);


	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	public static extern int SetProp(HandleRef hWnd, string propName, HandleRef data);


	// SetWindowPos
	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);


	// SystemParametersInfo
	[DllImport("user32.DLL", CallingConvention = CallingConvention.StdCall, CharSet = CharSet.Unicode, ExactSpelling = true, SetLastError = true)]
	public static extern bool SystemParametersInfoW(uint uiAction, uint uiParam, IntPtr pvParam, uint fWinIni);


	#endregion Static Methods





	// =========================================================================================================
	#region Internal Classes and Structs - Native
	// =========================================================================================================

	public enum EnBrowseForFolderMessages
	{
		EnableOk = 0x465,
		Initialized = 1,
		IUnknown = 5,
		SelChanged = 2,
		SetExpanded = 0x46a,
		SetOkText = 0x469,
		SetSelectionA = 0x466,
		SetSelectionW = 0x467,
		SetStatusTextA = 0x464,
		SetStatusTextW = 0x468,
		ValidateFailedA = 3,
		ValidateFailedW = 4
	}


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
	public class BROWSEINFOEx
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


	[StructLayout(LayoutKind.Sequential)]
	public class COMRECTEx
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public COMRECTEx()
		{
		}

		public COMRECTEx(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}
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


	public class LOGBRUSHEx
	{
		public int lbStyle;

		public int lbColor;

		public IntPtr lbHatch;
	}


	public struct RECTEx
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public RECTEx(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public RECTEx(OLERECT rect)
		{
			left = rect.left;
			top = rect.top;
			right = rect.right;
			bottom = rect.bottom;
		}

		public static RECTEx FromXYWH(int x, int y, int width, int height)
		{
			return new RECTEx(x, y, x + width, y + height);
		}
	}


	[StructLayout(LayoutKind.Sequential)]
	public class SCROLLINFOEx
	{
		public int cbSize = Marshal.SizeOf(typeof(SCROLLINFOEx));
		public int fMask;
		public int nMin;
		public int nMax;
		public int nPage;
		public int nPos;
		public int nTrackPos;

		public SCROLLINFOEx()
		{
		}

		public SCROLLINFOEx(int mask, int min, int max, int page, int pos)
		{
			fMask = mask;
			nMin = min;
			nMax = max;
			nPage = page;
			nPos = pos;
		}

		public SCROLLINFOEx(bool bInitWithAllMask) : this()
		{
			if (bInitWithAllMask)
				fMask = 23;
		}
	}


	public class Shell32
	{
		[DllImport("shell32.dll")]
		public static extern int SHGetSpecialFolderLocation(IntPtr hwnd, int csidl, ref IntPtr ppidl);

		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern bool SHGetPathFromIDList(IntPtr pidl, IntPtr pszPath);


		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr SHBrowseForFolder([In] BROWSEINFOEx lpbi);

		[DllImport("shell32.dll")]
		public static extern int SHGetMalloc([Out][MarshalAs(UnmanagedType.LPArray)] IMalloc[] ppMalloc);
	}






	#endregion #region Internal Classes and Structs


}
