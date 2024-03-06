#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.ComponentModel;
using System.Drawing;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Ctl.Interfaces;





// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Controls.Widgets;


public class QESplitter : Control, IMessageFilter
{
	private class CaptureTracker
	{
		public Point LastCapturedMousePos = new Point(-1, -1);

		public int DragOffset;

		public bool Capturing;

		public void Reset()
		{
			DragOffset = 0;
			LastCapturedMousePos.X = -1;
			LastCapturedMousePos.Y = -1;
			Capturing = false;
		}
	}

	private const int C_DRAW_START = 1;

	private const int C_DRAW_MOVE = 2;

	private const int C_DRAW_END = 3;

	private readonly int splitterWidth = 3;

	private BorderStyle borderStyle;

	private static readonly object EVENT_MOVED = new object();

	private bool horizontalSplitter = true;

	private Control boundControl;

	private readonly int boundControlMinSize = 10;

	private readonly CaptureTracker captTracker = new CaptureTracker();

	public bool HorizontalSplitter
	{
		get
		{
			return horizontalSplitter;
		}
		set
		{
			horizontalSplitter = value;

			Cursor = SplitterCursor;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override bool AllowDrop
	{
		get
		{
			return base.AllowDrop;
		}
		set
		{
			base.AllowDrop = value;
		}
	}

	[DefaultValue(BorderStyle.None)]
	public BorderStyle BorderStyle
	{
		get
		{
			return borderStyle;
		}
		set
		{
			if (!Enum.IsDefined(typeof(BorderStyle), value))
			{
				InvalidEnumArgumentException ex = new("value", (int)value, typeof(BorderStyle));
				Diag.Dug(ex);
				throw ex;
			}

			if (borderStyle != value)
			{
				borderStyle = value;
				UpdateStyles();
			}
		}
	}

	protected override Size DefaultSize => new Size(splitterWidth, splitterWidth);

	protected override CreateParams CreateParams
	{
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle &= -513;
			createParams.Style &= -8388609;
			switch (borderStyle)
			{
				case BorderStyle.Fixed3D:
					createParams.ExStyle |= 512;
					break;
				case BorderStyle.FixedSingle:
					createParams.Style |= 8388608;
					break;
			}

			return createParams;
		}
	}

	protected override ImeMode DefaultImeMode => ImeMode.Disable;

	private Cursor SplitterCursor
	{
		get
		{
			if (!horizontalSplitter)
			{
				return Cursors.VSplit;
			}

			return Cursors.HSplit;
		}
	}

	public event QESplitterMovedEventHandler SplitterMovedEvent
	{
		add
		{
			Events.AddHandler(EVENT_MOVED, value);
		}
		remove
		{
			Events.RemoveHandler(EVENT_MOVED, value);
		}
	}

	public QESplitter(Control boundControl, int boundControlMinSize)
	{
		SetStyle(ControlStyles.Selectable, value: false);
		TabStop = false;

		Cursor = SplitterCursor;

		this.boundControl = boundControl;
		this.boundControlMinSize = boundControlMinSize;
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			boundControl = null;
		}

		base.Dispose(disposing);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (e.KeyCode == Keys.Escape)
		{
			SplitEnd(accept: false);
		}
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		base.OnMouseDown(e);
		if (e.Button == MouseButtons.Left && e.Clicks == 1)
		{
			SplitBegin(e.X, e.Y);
		}
		else
		{
			Capture = false;
		}
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (Capture)
		{
			SplitMove(e.X, e.Y);
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (captTracker.Capturing)
		{
			SplitEnd(accept: true);
		}
	}

	[SecuritySafeCritical]
	[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	public bool PreFilterMessage(ref Message m)
	{
		if (m.Msg >= Core.Native.WM_KEYFIRST && m.Msg <= Core.Native.WM_KEYLAST)
		{
			if (m.Msg == Core.Native.WM_KEYFIRST && (int)m.WParam == 27)
			{
				SplitEnd(accept: false);
			}

			return true;
		}

		return false;
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if (horizontalSplitter)
		{
			if (width < 1)
			{
				width = 3;
			}
		}
		else if (height < 1)
		{
			height = 3;
		}

		base.SetBoundsCore(x, y, width, height, specified);
	}

	protected virtual void OnSplitterMoved(QESplitterMovedEventArgs sevent)
	{
		((QESplitterMovedEventHandler)Events[EVENT_MOVED])?.Invoke(this, sevent);
	}

	private void DrawSplitBar(int mode, Point currentMousePoint)
	{
		if (mode != 1)
		{
			DrawSplitHelper(captTracker.LastCapturedMousePos);
		}

		if (mode != 3)
		{
			DrawSplitHelper(currentMousePoint);
			captTracker.LastCapturedMousePos = currentMousePoint;
		}
	}

	private Rectangle CalcSplitRectangle(Point currentMousePoint)
	{
		if (horizontalSplitter)
		{
			return new Rectangle(0, currentMousePoint.Y - captTracker.DragOffset, Parent.ClientRectangle.Width, splitterWidth);
		}

		return new Rectangle(currentMousePoint.X - captTracker.DragOffset, 0, splitterWidth, Parent.ClientRectangle.Height);
	}

	private void DrawSplitHelper(Point currentMousePoint)
	{
		Rectangle rectangle = CalcSplitRectangle(currentMousePoint);
		IntPtr handle = Parent.Handle;
		IntPtr dCEx = Native.GetDCEx(handle, IntPtr.Zero, 1026);
		IntPtr intPtr = CreateHalftoneHBRUSH();
		IntPtr intPtr2 = Native.SelectObject(dCEx, intPtr);
		Native.PatBlt(dCEx, rectangle.X, rectangle.Y, rectangle.Width, rectangle.Height, 5898313);
		Native.SelectObject(dCEx, intPtr2);
		Native.DeleteObject(intPtr);
		Native.ReleaseDC(handle, dCEx);
	}

	private void SplitBegin(int x, int y)
	{
		captTracker.Reset();
		captTracker.Capturing = true;
		if (horizontalSplitter)
		{
			captTracker.DragOffset = y;
		}
		else
		{
			captTracker.DragOffset = x;
		}

		Point p = PointToScreen(new Point(x, y));
		p = Parent.PointToClient(p);
		Application.AddMessageFilter(this);
		Capture = true;
		DrawSplitBar(C_DRAW_START, p);
	}

	private void SplitMove(int x, int y)
	{
		Point p = PointToScreen(new Point(x, y));
		p = Parent.PointToClient(p);
		if (horizontalSplitter)
		{
			if (p.Y - captTracker.DragOffset - boundControl.Bounds.Top < boundControlMinSize)
			{
				p.Y = boundControl.Bounds.Top + boundControlMinSize + captTracker.DragOffset;
			}
		}
		else if (p.X - boundControl.Bounds.Left - captTracker.DragOffset < boundControlMinSize)
		{
			p.X = boundControl.Bounds.Left + boundControlMinSize + captTracker.DragOffset;
		}

		DrawSplitBar(C_DRAW_MOVE, p);
	}

	private void SplitEnd(bool accept)
	{
		try
		{
			DrawSplitBar(C_DRAW_END, Point.Empty);
			Capture = false;
			Application.RemoveMessageFilter(this);
			if (accept)
			{
				OnSplitterMoved(new QESplitterMovedEventArgs(horizontalSplitter ? captTracker.LastCapturedMousePos.Y - captTracker.DragOffset : captTracker.LastCapturedMousePos.X - captTracker.DragOffset, boundControl));
			}
		}
		finally
		{
			captTracker.Reset();
		}
	}

	public static IntPtr CreateHalftoneHBRUSH()
	{
		short[] array = new short[8];
		for (int i = 0; i < 8; i++)
		{
			array[i] = (short)(21845 << (i & 1));
		}

		IntPtr intPtr = Native.CreateBitmap(8, 8, 1, 1, array);
		IntPtr result = Native.CreateBrushIndirect(new Native.LOGBRUSH
		{
			lbColor = ColorTranslator.ToWin32(Color.Black),
			lbStyle = 3,
			lbHatch = intPtr
		});
		Native.DeleteObject(intPtr);
		return result;
	}
}
