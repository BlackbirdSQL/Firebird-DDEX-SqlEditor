using System;
using System.Collections.Generic;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Automation.Provider;
using BlackbirdSql.Common.Ctl.Events;
using BlackbirdSql.Common.Ctl.Structs;

using OLERECT = Microsoft.VisualStudio.OLE.Interop.RECT;


namespace BlackbirdSql.Common;

// =========================================================================================================
//											Native Class
//
/// <summary>
/// Central location for accessing of native members. 
/// </summary>
// =========================================================================================================

public abstract class Native : BlackbirdSql.Core.Native
{


	// ---------------------------------------------------------------------------------
	#region Constants and Static Fields - Native
	// ---------------------------------------------------------------------------------

	public const uint COLORREF_WHITE = 0xFFFFFFu;
	public const uint COLORREF_AUTO = 0x2000000u;

	/// <summary>
	/// User interface element reference identifier defined in oleacc.h.
	/// </summary>
	public static readonly Guid IID_IAccessible = new("618736E0-3C3D-11CF-810C-00AA00389B71");


	#endregion Constants and Static Fields





	// =========================================================================================================
	#region Enums - Native
	// =========================================================================================================


	public enum EnNotificationKind
	{
		NotificationKind_ItemAdded,
		NotificationKind_ItemRemoved,
		NotificationKind_ActionCompleted,
		NotificationKind_ActionAborted,
		NotificationKind_Other
	}


	public enum EnNotificationProcessing
	{
		NotificationProcessing_ImportantAll,
		NotificationProcessing_ImportantMostRecent,
		NotificationProcessing_All,
		NotificationProcessing_MostRecent,
		NotificationProcessing_CurrentThenMostRecent
	}


	#endregion Enums





	// =========================================================================================================
	#region Internal Classes and Structs - Native
	// =========================================================================================================


	private sealed class CommonHandles
	{
		public static readonly int Accelerator = HandleCollector.RegisterType("Accelerator", 80, 50);

		public static readonly int Cursor = HandleCollector.RegisterType("Cursor", 20, 500);

		public static readonly int EMF = HandleCollector.RegisterType("EnhancedMetaFile", 20, 500);

		public static readonly int Find = HandleCollector.RegisterType("Find", 0, 1000);

		public static readonly int GDI = HandleCollector.RegisterType("GDI", 50, 500);

		public static readonly int HDC = HandleCollector.RegisterType("HDC", 100, 2);

		public static readonly int CompatibleHDC = HandleCollector.RegisterType("ComptibleHDC", 50, 50);

		public static readonly int Icon = HandleCollector.RegisterType("Icon", 20, 500);

		public static readonly int Kernel = HandleCollector.RegisterType("Kernel", 0, 1000);

		public static readonly int Menu = HandleCollector.RegisterType("Menu", 30, 1000);

		public static readonly int Window = HandleCollector.RegisterType("Window", 5, 1000);
	}

	private sealed class HandleCollector
	{
		private class HandleType(string name, int expense, int initialThreshHold)
		{
			public readonly string name = name;

			private readonly int deltaPercent = 100 - expense;

			private readonly int initialThreshHold = initialThreshHold;

			private int threshHold = initialThreshHold;




			private int handleCount;


			public void Add(IntPtr handle)
			{
				if (handle == IntPtr.Zero)
				{
					return;
				}
				bool flag = false;
				int currentHandleCount = 0;
				lock (this)
				{
					handleCount++;
					flag = NeedCollection();
					currentHandleCount = handleCount;
				}
				lock (_LockGlobal)
				{
					HandleAddedEvent?.Invoke(name, handle, currentHandleCount);
				}
				if (flag && flag)
				{
					GC.Collect();
					Thread.Sleep((100 - deltaPercent) / 4);
				}
			}

			public int GetHandleCount()
			{
				lock (this)
				{
					return handleCount;
				}
			}

			public bool NeedCollection()
			{
				if (suspendCount > 0)
				{
					return false;
				}
				if (handleCount > threshHold)
				{
					threshHold = handleCount + handleCount * deltaPercent / 100;
					return true;
				}
				int num = 100 * threshHold / (100 + deltaPercent);
				if (num >= initialThreshHold && handleCount < (int)((float)num * 0.9f))
				{
					threshHold = num;
				}
				return false;
			}

			public IntPtr Remove(IntPtr handle)
			{
				if (handle == IntPtr.Zero)
				{
					return handle;
				}
				int currentHandleCount = 0;
				lock (this)
				{
					handleCount--;
					if (handleCount < 0)
					{
						handleCount = 0;
					}
					currentHandleCount = handleCount;
				}
				lock (_LockGlobal)
				{
					HandleRemovedEvent?.Invoke(name, handle, currentHandleCount);
				}
				return handle;
			}
		}

