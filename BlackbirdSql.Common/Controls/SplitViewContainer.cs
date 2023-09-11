// Microsoft.VisualStudio.Data.Tools.Design.Core, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Core.Controls.TabbedEditor.SplitViewContainer
using System;
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Config;
using BlackbirdSql.Common.Ctl;
using Microsoft.VisualStudio.Utilities;
using BlackbirdSql.Common.Events;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Controls;

[DesignerCategory("code")]
public class SplitViewContainer : Control, IServiceProvider
{
	public class SplitterEx : BlackbirdSql.Common.Controls.Splitter
	{
		private long _LastClickTime;

		private readonly VsFontColorPreferences _VsFontColorPreferences;

		public static long DoubleClickTicks => SystemInformation.DoubleClickTime * 10000;

		public SplitterEx(IServiceProvider serviceProvider)
		{
			SetStyle(ControlStyles.StandardClick | ControlStyles.StandardDoubleClick, value: false);
			SetBackColor();
			_VsFontColorPreferences = new VsFontColorPreferences();
			_VsFontColorPreferences.PreferencesChangedEvent += VsFontColorPreferences_PreferencesChanged;
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_VsFontColorPreferences.PreferencesChangedEvent -= VsFontColorPreferences_PreferencesChanged;
				_VsFontColorPreferences.Dispose();
			}
			base.Dispose(disposing);
		}

		private void VsFontColorPreferences_PreferencesChanged(object sender, EventArgs args)
		{
			SetBackColor();
		}

		private void SetBackColor()
		{
			BackColor = VsColorUtilities.GetShellColor(-217);
		}

		public void SendMouseDown(Control sourceControl, MouseEventArgs e)
		{
			Point point = PointToClient(sourceControl.PointToScreen(e.Location));
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
			OnMouseDown(e2);
		}

