#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;

using BlackbirdSql.Shared;

namespace BlackbirdSql.Shared.Controls.Grid;


public sealed class GridDragImageList : IDisposable
{
	private IntPtr handle = (IntPtr)0;

	private bool bOwnHandle;

	private const int C_ILC_MASK = 1;

	// private const int C_ILC_COLOR = 0;

	// private const int C_ILC_COLORDDB = 254;

	private const int C_ILC_COLOR4 = 4;

	// private const int C_ILC_COLOR8 = 8;

	// private const int C_ILC_COLOR16 = 16;

	// private const int C_ILC_COLOR24 = 24;

	public IntPtr Handle
	{
		get
		{
			return handle;
		}
		set
		{
			DetachHandle();
			handle = value;
		}
	}

	public GridDragImageList()
	{
	}

	public GridDragImageList(IntPtr handleIL)
	{
		handle = handleIL;
		bOwnHandle = false;
	}

	public GridDragImageList(int imageWidth, int imageHeigh, int flags, int initialCount, int growCount)
	{
		Handle = ImageList_Create(imageWidth, imageHeigh, flags, initialCount, growCount);
		bOwnHandle = true;
	}

	public GridDragImageList(int imageWidth, int imageHeigh)
	{
		Handle = ImageList_Create(imageWidth, imageHeigh, 25, C_ILC_MASK, C_ILC_COLOR4);
		bOwnHandle = true;
	}

	~GridDragImageList()
	{
		DetachHandle();
	}

	public void Dispose()
	{
		DetachHandle();
		GC.SuppressFinalize(this);
	}

	public void Add(Bitmap bitmapImage, Color colorTransparent)
	{
		bitmapImage = (Bitmap)bitmapImage.Clone();
		try
		{
			bitmapImage.MakeTransparent(colorTransparent);
			IntPtr intPtr = ControlPaint.CreateHBitmapTransparencyMask(bitmapImage);
			IntPtr intPtr2 = ControlPaint.CreateHBitmapColorMask(bitmapImage, intPtr);
			ImageList_Add(Handle, intPtr2, intPtr);
			SafeNative.DeleteObject(intPtr2);
			SafeNative.DeleteObject(intPtr);
			GC.KeepAlive(this);
		}
		finally
		{
			bitmapImage.Dispose();
		}
	}

	public static void EndDrag()
	{
		ImageList_EndDrag();
	}

	public bool BeginDrag(int iImage, int dxHotspot, int dyHotspot)
	{
		bool result = ImageList_BeginDrag(handle, iImage, dxHotspot, dyHotspot);
		GC.KeepAlive(this);
		return result;
	}

	public static bool DragEnter(IntPtr hwndLock, Point pt)
	{
		return ImageList_DragEnter(hwndLock, pt.X, pt.Y);
	}

	public static bool DragLeave(IntPtr hwndLock)
	{
		return ImageList_DragLeave(hwndLock);
	}

	public static bool DragMove(Point pt)
	{
		return ImageList_DragMove(pt.X, pt.Y);
	}

	public static bool DragShowNolock(bool bShow)
	{
		return ImageList_DragShowNolock(bShow);
	}

	private void DetachHandle()
	{
		IntPtr intPtr = handle;
		bool num = bOwnHandle;
		handle = (IntPtr)0;
		bOwnHandle = false;
		if (num && intPtr != (IntPtr)0)
		{
			ImageList_Destroy(intPtr);
		}
	}

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern IntPtr ImageList_Create(int cx, int cy, int flags, int cInitial, int cGrow);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern int ImageList_Add(IntPtr himl, IntPtr hbmImage, IntPtr hbmMask);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern bool ImageList_Destroy(IntPtr himl);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern bool ImageList_BeginDrag(IntPtr himlTrack, int iTrack, int dxHotspot, int dyHotspot);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern void ImageList_EndDrag();

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern bool ImageList_DragEnter(IntPtr hwndLock, int x, int y);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern bool ImageList_DragLeave(IntPtr hwndLock);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern bool ImageList_DragMove(int x, int y);

	[DllImport("comctl32.dll", CharSet = CharSet.Auto)]
	private static extern bool ImageList_DragShowNolock(bool bShow);
}