		private static HandleType[] handleTypes;

		private static int handleTypeCount;

		private static int suspendCount;

		// A static class lock
		private static readonly object _LockGlobal = new object();

		public static event HandleChangeEventHandler HandleAddedEvent;

		public static event HandleChangeEventHandler HandleRemovedEvent;

		public static IntPtr Add(IntPtr handle, int type)
		{
			handleTypes[type - 1].Add(handle);
			return handle;
		}

		public static void SuspendCollect()
		{
			lock (_LockGlobal)
			{
				suspendCount++;
			}
		}

		public static void ResumeCollect()
		{
			bool flag = false;
			lock (_LockGlobal)
			{
				if (suspendCount > 0)
				{
					suspendCount--;
				}
				if (suspendCount == 0)
				{
					for (int i = 0; i < handleTypeCount; i++)
					{
						lock (handleTypes[i])
						{
							if (handleTypes[i].NeedCollection())
							{
								flag = true;
							}
						}
					}
				}
			}
			if (flag)
			{
				GC.Collect();
			}
		}

		public static int RegisterType(string typeName, int expense, int initialThreshold)
		{
			lock (_LockGlobal)
			{
				if (handleTypeCount == 0 || handleTypeCount == handleTypes.Length)
				{
					HandleType[] destinationArray = new HandleType[handleTypeCount + 10];
					if (handleTypes != null)
					{
						Array.Copy(handleTypes, 0, destinationArray, 0, handleTypeCount);
					}
					handleTypes = destinationArray;
				}
				handleTypes[handleTypeCount++] = new HandleType(typeName, expense, initialThreshold);
				return handleTypeCount;
			}
		}

		public static IntPtr Remove(IntPtr handle, int type)
		{
			return handleTypes[type - 1].Remove(handle);
		}
	}


	[StructLayout(LayoutKind.Sequential)]
	public class COMRECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public COMRECT()
		{
		}