		public void SendMouseMove(Control sourceControl, MouseEventArgs e)
		{
			Point point = PointToClient(sourceControl.PointToScreen(e.Location));
			MouseEventArgs e2 = new MouseEventArgs(e.Button, e.Clicks, point.X, point.Y, e.Delta);
			OnMouseMove(e2);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			long ticks = DateTime.Now.Ticks;
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left)
			{
				long num = ticks - _LastClickTime;
				_LastClickTime = ticks;
				if (num >= 0 && num < DoubleClickTicks)
				{
					OnDoubleClick(EventArgs.Empty);
				}
			}
		}
	}

	public const string OPTION_ORIENTATION = "SplitView.Orientation";

	public const string OPTION_PERCENTAGEX = "SplitView.Percentage.X";

	public const string OPTION_PERCENTAGEY = "SplitView.Percentage.Y";

	public const string OPTION_SHOWSPLITTER = "SplitView.ShowSplitter";

	public const string OPTION_PRIMARYVIEW = "SplitView.PrimaryTab";

	private SplitViewSplitterStrip _splitViewStrip;

	private readonly Panel _panelDesign;

	private Panel _panelTop;

	private Panel _panelBottom;

	private Panel _splitContainer;

	private bool _currentlySettingBounds;

	private SplitterEx _splitter;

	private float _horizontalPrimaryPercentage = 0.66f;

	private float _verticalPrimaryPercentage = 0.5f;

	private bool _mouseDownOnSplitter;

	private Point _startDragLocation;

	private Orientation _orientation;

	private ToolStrip _pathStrip;

	private bool _pathStripVisibleInSplitMode = true;

	public bool UseCustomTabActivation { get; set; }

	public bool IsSplitterVisible
	{
		get
		{
			return SplitterBar.ShowSplitter;
		}
		set
		{
			bool showSplitter = SplitterBar.ShowSplitter;
			SplitterBar.ShowSplitter = value;
			if (showSplitter == SplitterBar.ShowSplitter)
			{
				return;
			}
			_splitContainer.Layout -= SplitContainer_Layout;
			_splitContainer.SuspendLayout();
			SplitterBar.SuspendLayout();
			_panelTop.SuspendLayout();
			_panelBottom.SuspendLayout();
			if (ShouldSwapButtons())
			{
				if (SplitterBar.ShowSplitter)
				{
					SwapButtonsNoSuspendResume();
				}
				else
				{
					Guid guid = (Guid)SplitterBar.PrimaryPaneFirstButton.Tag;
					UpdateActiveTab(guid);
					TabActivationRequestEvent?.Invoke(this, new TabActivationEventArgs(guid, true));
				}
			}
			if (SplitterBar.ShowSplitter)
			{
				ScaleView(PercentSplit);
			}
			else
			{
				ExpandPanel();
			}
			LoadPathItems();
			_panelBottom.ResumeLayout();
			_panelTop.ResumeLayout();
			SplitterBar.ResumeLayout();
			_splitContainer.ResumeLayout();
			_splitContainer.Layout += SplitContainer_Layout;
		}
	}

	public Orientation Orientation
	{
		get
		{
			return _orientation;
		}
		set
		{
			if (_orientation != value)
			{
				_orientation = value;
				SplitterBar.VSplitButton.Checked = Orientation == Orientation.Vertical && SplitterBar.ShowSplitter;
				SplitterBar.HSplitButton.Checked = Orientation == Orientation.Horizontal && SplitterBar.ShowSplitter;
				UpdateLayout();
				OrientationChangedEvent?.Invoke(this, new EventArgs());
			}
		}
	}

	private float PercentSplit
	{
		get
		{
			if (Orientation != 0)
			{
				return _verticalPrimaryPercentage;
			}
			return _horizontalPrimaryPercentage;
		}
		set
		{
			if (Orientation == Orientation.Horizontal)
			{
				_horizontalPrimaryPercentage = value;
			}
			else
			{
				_verticalPrimaryPercentage = value;
			}
		}
	}

	public Panel PanelDesign => _panelDesign;

	public Panel PanelTop
	{
		get
		{
			return _panelTop;
		}
		set
		{
			if (value != null && _panelTop != value)
			{
				value.Size = _panelTop.Size;
				value.Dock = _panelTop.Dock;
				_splitContainer.SuspendLayout();
				int num = _splitContainer.Controls.IndexOf(_panelTop);
				_splitContainer.Controls.RemoveAt(num);
				_splitContainer.Controls.Add(value);
				_splitContainer.Controls.SetChildIndex(value, num);
				_splitContainer.ResumeLayout();
				_splitContainer.Update();
				_panelTop = value;
			}
		}
	}

	public Panel PanelBottom
	{
		get
		{
			return _panelBottom;
		}
		set
		{
			if (value != null && _panelBottom != value)
			{
				_splitContainer.SuspendLayout();
				value.SuspendLayout();
				value.Size = _panelBottom.Size;
				value.Dock = _panelBottom.Dock;
				int num = _splitContainer.Controls.IndexOf(_panelBottom);
				_splitContainer.Controls.RemoveAt(num);
				value.Controls.Add(SplitterBar);
				_splitContainer.Controls.Add(value);
				_splitContainer.Controls.SetChildIndex(value, num);
				value.ResumeLayout();
				_splitContainer.ResumeLayout();
				_splitContainer.Update();
				_panelBottom = value;
			}
		}
	}

	public bool PathStripVisibleInSplitMode
	{
		get
		{
			return _pathStripVisibleInSplitMode;
		}
		set
		{
			_pathStripVisibleInSplitMode = value;
		}
	}

	public ToolStrip PathStrip => _pathStrip;

	public bool SplittersVisible
	{
		set
		{
			SplitterEx splitter = Splitter;
			bool visible = (SplitterBar.Visible = value);
			splitter.Visible = visible;
		}
	}

	public SplitterEx Splitter => _splitter;

	public SplitViewSplitterStrip SplitterBar => _splitViewStrip;

	public event EventHandler IsSplitterVisibleChangedEvent;

	public event EventHandler OrientationChangedEvent;

	public event EventHandler PanelSwappedEvent;

	public event EventHandler<TabActivationEventArgs> TabActivationRequestEvent;

	public SplitViewContainer()
	{
		InitializeComponent();
		BackColor = VsColorUtilities.GetShellColor(-217);
		_panelDesign = _panelTop;
		UseCustomTabActivation = false;
		Text = "SplitViewContainer";
		SplitterBar.ChevronButton.Click += SplitterBar_SplitButtonClicked;
		SplitterBar.HSplitButton.Click += SplitterBar_SplitButtonClicked;
		SplitterBar.VSplitButton.Click += SplitterBar_SplitButtonClicked;
		SplitterBar.SwapButton.Click += SwapButton_Click;
		SplitterBar.DesignerXamlClickEvent += DesignerXamlButton_Click;
		SplitterBar.DesignerXamlDoubleClickEvent += DesignerXamlButton_DoubleClick;
		SplitterBar.ShowSplitterChangedEvent += SplitterBar_ShowSplitterChanged;
		Splitter.DoubleClick += Splitter_DoubleClick;
		Splitter.SplitterMovedEvent += Splitter_SplitterMoved;
		LoadPathItems();
		UpdateSplitter();
		_splitContainer.Layout += SplitContainer_Layout;
		SplitterBar.Layout += SizePath;
		_pathStrip.Layout += SizePath;
	}

	private void UpdateLayout()
	{
		_splitContainer.SuspendLayout();
		PanelTop.SuspendLayout();
		PanelBottom.SuspendLayout();
		SplitterBar.SuspendLayout();
		Splitter.SuspendLayout();
		UpdateSplitter();
		if (SplitterBar.ShowSplitter)
		{
			ScaleView(PercentSplit);
		}
		else
		{
			ExpandPanel();
		}
		LoadPathItems();
		Splitter.ResumeLayout();
		SplitterBar.ResumeLayout();
		PanelTop.ResumeLayout();
		PanelBottom.ResumeLayout();
		_splitContainer.ResumeLayout();
	}

	public void CustomizeSplitterBarButton(Guid buttonTagGuid, EnSplitterBarButtonDisplayStyle displayStyle, string buttonText, Image buttonImage, string toolTipText = null)
	{
		ToolStripButton toolStripButton = null;
		foreach (ToolStripButton item in SplitterBar.EnumerateAllButtons())
		{
			if ((Guid)item.Tag == buttonTagGuid)
			{
				toolStripButton = item;
				break;
			}
		}
		if (toolStripButton != null)
		{
			ToolStripItemDisplayStyle displayStyle2 = ToolStripItemDisplayStyle.None;
			switch (displayStyle)
			{
				case EnSplitterBarButtonDisplayStyle.Text:
					displayStyle2 = ToolStripItemDisplayStyle.Text;
					break;
				case EnSplitterBarButtonDisplayStyle.Image:
					displayStyle2 = ToolStripItemDisplayStyle.Image;
					break;
				case EnSplitterBarButtonDisplayStyle.ImageAndText:
					displayStyle2 = ToolStripItemDisplayStyle.ImageAndText;
					break;
			}
			toolStripButton.DisplayStyle = displayStyle2;
			SizeF scaleFactor = ControlUtils.GetScaleFactor(this);
			toolStripButton.Image = ((buttonImage == null) ? null : ControlUtils.ScaleImage(buttonImage, scaleFactor));
			toolStripButton.Text = buttonText;
			toolStripButton.ToolTipText = toolTipText ?? buttonText;
		}
	}

	public void LoadWindowState(string section)
	{
	}

	public void SaveWindowState(string section)
	{
	}

	public void ClearActiveTab()
	{
		SplitterBar.Button1.Checked = false;
		SplitterBar.Button2.Checked = false;
		foreach (ToolStripButton primaryPaneButton in SplitterBar.PrimaryPaneButtons)
		{
			primaryPaneButton.Checked = false;
		}
		foreach (ToolStripButton secondaryPaneButton in SplitterBar.SecondaryPaneButtons)
		{
			secondaryPaneButton.Checked = false;
		}
	}

	private void DesignerXamlButton_Click(object sender, EventArgs e)
	{
		ToolStripButton toolStripButton = sender as ToolStripButton;
		SqlEtwProvider.EventWriteTSqlEditorTabSwitch(IsStart: true, toolStripButton.Name ?? string.Empty);
		if (toolStripButton == null)
		{
			return;
		}
		if (!UseCustomTabActivation && !toolStripButton.Checked)
		{
			UpdateActiveTab((Guid)toolStripButton.Tag);
			if (!IsSplitterVisible)
			{
				SwapPanels();
			}
		}
		if (!IsSplitterVisible)
		{
			TabActivationRequestEvent?.Invoke(sender, new TabActivationEventArgs((Guid)toolStripButton.Tag, true));
		}
		else
		{
			TabActivationRequestEvent?.Invoke(sender, new TabActivationEventArgs((Guid)toolStripButton.Tag));
		}
		SqlEtwProvider.EventWriteTSqlEditorTabSwitch(IsStart: false, toolStripButton.Name ?? string.Empty);
	}

	public void CycleToNextButton()
	{
		SplitterBar.GetButtonInCyclicOrderFromChecked(forward: true)?.PerformClick();
	}

	public void CycleToPreviousButton()
	{
		SplitterBar.GetButtonInCyclicOrderFromChecked(forward: false)?.PerformClick();
	}

	private void ExpandPanel()
	{
		ScaleView(1f);
		PanelBottom.Size = Size.Empty;
	}

	private void DesignerXamlButton_DoubleClick(object sender, EventArgs e)
	{
		ShowView(sender as ToolStripButton);
	}

	public Rectangle GetAvailableBounds(bool isTopOrLeft)
	{
		Rectangle empty;
		if (isTopOrLeft)
		{
			empty = PanelTop.ClientRectangle;
		}
		else if (Orientation == Orientation.Horizontal)
		{
			empty = PanelBottom.ClientRectangle;
			int num2 = (empty.Y = SplitterBar.Height - Splitter.Height);
			empty.Height = Math.Max(0, empty.Height - num2);
		}
		else
		{
			empty = PanelBottom.ClientRectangle;
			int num4 = (empty.X = SplitterBar.Width - Splitter.Width);
			empty.Width = Math.Max(0, empty.Width - num4);
		}
		return empty;
	}

	protected override object GetService(Type service)
	{
		if (base.Parent is IServiceProvider serviceProvider)
		{
			return serviceProvider.GetService(service);
		}
		return base.GetService(service);
	}

	private void InitializeComponent()
	{
		base.SuspendLayout();
		this._panelTop = new System.Windows.Forms.Panel
		{
			Dock = System.Windows.Forms.DockStyle.Top,
			Name = "PanelTop"
		};
		this._splitContainer = new System.Windows.Forms.Panel();
		this._splitContainer.SuspendLayout();
		this._splitContainer.Dock = System.Windows.Forms.DockStyle.Fill;
		this._splitContainer.Name = "SplitContainer";
		this._splitViewStrip = new BlackbirdSql.Common.Controls.SplitViewSplitterStrip(this);
		this._splitViewStrip.MouseDown += new System.Windows.Forms.MouseEventHandler(SplitterBar_MouseDown);
		this._splitViewStrip.MouseUp += new System.Windows.Forms.MouseEventHandler(SplitterBar_MouseUp);
		this._splitViewStrip.MouseMove += new System.Windows.Forms.MouseEventHandler(SplitterBar_MouseMove);
		this._splitViewStrip.MouseDoubleClick += new System.Windows.Forms.MouseEventHandler(SplitterBar_MouseDoubleClick);
		this._pathStrip = new System.Windows.Forms.ToolStrip
		{
			Text = "PathControl",
			CanOverflow = false,
			Dock = System.Windows.Forms.DockStyle.Bottom,
			GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden,
			AutoSize = false,
			Height = this._splitViewStrip.Height,
			BackColor = System.Drawing.SystemColors.Control
		};
		this._panelBottom = new System.Windows.Forms.Panel();
		this._panelBottom.SuspendLayout();
		this._panelBottom.Dock = System.Windows.Forms.DockStyle.Fill;
		this._panelBottom.Controls.Add(this._splitViewStrip);
		this._panelBottom.Name = "PanelBottom";
		this._splitter = new BlackbirdSql.Common.Controls.SplitViewContainer.SplitterEx(this)
		{
			Dock = System.Windows.Forms.DockStyle.Top,
			MinExtra = this._splitViewStrip.Height,
			Size = new System.Drawing.Size(1, 1)
		};
		this.BackColor = System.Drawing.SystemColors.Window;
		this._splitContainer.Controls.Add(this._panelBottom);
		this._splitContainer.Controls.Add(this._splitter);
		this._splitContainer.Controls.Add(this._panelTop);
		base.Controls.Add(this._splitContainer);
		base.Controls.Add(this._pathStrip);
		this._panelBottom.ResumeLayout(false);
		this._splitContainer.ResumeLayout(false);
		base.ResumeLayout(false);
		base.PerformLayout();
	}

	protected override void Dispose(bool disposing)
	{
		base.Dispose(disposing);
	}

	private void LoadPathItems()
	{
		bool flag = Orientation == Orientation.Horizontal && !IsSplitterVisible;
		ToolStrip obj = (flag ? SplitterBar : _pathStrip);
		obj.SuspendLayout();
		obj.ResumeLayout();
		_currentlySettingBounds = true;
		try
		{
			if (PathStripVisibleInSplitMode && _pathStrip.Visible == flag)
			{
				_pathStrip.Visible = !flag;
				if (flag && _mouseDownOnSplitter)
				{
					ExpandPanel();
				}
				else if (_pathStrip.Visible && Orientation == Orientation.Horizontal)
				{
					PanelTop.Height = Math.Min(PanelTop.Height, base.Height - _pathStrip.Height - SplitterBar.Height - 4);
				}
			}
		}
		finally
		{
			_currentlySettingBounds = false;
		}
	}

	private void ScaleView(float percentage)
	{
		bool pathBarPresent = true;
		if (Orientation == Orientation.Horizontal)
		{
			pathBarPresent = percentage != 1f;
			int num = base.Height;
			if (pathBarPresent)
			{
				num -= _splitViewStrip.Height;
			}
			SizePanel((int)(percentage * (float)num), pathBarPresent);
		}
		else
		{
			SizePanel((int)(percentage * (float)_splitContainer.Width), pathBarPresent);
		}
	}

	private void SizePanel(int distance, bool pathBarPresent)
	{
		_splitContainer.SuspendLayout();
		if (Orientation == Orientation.Horizontal)
		{
			distance = Math.Max(0, distance);
			int num = base.Height;
			if (pathBarPresent && _pathStrip.Visible)
			{
				num -= _pathStrip.Height;
			}
			if (Splitter.Visible)
			{
				num = num - Splitter.MinExtra - Splitter.Height;
			}
			distance = Math.Min(distance, num);
			PanelTop.Height = distance;
		}
		else
		{
			distance = Math.Max(0, distance);
			int val = _splitContainer.Width;
			if (Splitter.Visible)
			{
				val = _splitContainer.Width - Splitter.MinExtra - Splitter.Width;
			}
			distance = Math.Min(distance, val);
			PanelTop.Width = distance;
		}
		if (Splitter.SplitPosition != distance)
		{
			Splitter.InvalidateSplitPosition();
		}
		_splitContainer.ResumeLayout();
	}

	private void SizePath(object sender, EventArgs e)
	{
	}

	private void Splitter_SplitterMoved(object sender, SplitterEventArgs e)
	{
		_mouseDownOnSplitter = false;
		if (Orientation == Orientation.Horizontal && _splitContainer.Height > 0)
		{
			if (PanelBottom.Height - Splitter.MinExtra > 0)
			{
				PercentSplit = (float)PanelTop.Height / (float)_splitContainer.Height;
			}
		}
		else if (_splitContainer.Width > 0 && PanelBottom.Width - Splitter.MinExtra > 0)
		{
			PercentSplit = (float)PanelTop.Width / (float)_splitContainer.Width;
		}
	}

	private void Splitter_DoubleClick(object sender, EventArgs e)
	{
		SplitterBar.ChevronButton.PerformClick();
	}

	private void SplitterBar_MouseMove(object sender, MouseEventArgs e)
	{
		if (_mouseDownOnSplitter && e.Button == MouseButtons.Left)
		{
			Size dragSize = SystemInformation.DragSize;
			if (Math.Abs(e.X - _startDragLocation.X) >= dragSize.Width || Math.Abs(e.Y - _startDragLocation.Y) >= dragSize.Height)
			{
				Point point = SplitterBar.PointToClient(Splitter.PointToScreen(Point.Empty));
				MouseEventArgs e2 = new MouseEventArgs(e.Button, 1, point.X, point.Y, e.Delta);
				SplitterBar.Capture = false;
				Splitter.SendMouseDown(SplitterBar, e2);
				Splitter.SendMouseMove(SplitterBar, e);
			}
		}
	}

	private void SplitterBar_MouseUp(object sender, MouseEventArgs e)
	{
		_mouseDownOnSplitter = false;
		SplitterBar.Capture = false;
	}

	private void SplitterBar_MouseDown(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && SplitterBar.SplitterRectangle.Contains(e.Location))
		{
			_mouseDownOnSplitter = true;
			_startDragLocation = e.Location;
			SplitterBar.Capture = true;
			if (Orientation == Orientation.Horizontal)
			{
				Cursor.Current = Cursors.HSplit;
			}
			else
			{
				Cursor.Current = Cursors.VSplit;
			}
		}
		else
		{
			_mouseDownOnSplitter = false;
		}
	}

	private void SplitterBar_MouseDoubleClick(object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && SplitterBar.SplitterRectangle.Contains(e.Location))
		{
			Splitter_DoubleClick(sender, e);
		}
	}

	private void SplitterBar_ShowSplitterChanged(object sender, EventArgs e)
	{
		IsSplitterVisibleChangedEvent?.Invoke(this, e);
	}

	private void SplitterBar_SplitButtonClicked(object sender, EventArgs e)
	{
		ToolStripButton toolStripButton = sender as ToolStripButton;
		if (toolStripButton == SplitterBar.ChevronButton)
		{
			IsSplitterVisible = !SplitterBar.ShowSplitter;
		}
		else if (toolStripButton == SplitterBar.HSplitButton)
		{
			Orientation = Orientation.Horizontal;
			IsSplitterVisible = true;
		}
		else if (toolStripButton == SplitterBar.VSplitButton)
		{
			Orientation = Orientation.Vertical;
			IsSplitterVisible = true;
		}
	}

	private void SplitContainer_Layout(object sender, LayoutEventArgs e)
	{
		UpdateShowSplitter(sender, e);
	}

	public void SwapButtonsNoSuspendResume()
	{
		SplitterBar.Items.IndexOf(SplitterBar.Button1);
		SplitterBar.Items.IndexOf(SplitterBar.Button2);
		int num = 0;
		foreach (ToolStripButton secondaryPaneButton in SplitterBar.SecondaryPaneButtons)
		{
			SplitterBar.Items.Insert(num++, secondaryPaneButton);
		}
		SplitterBar.Items.Insert(num++, SplitterBar.SwapButton);
		foreach (ToolStripButton primaryPaneButton in SplitterBar.PrimaryPaneButtons)
		{
			SplitterBar.Items.Insert(num++, primaryPaneButton);
		}
	}

	public void SwapButtons()
	{
		SplitterBar.SuspendLayout();
		SwapButtonsNoSuspendResume();
		SplitterBar.ResumeLayout();
	}

	private void SwapButton_Click(object sender, EventArgs e)
	{
		SwapPanels();
	}

	public void SwapPanelsNoSuspendResume()
	{
		Panel panelTop = PanelTop;
		Size size = PanelTop.Size;
		DockStyle dock = PanelTop.Dock;
		Panel panelBottom = PanelBottom;
		Size size2 = PanelBottom.Size;
		DockStyle dock2 = PanelBottom.Dock;
		_panelBottom = panelTop;
		_panelBottom.Dock = dock2;
		_panelBottom.Size = size2;
		_panelBottom.Controls.Add(SplitterBar);
		_panelTop = panelBottom;
		_panelTop.Dock = dock;
		_panelTop.Size = (IsSplitterVisible ? size : new Size(_splitContainer.Width - Splitter.MinExtra, _splitContainer.Height - Splitter.MinExtra));
		_splitContainer.Controls.SetChildIndex(_panelBottom, 0);
		_splitContainer.Controls.SetChildIndex(_splitter, 1);
		_splitContainer.Controls.SetChildIndex(_panelTop, 2);
	}

	public void SwapPanels()
	{
		_splitContainer.SuspendLayout();
		SplitterBar.SuspendLayout();
		_panelTop.SuspendLayout();
		_panelBottom.SuspendLayout();
		if (IsSplitterVisible)
		{
			SwapButtons();
		}
		SwapPanelsNoSuspendResume();
		UpdateSplitter();
		SplitterBar.ResumeLayout();
		_panelTop.ResumeLayout();
		_panelBottom.ResumeLayout();
		_splitContainer.ResumeLayout();
		Update();
		PanelSwappedEvent?.Invoke(this, EventArgs.Empty);
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		bool isSplitterVisible = IsSplitterVisible;
		using (DpiAwareness.EnterDpiScope(DpiAwarenessContext.SystemAware))
		{
			_currentlySettingBounds = base.Width != width || base.Height != height;
			try
			{
				float percentage = (isSplitterVisible ? PercentSplit : 1f);
				base.SetBoundsCore(x, y, width, height, specified);
				if (width > 0 && height > 0 && _currentlySettingBounds && base.Visible)
				{
					ScaleView(percentage);
				}
			}
			finally
			{
				_currentlySettingBounds = false;
			}
		}
	}

	private void ShowView(ToolStripButton designOrXamlButton)
	{
		if (SplitterBar.ShowSplitter)
		{
			if (designOrXamlButton != null && SplitterBar.SecondaryPaneButtons.Contains(designOrXamlButton))
			{
				SwapButton_Click(designOrXamlButton, EventArgs.Empty);
			}
			_splitContainer.SuspendLayout();
			SplitterBar.ShowSplitter = false;
			LoadPathItems();
			ExpandPanel();
			_splitContainer.ResumeLayout();
		}
		else if (!designOrXamlButton.Checked)
		{
			designOrXamlButton.PerformClick();
		}
	}

	public void ShowView(AbstractEditorTab editorTab)
	{
		ShowView(GetButton(editorTab));
	}

	public ToolStripButton GetButton(AbstractEditorTab editorTab)
	{
		ToolStripButton result = null;
		foreach (ToolStripButton item in SplitterBar.EnumerateAllButtons())
		{
			if ((Guid)item.Tag == editorTab.LogicalView)
			{
				result = item;
				break;
			}
		}
		return result;
	}

	private void UpdateShowSplitter(object sender, EventArgs e)
	{
		if (_currentlySettingBounds)
		{
			return;
		}
		bool showSplitter = SplitterBar.ShowSplitter;
		if (Orientation == Orientation.Horizontal)
		{
			if (Splitter.SplitPosition != -1 && Splitter.SplitPosition <= Splitter.MinExtra)
			{
				SplitterBar.SwapButton.PerformClick();
				if (!showSplitter)
				{
					LoadPathItems();
					ExpandPanel();
				}
			}
			if (PanelBottom.Height > 0 && Splitter.SplitPosition != -1)
			{
				SplitterBar.ShowSplitter = PanelBottom.Height > Splitter.MinExtra && Splitter.SplitPosition > Splitter.MinExtra;
			}
		}
		else
		{
			if (Splitter.SplitPosition != -1 && Splitter.SplitPosition <= Splitter.MinExtra)
			{
				SplitterBar.SwapButton.PerformClick();
				if (!showSplitter)
				{
					LoadPathItems();
					ExpandPanel();
				}
			}
			if (PanelBottom.Width > 0 && Splitter.SplitPosition != -1)
			{
				SplitterBar.ShowSplitter = PanelBottom.Width > Splitter.MinExtra && Splitter.SplitPosition > Splitter.MinExtra;
			}
		}
		if (SplitterBar.ShowSplitter != showSplitter)
		{
			LoadPathItems();
			if (SplitterBar.ShowSplitter && SplitterBar.PrimaryPaneButtons != null && ShouldSwapButtons())
			{
				SwapButtons();
			}
		}
	}

	private bool ShouldSwapButtons()
	{
		bool result = false;
		foreach (ToolStripButton secondaryPaneButton in SplitterBar.SecondaryPaneButtons)
		{
			if (secondaryPaneButton.Checked)
			{
				result = true;
				break;
			}
		}
		return result;
	}

	public void UpdateActiveTab(Guid activeTabTypeGuid)
	{
		foreach (ToolStripButton item in SplitterBar.EnumerateAllButtons())
		{
			if ((Guid)item.Tag == activeTabTypeGuid)
			{
				item.Checked = true;
			}
			else
			{
				item.Checked = false;
			}
		}
	}

	private void UpdateSplitter()
	{
		if (_orientation == Orientation.Vertical)
		{
			PanelTop.Dock = DockStyle.Left;
			Splitter.Dock = DockStyle.Left;
			SplitterBar.Dock = DockStyle.Left;
			Splitter.MinExtra = SplitterBar.Width;
			Splitter.MinSize = 0;
			Splitter.Width = 1;
			Splitter.Height = 1;
		}
		else
		{
			PanelTop.Dock = DockStyle.Top;
			Splitter.Dock = DockStyle.Top;
			SplitterBar.Dock = DockStyle.Top;
			Splitter.MinExtra = SplitterBar.Height;
			Splitter.MinSize = 0;
			Splitter.Width = 1;
			Splitter.Height = 1;
		}
	}

	object IServiceProvider.GetService(Type serviceType)
	{
		return GetService(serviceType);
	}
}
