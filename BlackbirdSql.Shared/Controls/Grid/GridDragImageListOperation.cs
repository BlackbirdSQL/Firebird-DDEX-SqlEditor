// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.GridDragImageListOperation

using System;
using System.Drawing;



namespace BlackbirdSql.Shared.Controls.Grid;


public sealed class GridDragImageListOperation : IDisposable
{
	private IntPtr handleWnd = (IntPtr)(-1);

	private GridDragImageList ownedDIL;

	public GridDragImageListOperation(GridDragImageList dil, Point ptGrab, IntPtr hwnd, Point ptStart)
	{
		CommonConstruct(dil, ptGrab, hwnd, ptStart);
	}

	public GridDragImageListOperation(GridDragImageList dil, Point ptGrab, IntPtr hwnd, Point ptStart, bool bOwnDIL)
	{
		CommonConstruct(dil, ptGrab, hwnd, ptStart);
		if (bOwnDIL)
		{
			ownedDIL = dil;
		}
	}

	public GridDragImageListOperation(GridDragImageList dil, Point ptGrab, Point ptStart)
	{
		CommonConstruct(dil, ptGrab, (IntPtr)0, ptStart);
	}

	public void Dispose()
	{
		if (handleWnd != (IntPtr)(-1))
		{
			GridDragImageList.DragLeave(handleWnd);
			GridDragImageList.EndDrag();
		}

		if (ownedDIL != null)
		{
			ownedDIL.Dispose();
			ownedDIL = null;
		}
	}

	private void CommonConstruct(GridDragImageList dil, Point ptGrab, IntPtr hwnd, Point ptStart)
	{
		handleWnd = hwnd;
		dil.BeginDrag(0, ptGrab.X, ptGrab.Y);
		GridDragImageList.DragEnter(hwnd, ptStart);
	}
}
