// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.Splitter
using System;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Security;
using System.Security.Permissions;
using System.Windows.Forms;

namespace BlackbirdSql.Shared.Controls.Widgets;


public class Splitter : Control
{
	private class SplitRenderer : Form
	{
		public SplitRenderer()
		{
			ShowInTaskbar = false;
			FormBorderStyle = FormBorderStyle.None;
			Opacity = 0.5;
			BackColor = Color.Black;
		}
	}

	private class SplitData
	{
		public int dockWidth = -1;

		public int dockHeight = -1;

		public Control target;
	}

	private class SplitterMessageFilter(Splitter splitter) : IMessageFilter
	{
		private readonly Splitter _Owner = splitter;

		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		public bool PreFilterMessage(ref Message m)
		{
			if (m.Msg >= Native.WM_KEYFIRST && m.Msg <= Native.WM_KEYLAST)
			{
				if (m.Msg == Native.WM_KEYFIRST && (int)(long)m.WParam == 27)
				{
					_Owner.SplitEnd(accept: false);
				}
				return true;
			}
			return false;
		}
	}

	private const int C_DRAW_START = 1;

	private const int C_DRAW_MOVE = 2;

	private const int C_DRAW_END = 3;

	private const int C_DefaultWidth = 3;

	private BorderStyle _BorderStyle;

	private int _MinSize = 25;

	private int _MinExtra = 25;

	private Point _Anchor = Point.Empty;

	private Control _SplitTarget;

	private int _SplitSize = -1;

	private int _SplitterThickness = 3;

	private int _InitTargetSize;

	private int _LastDrawSplit = -1;

	private int _MaxSize;

	private static readonly object S_EVENT_MOVING = new object();

	private static readonly object S_EVENT_MOVED = new object();

	private SplitterMessageFilter _SplitterMessageFilter;

	private const int C_WS_EX_CLIENTEDGE = 512;

