using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using BlackbirdSql.Shared.Events;

using OLERECT = Microsoft.VisualStudio.OLE.Interop.RECT;
using Point = System.Windows.Point;
using Size = System.Windows.Size;



namespace BlackbirdSql.Shared;


// =========================================================================================================
//											Native Class
//
/// <summary>
/// Central location for accessing of native members. 
/// </summary>
// =========================================================================================================
public abstract class Native : BlackbirdSql.Sys.Native
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
	#region Static Methods - Native
	// =========================================================================================================


	// ColorFromRGB
	public static Color ColorFromRGB(uint colorRef)
	{
		return Color.FromArgb(RGB_GETRED(colorRef), RGB_GETGREEN(colorRef), RGB_GETBLUE(colorRef));
	}


	// CreateBitmap
	public static IntPtr CreateBitmapManaged(int nWidth, int nHeight, int nPlanes, int nBitsPerPixel, short[] lpvBits)
	{
		return HandleCollector.Add(CreateBitmap(nWidth, nHeight, nPlanes, nBitsPerPixel, lpvBits), CommonHandles.GDI);
	}



	// CreateBrushIndirect
	public static IntPtr CreateBrushIndirectManaged(LOGBRUSHEx lb)
	{
		return HandleCollector.Add(CreateBrushIndirect(lb), CommonHandles.GDI);
	}


	// DeleteObject
	public static bool DeleteObjectManaged(IntPtr hObject)
	{
		HandleCollector.Remove(hObject, CommonHandles.GDI);
		return DeleteObject(hObject);
	}


	// GetClientRect
	public static bool GetClientRect(IntPtr hwnd, out UIRECTEx lpRect)
	{
		bool result = GetClientRect(hwnd, out OLERECT lpOleRect);

		lpRect = new(lpOleRect);

		return result;
	}



	// GetDCEx
	public static IntPtr GetDCExManaged(IntPtr hWnd, IntPtr hrgnClip, int flags)
	{
		return HandleCollector.Add(GetDCEx(hWnd, hrgnClip, flags), CommonHandles.HDC);
	}


	// GetWindowLongPtrPtr
	public static IntPtr GetWindowLongPtrPtr(IntPtr hWnd, int nIndex)
	{
		if (IntPtr.Size == 4)
			return GetWindowLong(hWnd, nIndex);

		return GetWindowLongPtr(hWnd, nIndex);
	}



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


	#endregion Static Methods





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




			// A private 'this' object lock
			private readonly object _LockLocal = new object();

			private int handleCount;


			public void Add(IntPtr handle)
			{
				if (handle == IntPtr.Zero)
				{
					return;
				}
				bool flag = false;
				int currentHandleCount = 0;
				lock (_LockLocal)
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
				lock (_LockLocal)
					return handleCount;
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
				lock (_LockLocal)
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


	[Serializable]
	public struct UIRECTEx
	{
		// Microsoft.SqlServer.ConnectionDlg.UI.WPF.PlatformUI.RECT

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Left;

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Top;

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Right;

		[ComAliasName("Microsoft.VisualStudio.OLE.Interop.LONG")]
		public int Bottom;

		public readonly Point Position => new Point(Left, Top);

		public System.Windows.Size Size => new Size(Width, Height);

		public int Height
		{
			readonly get
			{
				return Bottom - Top;
			}
			set
			{
				Bottom = Top + value;
			}
		}

		public int Width
		{
#pragma warning disable IDE0251 // Make member 'readonly'
			get
			{
				return Right - Left;
			}
#pragma warning restore IDE0251 // Make member 'readonly'
			set
			{
				Right = Left + value;
			}
		}

		public UIRECTEx(int left, int top, int right, int bottom)
		{
			Left = left;
			Top = top;
			Right = right;
			Bottom = bottom;
		}

		public UIRECTEx(Rect rect)
		{
			Left = (int)rect.Left;
			Top = (int)rect.Top;
			Right = (int)rect.Right;
			Bottom = (int)rect.Bottom;
		}

		public UIRECTEx(Microsoft.VisualStudio.OLE.Interop.RECT rect)
		{
			Left = rect.left;
			Top = rect.top;
			Right = rect.right;
			Bottom = rect.bottom;
		}

		public void Offset(int dx, int dy)
		{
			Left += dx;
			Right += dx;
			Top += dy;
			Bottom += dy;
		}

		public Int32Rect ToInt32Rect()
		{
			return new Int32Rect(Left, Top, Width, Height);
		}
	}




	public class Util
	{
		public static int LOWORD(IntPtr n)
		{
			return (int)((long)n & 0xFFFF);
		}

		public static IntPtr MAKELPARAM(int low, int high)
		{
			return (IntPtr)((high << 16) | (low & 0xFFFF));
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

}