		public COMRECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}
	}



	[StructLayout(LayoutKind.Sequential)]
	public class HELPINFO
	{
		public int cbSize = Marshal.SizeOf(typeof(HELPINFO));

		public int iContextType;

		public int iCtrlId;

		public IntPtr hItemHandle;

		public int dwContextId;

		public POINT MousePos;
	}



	public class LOGBRUSH
	{
		public int lbStyle;

		public int lbColor;

		public IntPtr lbHatch;
	}



	[StructLayout(LayoutKind.Sequential)]
	public class POINT
	{
		public int x;

		public int y;

		public POINT()
		{
		}

		public POINT(int x, int y)
		{
			this.x = x;
			this.y = y;
		}
	}



	public struct RECT
	{
		public int left;

		public int top;

		public int right;

		public int bottom;

		public RECT(int left, int top, int right, int bottom)
		{
			this.left = left;
			this.top = top;
			this.right = right;
			this.bottom = bottom;
		}

		public RECT(OLERECT rect)
		{
			left = rect.left;
			top = rect.top;
			right = rect.right;
			bottom = rect.bottom;
		}

		public static RECT FromXYWH(int x, int y, int width, int height)
		{
			return new RECT(x, y, x + width, y + height);
		}
	}






	public class Util
	{
		public static IntPtr MAKELPARAM(int low, int high)
		{
			return (IntPtr)((high << 16) | (low & 0xFFFF));
		}

		public static int LOWORD(IntPtr n)
		{
			return (int)((long)n & 0xFFFF);
		}

		public static int RGB_GETRED(int color)
		{
			return color & 0xFF;
		}

		public static int RGB_GETGREEN(int color)
		{
			return (color >> 8) & 0xFF;
		}

		public static int RGB_GETBLUE(int color)
		{
			return (color >> 16) & 0xFF;
		}
	}


	#endregion #region Internal Classes and Structs





	// =========================================================================================================
	#region Events - Native
	// =========================================================================================================


	[return: MarshalAs(UnmanagedType.Bool)]
	public delegate bool EnumMonitorsDelegate(IntPtr hMonitor, IntPtr hdcMonitor, ref UIRECT lprcMonitor, IntPtr dwData);


	#endregion Events





	// =========================================================================================================
	#region Static Methods - Native
	// =========================================================================================================


	// ColorFromRGB
	public static Color ColorFromRGB(uint colorRef)
	{
		return Color.FromArgb(RGB_GETRED(colorRef), RGB_GETGREEN(colorRef), RGB_GETBLUE(colorRef));
	}


	// CreateBitmap
	public static IntPtr CreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, short[] lpvBits)
	{
		return HandleCollector.Add(IntCreateBitmap(nWidth, nHeight, nPlanes, nBitsPerPixel, lpvBits), CommonHandles.GDI);
	}



	// CreateBrushIndirect
	public static IntPtr CreateBrushIndirect(LOGBRUSH lb)
	{
		return HandleCollector.Add(IntCreateBrushIndirect(lb), CommonHandles.GDI);
	}



	// CreatePen
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr CreatePen(int nStyle, int nWidth, int crColor);



	// CreateStdAccessibleObject
	[DllImport("oleacc.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int CreateStdAccessibleObject(IntPtr hWnd, int objID, ref Guid refiid, [In][Out][MarshalAs(UnmanagedType.Interface)] ref object pAcc);



	// DeleteObject
	[DllImport("gdi32.dll", CharSet = CharSet.Auto)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool DeleteObject(HandleRef hObject);

	// DeleteObject
	public static bool DeleteObject(IntPtr hObject)
	{
		HandleCollector.Remove(hObject, CommonHandles.GDI);
		return IntDeleteObject(hObject);
	}



	// EnumDisplayMonitors
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool EnumDisplayMonitors(IntPtr hdc, IntPtr lprcClip, EnumMonitorsDelegate lpfnEnum, IntPtr dwData);



	public static void FindMaximumSingleMonitorRectangle(UIRECT windowRect, out UIRECT screenSubRect, out UIRECT monitorRect)
	{
		List<UIRECT> rects = [];
		EnumDisplayMonitors(IntPtr.Zero, IntPtr.Zero, delegate (IntPtr hMonitor, IntPtr hdcMonitor, ref UIRECT rect, IntPtr lpData)
		{
			MONITORINFO monitorInfo = default;
			monitorInfo.cbSize = (uint)Marshal.SizeOf(typeof(MONITORINFO));
			GetMonitorInfo(hMonitor, ref monitorInfo);
			rects.Add(monitorInfo.rcWork);
			return true;
		}, IntPtr.Zero);
		long num = 0L;
		UIRECT rECT = new UIRECT
		{
			Left = 0,
			Right = 0,
			Top = 0,
			Bottom = 0
		};
		screenSubRect = rECT;
		rECT = new UIRECT
		{
			Left = 0,
			Right = 0,
			Top = 0,
			Bottom = 0
		};
		monitorRect = rECT;
		foreach (UIRECT item in rects)
		{
			UIRECT lprcSrc = item;
			IntersectRect(out var lprcDst, ref lprcSrc, ref windowRect);
			long num2 = lprcDst.Width * lprcDst.Height;
			if (num2 > num)
			{
				screenSubRect = lprcDst;
				monitorRect = item;
				num = num2;
			}
		}
	}



	// GetClientRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetClientRect(IntPtr hwnd, out OLERECT lpRect);

	// GetClientRect
	public static bool GetClientRect(IntPtr hwnd, out UIRECT lpRect)
	{
		bool result = GetClientRect(hwnd, out OLERECT lpOleRect);

		lpRect = new(lpOleRect);

		return result;
	}



	// GetDCEx
	public static IntPtr GetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags)
	{
		return HandleCollector.Add(IntGetDCEx(hWnd, hrgnClip, flags), CommonHandles.HDC);
	}



	// GetFocus
	[DllImport("user32.dll")]
	public static extern IntPtr GetFocus();


	// GetMonitorInfo
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetMonitorInfo(IntPtr hMonitor, ref MONITORINFO monitorInfo);



	// GetSysColor
	[DllImport("user32", CharSet = CharSet.Auto)]
	public static extern uint GetSysColor(int sysColor);


	// GetWindow
	[DllImport("user32.dll")]
	public static extern IntPtr GetWindow(IntPtr hwnd, int nCmd);



	// GetWindowLongInt
	[DllImport("user32.dll")]
	public static extern int GetWindowLongInt(IntPtr hWnd, int nIndex);


	// GetWindowLongPtr32
	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLong")]
	public static extern IntPtr GetWindowLongPtr32(IntPtr hWnd, int nIndex);

	// GetWindowLongPtr64
	[DllImport("user32.dll", CharSet = CharSet.Auto, EntryPoint = "GetWindowLongPtr")]
	public static extern IntPtr GetWindowLongPtr64(IntPtr hWnd, int nIndex);

	// GetWindowLongPtrPtr
	public static IntPtr GetWindowLongPtrPtr(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 4)
			return GetWindowLongPtr32(hWnd, nIndex);

		return GetWindowLongPtr64(hWnd, nIndex);
	}



	// GetWindowRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hwnd, out RECT lpRect);

	// GetWindowRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hwnd, out OLERECT lpRect);

	// GetWindowRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool GetWindowRect(IntPtr hwnd, out UIRECT lpRect);



	// HIWORD
	public static short HIWORD(uint dwValue)
	{
		return (short)(dwValue >> 16 & 0xFFFF);
	}



	// IntCreateBitmap
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "CreateBitmap", ExactSpelling = true)]
	private static extern IntPtr IntCreateBitmap(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, short[] lpvBits);



	// IntCreateBrushIndirect
	[DllImport("gdi32", CharSet = CharSet.Auto, EntryPoint = "CreateBrushIndirect", ExactSpelling = true)]
	private static extern IntPtr IntCreateBrushIndirect(LOGBRUSH lb);



	// IntDeleteObject
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, EntryPoint = "DeleteObject", ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	private static extern bool IntDeleteObject(IntPtr hObject);



	// IntersectRect
	[DllImport("user32.dll")]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IntersectRect(out UIRECT lprcDst, [In] ref UIRECT lprcSrc1, [In] ref UIRECT lprcSrc2);



	// IntGetDCEx
	[DllImport("user32", CharSet = CharSet.Auto, EntryPoint = "GetDCEx", ExactSpelling = true)]
	private static extern IntPtr IntGetDCEx(IntPtr hWnd, IntPtr hrgnClip, int flags);



	// IntPtrToObjectArray
	public static object[] IntPtrToObjectArray(IntPtr ptr)
	{
		object[] result = null;
		if (ptr != IntPtr.Zero)
		{
			object objectForNativeVariant = Marshal.GetObjectForNativeVariant(ptr);
			result = ((objectForNativeVariant is Array) ? ((object[])objectForNativeVariant) : [objectForNativeVariant]);
		}

		return result;
	}



	// IsWindow
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool IsWindow(IntPtr hWnd);


	// LineTo
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool LineTo(HandleRef hdc, int x, int y);



	// LOWORD
	public static short LOWORD(uint dwValue)
	{
		return (short)(dwValue & 0xFFFF);
	}



	// MoveToEx
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern bool MoveToEx(HandleRef hdc, int x, int y, POINT pt);



	// PatBlt
	[DllImport("gdi32", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool PatBlt(IntPtr hdc, int left, int top, int width, int height, int rop);



	// ReleaseDC
	[DllImport("User32.dll", CallingConvention = CallingConvention.StdCall, ExactSpelling = true)]
	public static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

	// ReleaseDC
	public static int ReleaseDC(HandleRef hWnd, HandleRef hDC)
	{
		return ReleaseDC(hWnd.Handle, hDC.Handle);
	}


	// RGB_GETBLUE
	public static int RGB_GETBLUE(uint color)
	{
		return (int)((color >> 16) & 0xFF);
	}

	// RGB_GETGREEN
	public static int RGB_GETGREEN(uint color)
	{
		return (int)((color >> 8) & 0xFF);
	}

	// RGB_GETRED
	public static int RGB_GETRED(uint color)
	{
		return (int)(color & 0xFF);
	}


	// SelectObject
	[DllImport("gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr SelectObject(IntPtr hDC, IntPtr hObject);

	// SelectObject
	[DllImport("Gdi32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern IntPtr SelectObject(HandleRef hDC, HandleRef hObject);



	// SetFocus
	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern IntPtr SetFocus(IntPtr hWnd);



	// SetScrollInfo
	[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
	public static extern int SetScrollInfo(IntPtr hWnd, int fnBar, SCROLLINFO si, bool redraw);


	// SetWindowLong
	[DllImport("user32.dll", CharSet = CharSet.Unicode)]
	public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);



	// SetWindowPos
	[DllImport("User32", CharSet = CharSet.Auto, ExactSpelling = true)]
	[return: MarshalAs(UnmanagedType.Bool)]
	public static extern bool SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int y, int cx, int cy, int flags);



	// UiaRaiseNotificationEvent
	[DllImport("UIAutomationCore.dll", CharSet = CharSet.Unicode)]
	public static extern int UiaRaiseNotificationEvent(IRawElementProviderSimple provider, EnNotificationKind notificationKind, EnNotificationProcessing notificationProcessing, string notificationText, string notificationId);


	#endregion Static Methods











	// [DllImport("ole32.dll", PreserveSig = false)]
	// public static extern IStream CreateStreamOnHGlobal(IntPtr hGlobal, [MarshalAs(UnmanagedType.Bool)] bool fDeleteOnRelease);
}
