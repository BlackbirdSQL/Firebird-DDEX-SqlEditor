
using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Security;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Core;


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
	#region Enums, Constants and Static Fields - Native
	// ---------------------------------------------------------------------------------


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
	


	[StructLayout(LayoutKind.Sequential)]
	public class SCROLLINFO
	{
		public int cbSize = Marshal.SizeOf(typeof(SCROLLINFO));
		public int fMask;
		public int nMin;
		public int nMax;
		public int nPage;
		public int nPos;
		public int nTrackPos;

		public SCROLLINFO()
		{
		}

		public SCROLLINFO(int mask, int min, int max, int page, int pos)
		{
			fMask = mask;
			nMin = min;
			nMax = max;
			nPage = page;
			nPos = pos;
		}

		public SCROLLINFO(bool bInitWithAllMask) : this()
		{
			if (bInitWithAllMask)
				fMask = 23;
		}
	}

	#endregion Enums, Constants and Static Fields





	// =========================================================================================================
	#region Static Methods - Native
	// =========================================================================================================


	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern int DispatchMessage([In] ref MSG msg);


	[DllImport("user32.dll")]
	public static extern bool EnableWindow(IntPtr hWnd, bool enable);


	// Failed
	public static bool Failed(int hr)
	{
		return hr < 0;
	}

	public static bool Failed(int hr, params int[] expectedFailures)
	{
		if (Failed(hr) && (expectedFailures == null || Array.IndexOf(expectedFailures, hr) < 0))
		{
			return true;
		}

		return false;
	}


	// FindWindowEx
	[DllImport("user32.dll", SetLastError = true)]
	public static extern IntPtr FindWindowEx(IntPtr parentHandle, IntPtr childAfter, string className, string windowTitle);


	// GetMessage
	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern bool GetMessage([In][Out] ref MSG msg, int hWnd, int uMsgFilterMin, int uMsgFilterMax);


	// GetScrollInfo
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool GetScrollInfo(IntPtr hWnd, int fnBar, [In][Out] SCROLLINFO si);


	/*
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetScrollInfo(IntPtr hwnd, int fnBar, ref SCROLLINFO lpsi);
	*/



	// PostMessage
	[DllImport("user32.dll")]
	public static extern bool PostMessage(IntPtr hWnd, uint Msg, int wParam, int lParam);


	// SendMessage
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, string lParam);

	// SendMessage
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SendMessage(HandleRef hWnd, int msg, int wParam, IntPtr lParam);

	// SendMessage
	[DllImport("user32.dll", CharSet = CharSet.Unicode, SetLastError = true)]
	public static extern IntPtr SendMessage(IntPtr hWnd, int nMsg, IntPtr wParam, IntPtr lParam);

	// SendMessage
	public static IntPtr SendMessage(IntPtr hwnd, int msg)
	{
		return SendMessage(hwnd, msg, IntPtr.Zero, IntPtr.Zero);
	}

	// SendMessage
	public static IntPtr SendMessage(IntPtr hwnd, int msg, IntPtr wParam)
	{
		return SendMessage(hwnd, msg, wParam, IntPtr.Zero);
	}

	
	/*
	[DllImport("QCall", CharSet = CharSet.Unicode)]
	[SecurityCritical]
	[SuppressUnmanagedCodeSecurity]
	public static extern bool InternalUseRandomizedHashing();
	*/


	[DllImport("User32.dll", CharSet = CharSet.Unicode)]
	public static extern int SetProp(HandleRef hWnd, string propName, HandleRef data);


	// Succeeded
	public static bool Succeeded(int hr)
	{
		return hr >= 0;
	}



	// ThrowOnFailure
	public static int ThrowOnFailure(int hr)
	{
		return ThrowOnFailure(hr, (string)null);
	}

	// ThrowOnFailure
	public static int ThrowOnFailure(int hr, string context = null)
	{
		if (Failed(hr))
			Marshal.ThrowExceptionForHR(hr);

		return hr;
	}

	// ThrowOnFailure
	public static int ThrowOnFailure(int hr, params int[] expectedFailures)
	{
		if (Failed(hr) && (expectedFailures == null || Array.IndexOf(expectedFailures, hr) < 0))
		{
			Exception ex = Marshal.GetExceptionForHR(hr);
			Diag.Dug(ex);
			throw ex;
		}

		return hr;
	}


	// TranslateMessage
	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern bool TranslateMessage([In][Out] ref MSG msg);

	// WrapComCall
	public static int WrapComCall(int hr)
	{
		if (Cmd.Failed(hr))
			throw Marshal.GetExceptionForHR(hr);

		return hr;
	}

	// WrapComCall
	public static int WrapComCall(int hr, params int[] expectedFailures)
	{
		if (Cmd.Failed(hr) && (expectedFailures == null || Array.IndexOf(expectedFailures, hr) < 0))
		{
			throw Marshal.GetExceptionForHR(hr);
		}

		return hr;
	}



	#endregion Static Methods


}