	private SplitRenderer _Renderer;

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[DefaultValue(AnchorStyles.None)]
	public override AnchorStyles Anchor
	{
		get
		{
			return AnchorStyles.None;
		}
		set
		{
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

	protected override Size DefaultSize => new Size(C_DefaultWidth, C_DefaultWidth);

	protected override Cursor DefaultCursor
	{
		get
		{
			switch (Dock)
			{
				case DockStyle.Top:
				case DockStyle.Bottom:
					return Cursors.HSplit;
				case DockStyle.Left:
				case DockStyle.Right:
					return Cursors.VSplit;
				default:
					return base.DefaultCursor;
			}
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Color ForeColor
	{
		get
		{
			return base.ForeColor;
		}
		set
		{
			base.ForeColor = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Image BackgroundImage
	{
		get
		{
			return base.BackgroundImage;
		}
		set
		{
			base.BackgroundImage = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override ImageLayout BackgroundImageLayout
	{
		get
		{
			return base.BackgroundImageLayout;
		}
		set
		{
			base.BackgroundImageLayout = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public override Font Font
	{
		get
		{
			return base.Font;
		}
		set
		{
			base.Font = value;
		}
	}

	public BorderStyle BorderStyle
	{
		get
		{
			return _BorderStyle;
		}
		set
		{
			if (_BorderStyle != value)
			{
				_BorderStyle = value;
				UpdateStyles();
			}
		}
	}

	protected override CreateParams CreateParams
	{
		[SecuritySafeCritical]
		[SecurityPermission(SecurityAction.Demand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		get
		{
			CreateParams createParams = base.CreateParams;
			createParams.ExStyle &= -513;
			createParams.Style &= -8388609;
			switch (_BorderStyle)
			{
				case BorderStyle.Fixed3D:
					createParams.ExStyle |= C_WS_EX_CLIENTEDGE;
					break;
				case BorderStyle.FixedSingle:
					createParams.Style |= 8388608;
					break;
			}
			return createParams;
		}
	}

	protected override ImeMode DefaultImeMode => ImeMode.Disable;

	[Localizable(true)]
	[DefaultValue(DockStyle.Left)]
	public override DockStyle Dock
	{
		get
		{
			return base.Dock;
		}
		set
		{
			int num = _SplitterThickness;
			base.Dock = value;
			switch (Dock)
			{
				case DockStyle.Top:
				case DockStyle.Bottom:
					if (_SplitterThickness != -1)
					{
						Height = num;
					}
					break;
				case DockStyle.Left:
				case DockStyle.Right:
					if (_SplitterThickness != -1)
					{
						Width = num;
					}
					break;
			}
		}
	}

	private bool Horizontal
	{
		get
		{
			DockStyle dock = Dock;
			if (dock != DockStyle.Left)
			{
				return dock == DockStyle.Right;
			}
			return true;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new ImeMode ImeMode
	{
		get
		{
			return base.ImeMode;
		}
		set
		{
			base.ImeMode = value;
		}
	}

	public int MinExtra
	{
		get
		{
			return _MinExtra;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			_MinExtra = value;
		}
	}

	public int MinSize
	{
		get
		{
			return _MinSize;
		}
		set
		{
			if (value < 0)
			{
				value = 0;
			}
			_MinSize = value;
		}
	}

	public int SplitPosition
	{
		get
		{
			if (_SplitSize == -1)
			{
				_SplitSize = CalcSplitSize();
			}
			return _SplitSize;
		}
		set
		{
			SplitData splitData = CalcSplitBounds();
			if (value > _MaxSize)
			{
				value = _MaxSize;
			}
			if (value < _MinSize)
			{
				value = _MinSize;
			}
			_SplitSize = value;
			DrawSplitBar(C_DRAW_END);
			if (splitData.target == null)
			{
				_SplitSize = -1;
				return;
			}
			Rectangle bounds = splitData.target.Bounds;
			switch (Dock)
			{
				case DockStyle.Top:
					bounds.Height = value;
					break;
				case DockStyle.Bottom:
					bounds.Y += bounds.Height - _SplitSize;
					bounds.Height = value;
					break;
				case DockStyle.Left:
					bounds.Width = value;
					break;
				case DockStyle.Right:
					bounds.X += bounds.Width - _SplitSize;
					bounds.Width = value;
					break;
			}
			splitData.target.Bounds = bounds;
			Application.DoEvents();
			OnSplitterMoved(new SplitterEventArgs(Left, Top, Left + bounds.Width / 2, Top + bounds.Height / 2));
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new bool TabStop
	{
		get
		{
			return base.TabStop;
		}
		set
		{
			base.TabStop = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	[Bindable(false)]
	[DesignerSerializationVisibility(DesignerSerializationVisibility.Hidden)]
	public override string Text
	{
		get
		{
			return base.Text;
		}
		set
		{
			base.Text = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler ForeColorChanged
	{
		add
		{
			base.ForeColorChanged += value;
		}
		remove
		{
			base.ForeColorChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler BackgroundImageChanged
	{
		add
		{
			base.BackgroundImageChanged += value;
		}
		remove
		{
			base.BackgroundImageChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler BackgroundImageLayoutChanged
	{
		add
		{
			base.BackgroundImageLayoutChanged += value;
		}
		remove
		{
			base.BackgroundImageLayoutChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler FontChanged
	{
		add
		{
			base.FontChanged += value;
		}
		remove
		{
			base.FontChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler ImeModeChanged
	{
		add
		{
			base.ImeModeChanged += value;
		}
		remove
		{
			base.ImeModeChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler TabStopChanged
	{
		add
		{
			base.TabStopChanged += value;
		}
		remove
		{
			base.TabStopChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler TextChanged
	{
		add
		{
			base.TextChanged += value;
		}
		remove
		{
			base.TextChanged -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler Enter
	{
		add
		{
			base.Enter += value;
		}
		remove
		{
			base.Enter -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event KeyEventHandler KeyUp
	{
		add
		{
			base.KeyUp += value;
		}
		remove
		{
			base.KeyUp -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event KeyEventHandler KeyDown
	{
		add
		{
			base.KeyDown += value;
		}
		remove
		{
			base.KeyDown -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event KeyPressEventHandler KeyPress
	{
		add
		{
			base.KeyPress += value;
		}
		remove
		{
			base.KeyPress -= value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Never)]
	public new event EventHandler Leave
	{
		add
		{
			base.Leave += value;
		}
		remove
		{
			base.Leave -= value;
		}
	}

	public event SplitterEventHandler SplitterMovingEvent
	{
		add
		{
			Events.AddHandler(S_EVENT_MOVING, value);
		}
		remove
		{
			Events.RemoveHandler(S_EVENT_MOVING, value);
		}
	}

	public event SplitterEventHandler SplitterMovedEvent
	{
		add
		{
			Events.AddHandler(S_EVENT_MOVED, value);
		}
		remove
		{
			Events.RemoveHandler(S_EVENT_MOVED, value);
		}
	}

	public Splitter()
	{
		SetStyle(ControlStyles.Selectable, value: false);
		TabStop = false;
		_MinSize = 25;
		_MinExtra = 25;
		Dock = DockStyle.Left;
	}

	public void InvalidateSplitPosition()
	{
		_SplitSize = -1;
	}

	private void DrawSplitBar(int mode)
	{
		if (mode != 1 && _LastDrawSplit != -1)
		{
			DrawSplitHelper(_LastDrawSplit);
			_LastDrawSplit = -1;
		}
		else if (mode != 1 && _LastDrawSplit == -1)
		{
			return;
		}
		if (mode != 3)
		{
			DrawSplitHelper(_SplitSize);
			_LastDrawSplit = _SplitSize;
			return;
		}
		if (_LastDrawSplit != -1)
		{
			DrawSplitHelper(_LastDrawSplit);
		}
		_LastDrawSplit = -1;
		if (_Renderer != null)
		{
			_Renderer.Dispose();
			_Renderer = null;
		}
	}

	private Rectangle CalcSplitLine(int _SplitSize, int minWeight)
	{
		Rectangle bounds = Bounds;
		Rectangle bounds2 = _SplitTarget.Bounds;
		switch (Dock)
		{
			case DockStyle.Top:
				if (bounds.Height < minWeight)
				{
					bounds.Height = minWeight;
				}
				bounds.Y = bounds2.Y + _SplitSize;
				break;
			case DockStyle.Bottom:
				if (bounds.Height < minWeight)
				{
					bounds.Height = minWeight;
				}
				bounds.Y = bounds2.Y + bounds2.Height - _SplitSize - bounds.Height;
				break;
			case DockStyle.Left:
				if (bounds.Width < minWeight)
				{
					bounds.Width = minWeight;
				}
				bounds.X = bounds2.X + _SplitSize;
				break;
			case DockStyle.Right:
				if (bounds.Width < minWeight)
				{
					bounds.Width = minWeight;
				}
				bounds.X = bounds2.X + bounds2.Width - _SplitSize - bounds.Width;
				break;
		}
		return bounds;
	}

	private int CalcSplitSize()
	{
		Control control = FindTarget();
		if (control == null)
		{
			return -1;
		}
		Rectangle bounds = control.Bounds;
		switch (Dock)
		{
			case DockStyle.Top:
			case DockStyle.Bottom:
				return bounds.Height;
			case DockStyle.Left:
			case DockStyle.Right:
				return bounds.Width;
			default:
				return -1;
		}
	}

	private SplitData CalcSplitBounds()
	{
		SplitData splitData = new SplitData();
		Control control = splitData.target = FindTarget();
		if (control != null)
		{
			switch (control.Dock)
			{
				case DockStyle.Left:
				case DockStyle.Right:
					_InitTargetSize = control.Bounds.Width;
					break;
				case DockStyle.Top:
				case DockStyle.Bottom:
					_InitTargetSize = control.Bounds.Height;
					break;
			}
			Control control2 = Parent;
			ControlCollection controls = control2.Controls;
			int count = controls.Count;
			int num = 0;
			int num2 = 0;
			for (int i = 0; i < count; i++)
			{
				Control control3 = controls[i];
				if (control3 != control)
				{
					switch (control3.Dock)
					{
						case DockStyle.Left:
						case DockStyle.Right:
							num += control3.Width;
							break;
						case DockStyle.Top:
						case DockStyle.Bottom:
							num2 += control3.Height;
							break;
					}
				}
			}
			Size clientSize = control2.ClientSize;
			if (Horizontal)
			{
				_MaxSize = clientSize.Width - num - _MinExtra;
			}
			else
			{
				_MaxSize = clientSize.Height - num2 - _MinExtra;
			}
			splitData.dockWidth = num;
			splitData.dockHeight = num2;
		}
		return splitData;
	}

	private void DrawSplitHelper(int _SplitSize)
	{
		if (_SplitTarget != null)
		{
			Rectangle r = CalcSplitLine(_SplitSize, 5);
			_Renderer ??= new SplitRenderer();
			r = Parent.RectangleToScreen(r);
			_Renderer.SetBounds(r.Left, r.Top, r.Width, r.Height);
			_Renderer.Show();
		}
	}

	private Control FindTarget()
	{
		Control control = Parent;
		if (control == null)
		{
			return null;
		}
		ControlCollection controls = control.Controls;
		int count = controls.Count;
		DockStyle dock = Dock;
		for (int i = 0; i < count; i++)
		{
			Control control2 = controls[i];
			if (control2 == this)
			{
				continue;
			}
			switch (dock)
			{
				case DockStyle.Top:
					if (control2.Bottom == Top)
					{
						return control2;
					}
					break;
				case DockStyle.Bottom:
					if (control2.Top == Bottom)
					{
						return control2;
					}
					break;
				case DockStyle.Left:
					if (control2.Right == Left)
					{
						return control2;
					}
					break;
				case DockStyle.Right:
					if (control2.Left == Right)
					{
						return control2;
					}
					break;
			}
		}
		return null;
	}

	private int GetSplitSize(int x, int y)
	{
		int num = !Horizontal ? y - _Anchor.Y : x - _Anchor.X;
		int val = 0;
		switch (Dock)
		{
			case DockStyle.Top:
				val = _SplitTarget.Height + num;
				break;
			case DockStyle.Bottom:
				val = _SplitTarget.Height - num;
				break;
			case DockStyle.Left:
				val = _SplitTarget.Width + num;
				break;
			case DockStyle.Right:
				val = _SplitTarget.Width - num;
				break;
		}
		return Math.Max(Math.Min(val, _MaxSize), _MinSize);
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		base.OnKeyDown(e);
		if (_SplitTarget != null && e.KeyCode == Keys.Escape)
		{
			SplitEnd(accept: false);
		}
	}

	protected override void OnMouseCaptureChanged(EventArgs e)
	{
		base.OnMouseCaptureChanged(e);
		if (!Capture && _SplitTarget != null)
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
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		base.OnMouseMove(e);
		if (_SplitTarget != null)
		{
			int num = e.X + Left;
			int num2 = e.Y + Top;
			Rectangle rectangle = CalcSplitLine(GetSplitSize(e.X, e.Y), 0);
			int splitX = rectangle.X;
			int splitY = rectangle.Y;
			OnSplitterMoving(new SplitterEventArgs(num, num2, splitX, splitY));
		}
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		base.OnMouseUp(e);
		if (_SplitTarget != null)
		{
			_ = e.X;
			_ = Left;
			_ = e.Y;
			_ = Top;
			Rectangle rectangle = CalcSplitLine(GetSplitSize(e.X, e.Y), 0);
			_ = rectangle.X;
			_ = rectangle.Y;
			SplitEnd(accept: true);
		}
	}

	protected virtual void OnSplitterMoving(SplitterEventArgs sevent)
	{
		((SplitterEventHandler)Events[S_EVENT_MOVING])?.Invoke(this, sevent);
		if (_SplitTarget != null)
		{
			SplitMove(sevent.SplitX, sevent.SplitY);
		}
	}

	protected virtual void OnSplitterMoved(SplitterEventArgs sevent)
	{
		((SplitterEventHandler)Events[S_EVENT_MOVED])?.Invoke(this, sevent);
		if (_SplitTarget != null)
		{
			SplitMove(sevent.SplitX, sevent.SplitY);
		}
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		if (Horizontal)
		{
			if (width < 1)
			{
				width = C_DefaultWidth;
			}
			_SplitterThickness = width;
		}
		else
		{
			if (height < 1)
			{
				height = C_DefaultWidth;
			}
			_SplitterThickness = height;
		}
		base.SetBoundsCore(x, y, width, height, specified);
	}

	private void SplitBegin(int x, int y)
	{
		SplitData splitData = CalcSplitBounds();
		if (splitData.target != null && _MinSize < _MaxSize)
		{
			_Anchor = new Point(x, y);
			_SplitTarget = splitData.target;
			_SplitSize = GetSplitSize(x, y);
			if (_SplitterMessageFilter != null)
			{
				_SplitterMessageFilter = new SplitterMessageFilter(this);
			}
			Application.AddMessageFilter(_SplitterMessageFilter);
			Capture = true;
			DrawSplitBar(C_DRAW_START);
		}
	}

	private void SplitEnd(bool accept)
	{
		DrawSplitBar(C_DRAW_END);
		_SplitTarget = null;
		Capture = false;
		if (_SplitterMessageFilter != null)
		{
			Application.RemoveMessageFilter(_SplitterMessageFilter);
			_SplitterMessageFilter = null;
		}
		if (accept)
		{
			ApplySplitPosition();
		}
		else if (_SplitSize != _InitTargetSize)
		{
			SplitPosition = _InitTargetSize;
		}
		_Anchor = Point.Empty;
	}

	private void ApplySplitPosition()
	{
		SplitPosition = _SplitSize;
	}

	private void SplitMove(int x, int y)
	{
		int num = GetSplitSize(x - Left + _Anchor.X, y - Top + _Anchor.Y);
		if (_SplitSize != num)
		{
			_SplitSize = num;
			DrawSplitBar(C_DRAW_MOVE);
		}
	}

	public override string ToString()
	{
		string text = base.ToString();
		return text + ", MinExtra: " + MinExtra.ToString(CultureInfo.CurrentCulture) + ", MinSize: " + MinSize.ToString(CultureInfo.CurrentCulture);
	}
}
