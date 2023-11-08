// System.Windows.Forms, Version=6.0.2.0, Culture=neutral, PublicKeyToken=b77a5c561934e089
// System.Windows.Forms.PropertyGridInternal.PropertyGridView
using System;
using System.Collections;
using System.ComponentModel;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Design;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using System.Windows.Forms.Design;
using System.Windows.Forms.PropertyGridInternal;
using System.Windows.Forms.VisualStyles;
using Microsoft.Win32;

internal class PropertyGridView : Control, IWin32Window, IWindowsFormsEditorService, IServiceProvider
{
	internal class DropDownHolder : Form, IMouseHookClient
	{
		internal class DropDownHolderAccessibleObject : ControlAccessibleObject
		{
			private readonly DropDownHolder _owningDropDownHolder;

			internal override global::Interop.UiaCore.IRawElementProviderFragmentRoot? FragmentRoot => _owningDropDownHolder._gridView?.OwnerGrid?.AccessibilityObject;

			private bool ExistsInAccessibleTree
			{
				get
				{
					if (_owningDropDownHolder.IsHandleCreated)
					{
						return _owningDropDownHolder.Visible;
					}
					return false;
				}
			}

			public DropDownHolderAccessibleObject(DropDownHolder dropDownHolder)
				: base(dropDownHolder)
			{
				_owningDropDownHolder = dropDownHolder;
			}

			internal override global::Interop.UiaCore.IRawElementProviderFragment? FragmentNavigate(global::Interop.UiaCore.NavigateDirection direction)
			{
				return direction switch
				{
					global::Interop.UiaCore.NavigateDirection.Parent => (!ExistsInAccessibleTree) ? null : _owningDropDownHolder._gridView?.SelectedGridEntry?.AccessibilityObject, 
					global::Interop.UiaCore.NavigateDirection.NextSibling => (!ExistsInAccessibleTree) ? null : _owningDropDownHolder._gridView?.EditAccessibleObject, 
					global::Interop.UiaCore.NavigateDirection.PreviousSibling => null, 
					_ => base.FragmentNavigate(direction), 
				};
			}

			internal override object? GetPropertyValue(global::Interop.UiaCore.UIA propertyID)
			{
				if (propertyID != global::Interop.UiaCore.UIA.NamePropertyId)
				{
					return base.GetPropertyValue(propertyID);
				}
				return System.SR.PropertyGridViewDropDownControlHolderAccessibleName;
			}
		}

		private Control _currentControl;

		private readonly PropertyGridView _gridView;

		private readonly MouseHook _mouseHook;

		private LinkLabel _createNewLink;

		private bool _resizable = true;

		private bool _resizing;

		private bool _resizeUp;

		private Point _dragStart = Point.Empty;

		private Rectangle _dragBaseRect = Rectangle.Empty;

		private int _currentMoveType;

		private static readonly int s_resizeGripSize = SystemInformation.HorizontalScrollBarHeight;

		private static readonly int s_resizeBarSize = s_resizeGripSize + 1;

		private static readonly int s_resizeBorderSize = s_resizeBarSize / 2;

		private static readonly Size s_minDropDownSize = new Size(SystemInformation.VerticalScrollBarWidth * 4, SystemInformation.HorizontalScrollBarHeight * 4);

		private Bitmap _sizeGripGlyph;

		private const int DropDownHolderBorder = 1;

		private const int MoveTypeNone = 0;

		private const int MoveTypeBottom = 1;

		private const int MoveTypeLeft = 2;

		private const int MoveTypeTop = 4;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style |= -2139095040;
				createParams.ExStyle |= 128;
				createParams.ClassStyle |= 131072;
				if (_gridView != null)
				{
					createParams.Parent = _gridView.ParentInternal.Handle;
				}
				return createParams;
			}
		}

		private LinkLabel CreateNewLink
		{
			get
			{
				if (_createNewLink == null)
				{
					_createNewLink = new LinkLabel();
					_createNewLink.LinkClicked += OnNewLinkClicked;
				}
				return _createNewLink;
			}
		}

		public virtual bool HookMouseDown
		{
			get
			{
				return _mouseHook.HookMouseDown;
			}
			set
			{
				_mouseHook.HookMouseDown = value;
			}
		}

		public bool ResizeUp
		{
			set
			{
				if (_resizeUp == value)
				{
					return;
				}
				_sizeGripGlyph = null;
				_resizeUp = value;
				if (_resizable)
				{
					base.DockPadding.Bottom = 0;
					base.DockPadding.Top = 0;
					if (value)
					{
						base.DockPadding.Top = s_resizeBarSize;
					}
					else
					{
						base.DockPadding.Bottom = s_resizeBarSize;
					}
				}
			}
		}

		internal override bool SupportsUiaProviders => true;

		public virtual Control Component => _currentControl;

		internal DropDownHolder(PropertyGridView gridView)
		{
			base.ShowInTaskbar = false;
			base.ControlBox = false;
			base.MinimizeBox = false;
			base.MaximizeBox = false;
			Text = string.Empty;
			base.FormBorderStyle = FormBorderStyle.None;
			base.AutoScaleMode = AutoScaleMode.None;
			_mouseHook = new MouseHook(this, this, gridView);
			base.Visible = false;
			_gridView = gridView;
			BackColor = _gridView.BackColor;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new DropDownHolderAccessibleObject(this);
		}

		protected override void DestroyHandle()
		{
			_mouseHook.HookMouseDown = false;
			base.DestroyHandle();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && _createNewLink != null)
			{
				_createNewLink.Dispose();
				_createNewLink = null;
			}
			base.Dispose(disposing);
		}

		public void DoModalLoop()
		{
			while (base.Visible)
			{
				Application.DoEventsModal();
				global::Interop.User32.MsgWaitForMultipleObjectsEx(0u, IntPtr.Zero, 250u, global::Interop.User32.QS.ALLINPUT, global::Interop.User32.MWMO.INPUTAVAILABLE);
			}
		}

		private InstanceCreationEditor GetInstanceCreationEditor(PropertyDescriptorGridEntry entry)
		{
			if (entry == null)
			{
				return null;
			}
			InstanceCreationEditor instanceCreationEditor = null;
			PropertyDescriptor propertyDescriptor = entry.PropertyDescriptor;
			if (propertyDescriptor != null)
			{
				instanceCreationEditor = propertyDescriptor.GetEditor(typeof(InstanceCreationEditor)) as InstanceCreationEditor;
			}
			if (instanceCreationEditor == null)
			{
				UITypeEditor uITypeEditor = entry.UITypeEditor;
				if (uITypeEditor != null && uITypeEditor.GetEditStyle() == UITypeEditorEditStyle.DropDown)
				{
					instanceCreationEditor = (InstanceCreationEditor)TypeDescriptor.GetEditor(uITypeEditor, typeof(InstanceCreationEditor));
				}
			}
			return instanceCreationEditor;
		}

		private Bitmap GetSizeGripGlyph(Graphics g)
		{
			if (_sizeGripGlyph != null)
			{
				return _sizeGripGlyph;
			}
			_sizeGripGlyph = new Bitmap(s_resizeGripSize, s_resizeGripSize, g);
			using (Graphics graphics = Graphics.FromImage(_sizeGripGlyph))
			{
				Matrix matrix = new Matrix();
				matrix.Translate(s_resizeGripSize + 1, _resizeUp ? (s_resizeGripSize + 1) : 0);
				matrix.Scale(-1f, (!_resizeUp) ? 1 : (-1));
				graphics.Transform = matrix;
				ControlPaint.DrawSizeGrip(graphics, BackColor, 0, 0, s_resizeGripSize, s_resizeGripSize);
				graphics.ResetTransform();
			}
			_sizeGripGlyph.MakeTransparent(BackColor);
			return _sizeGripGlyph;
		}

		public virtual bool GetUsed()
		{
			return _currentControl != null;
		}

		public virtual void FocusComponent()
		{
			if (_currentControl != null && base.Visible)
			{
				_currentControl.Focus();
			}
		}

		private bool OwnsWindow(IntPtr hWnd)
		{
			while (hWnd != IntPtr.Zero)
			{
				hWnd = global::Interop.User32.GetWindowLong(hWnd, global::Interop.User32.GWL.HWNDPARENT);
				if (hWnd == IntPtr.Zero)
				{
					return false;
				}
				if (hWnd == base.Handle)
				{
					return true;
				}
			}
			return false;
		}

		public bool OnClickHooked()
		{
			_gridView.CloseDropDownInternal(resetFocus: false);
			return false;
		}

		private void OnCurrentControlResize(object o, EventArgs e)
		{
			if (_currentControl != null && !_resizing)
			{
				int width = base.Width;
				Size size = new Size(2 + _currentControl.Width, 2 + _currentControl.Height);
				if (_resizable)
				{
					size.Height += s_resizeBarSize;
				}
				try
				{
					_resizing = true;
					SuspendLayout();
					base.Size = size;
				}
				finally
				{
					_resizing = false;
					ResumeLayout(performLayout: false);
				}
				base.Left -= base.Width - width;
			}
		}

		protected override void OnLayout(LayoutEventArgs levent)
		{
			try
			{
				_resizing = true;
				base.OnLayout(levent);
			}
			finally
			{
				_resizing = false;
			}
		}

		private void OnNewLinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
		{
			if (!(e.Link.LinkData is InstanceCreationEditor instanceCreationEditor) || _gridView?.SelectedGridEntry == null)
			{
				return;
			}
			Type propertyType = _gridView.SelectedGridEntry.PropertyType;
			if ((object)propertyType == null)
			{
				return;
			}
			_gridView.CloseDropDown();
			object obj = instanceCreationEditor.CreateInstance(_gridView.SelectedGridEntry, propertyType);
			if (obj != null)
			{
				if (!propertyType.IsInstanceOfType(obj))
				{
					throw new InvalidCastException(string.Format(System.SR.PropertyGridViewEditorCreatedInvalidObject, propertyType));
				}
				_gridView.CommitValue(obj);
			}
		}

		private int MoveTypeFromPoint(int x, int y)
		{
			Rectangle rectangle = new Rectangle(0, base.Height - s_resizeGripSize, s_resizeGripSize, s_resizeGripSize);
			Rectangle rectangle2 = new Rectangle(0, 0, s_resizeGripSize, s_resizeGripSize);
			if (!_resizeUp && rectangle.Contains(x, y))
			{
				return 3;
			}
			if (_resizeUp && rectangle2.Contains(x, y))
			{
				return 6;
			}
			if (!_resizeUp && Math.Abs(base.Height - y) < s_resizeBorderSize)
			{
				return 1;
			}
			if (_resizeUp && Math.Abs(y) < s_resizeBorderSize)
			{
				return 4;
			}
			return 0;
		}

		protected override void OnMouseDown(MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				_currentMoveType = MoveTypeFromPoint(e.X, e.Y);
				if (_currentMoveType != 0)
				{
					_dragStart = PointToScreen(new Point(e.X, e.Y));
					_dragBaseRect = base.Bounds;
					base.Capture = true;
				}
				else
				{
					_gridView.CloseDropDown();
				}
			}
			base.OnMouseDown(e);
		}

		protected override void OnMouseMove(MouseEventArgs e)
		{
			if (_currentMoveType == 0)
			{
				Cursor cursor;
				switch (MoveTypeFromPoint(e.X, e.Y))
				{
				case 3:
					cursor = Cursors.SizeNESW;
					break;
				case 1:
				case 4:
					cursor = Cursors.SizeNS;
					break;
				case 6:
					cursor = Cursors.SizeNWSE;
					break;
				default:
					cursor = null;
					break;
				}
				Cursor = cursor;
			}
			else
			{
				Point point = PointToScreen(new Point(e.X, e.Y));
				Rectangle bounds = base.Bounds;
				if ((_currentMoveType & 1) == 1)
				{
					bounds.Height = Math.Max(s_minDropDownSize.Height, _dragBaseRect.Height + (point.Y - _dragStart.Y));
				}
				if ((_currentMoveType & 4) == 4)
				{
					int num = point.Y - _dragStart.Y;
					if (_dragBaseRect.Height - num > s_minDropDownSize.Height)
					{
						bounds.Y = _dragBaseRect.Top + num;
						bounds.Height = _dragBaseRect.Height - num;
					}
				}
				if ((_currentMoveType & 2) == 2)
				{
					int num2 = point.X - _dragStart.X;
					if (_dragBaseRect.Width - num2 > s_minDropDownSize.Width)
					{
						bounds.X = _dragBaseRect.Left + num2;
						bounds.Width = _dragBaseRect.Width - num2;
					}
				}
				if (bounds != base.Bounds)
				{
					try
					{
						_resizing = true;
						base.Bounds = bounds;
					}
					finally
					{
						_resizing = false;
					}
				}
				Invalidate();
			}
			base.OnMouseMove(e);
		}

		protected override void OnMouseLeave(EventArgs e)
		{
			Cursor = null;
			base.OnMouseLeave(e);
		}

		protected override void OnMouseUp(MouseEventArgs e)
		{
			base.OnMouseUp(e);
			if (e.Button == MouseButtons.Left)
			{
				_currentMoveType = 0;
				_dragStart = Point.Empty;
				_dragBaseRect = Rectangle.Empty;
				base.Capture = false;
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
			base.OnPaint(e);
			if (_resizable)
			{
				Rectangle rect = new Rectangle(0, (!_resizeUp) ? (base.Height - s_resizeGripSize) : 0, s_resizeGripSize, s_resizeGripSize);
				e.Graphics.DrawImage(GetSizeGripGlyph(e.Graphics), rect);
				int num = (_resizeUp ? (s_resizeBarSize - 1) : (base.Height - s_resizeBarSize));
				using Pen pen = new Pen(SystemColors.ControlDark, 1f)
				{
					DashStyle = DashStyle.Solid
				};
				e.Graphics.DrawLine(pen, 0, num, base.Width, num);
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if ((keyData & (Keys.Shift | Keys.Control | Keys.Alt)) == 0)
			{
				switch (keyData & Keys.KeyCode)
				{
				case Keys.Escape:
					_gridView.OnEscape(this);
					return true;
				case Keys.F4:
					_gridView.F4Selection(popupModalDialog: true);
					return true;
				case Keys.Return:
					if (_gridView.UnfocusSelection() && _gridView.SelectedGridEntry != null)
					{
						_gridView.SelectedGridEntry.OnValueReturnKey();
					}
					return true;
				}
			}
			return base.ProcessDialogKey(keyData);
		}

		public void SetComponent(Control control, bool resizable)
		{
			_resizable = resizable;
			Font = _gridView.Font;
			InstanceCreationEditor instanceCreationEditor = ((control == null) ? null : GetInstanceCreationEditor(_gridView.SelectedGridEntry as PropertyDescriptorGridEntry));
			if (_currentControl != null)
			{
				_currentControl.Resize -= OnCurrentControlResize;
				base.Controls.Remove(_currentControl);
				_currentControl = null;
			}
			if (_createNewLink != null && _createNewLink.Parent == this)
			{
				base.Controls.Remove(_createNewLink);
			}
			if (control == null)
			{
				base.Enabled = false;
				return;
			}
			_currentControl = control;
			base.DockPadding.All = 0;
			if (_currentControl is GridViewListBox gridViewListBox && gridViewListBox.Items.Count == 0)
			{
				gridViewListBox.Height = Math.Max(gridViewListBox.Height, gridViewListBox.ItemHeight);
			}
			try
			{
				SuspendLayout();
				base.Controls.Add(control);
				Size size = new Size(2 + control.Width, 2 + control.Height);
				if (instanceCreationEditor != null)
				{
					CreateNewLink.Text = instanceCreationEditor.Text;
					CreateNewLink.Links.Clear();
					CreateNewLink.Links.Add(0, instanceCreationEditor.Text.Length, instanceCreationEditor);
					int num = CreateNewLink.Height;
					using (Graphics g = _gridView.CreateGraphics())
					{
						num = (int)PropertyGrid.MeasureTextHelper.MeasureText(_gridView.OwnerGrid, g, instanceCreationEditor.Text, _gridView.GetBaseFont()).Height;
					}
					CreateNewLink.Height = num + 1;
					size.Height += num + 2;
				}
				if (resizable)
				{
					size.Height += s_resizeBarSize;
					if (_resizeUp)
					{
						base.DockPadding.Top = s_resizeBarSize;
					}
					else
					{
						base.DockPadding.Bottom = s_resizeBarSize;
					}
				}
				base.Size = size;
				control.Dock = DockStyle.Fill;
				control.Visible = true;
				if (instanceCreationEditor != null)
				{
					CreateNewLink.Dock = DockStyle.Bottom;
					base.Controls.Add(CreateNewLink);
				}
			}
			finally
			{
				ResumeLayout(performLayout: true);
			}
			_currentControl.Resize += OnCurrentControlResize;
			base.Enabled = _currentControl != null;
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == 6)
			{
				SetState(States.Modal, value: true);
				IntPtr lParam = m.LParam;
				if (base.Visible && global::Interop.PARAM.LOWORD(m.WParam) == 0 && !OwnsWindow(lParam))
				{
					_gridView.CloseDropDownInternal(resetFocus: false);
					return;
				}
			}
			else
			{
				if (m.Msg == 16)
				{
					if (base.Visible)
					{
						_gridView.CloseDropDown();
					}
					return;
				}
				if (m.Msg == 736)
				{
					m.Result = IntPtr.Zero;
					return;
				}
			}
			base.WndProc(ref m);
		}
	}

	private enum ErrorState
	{
		None,
		Thrown,
		MessageBoxUp
	}

	internal class GridPositionData
	{
		private readonly ArrayList _expandedState;

		private readonly GridEntryCollection _selectedItemTree;

		private readonly int _itemRow;

		public GridPositionData(PropertyGridView gridView)
		{
			_selectedItemTree = gridView.GetGridEntryHierarchy(gridView._selectedGridEntry);
			_expandedState = gridView.SaveHierarchyState(gridView.TopLevelGridEntries);
			_itemRow = gridView._selectedRow;
		}

		public GridEntry Restore(PropertyGridView gridView)
		{
			gridView.RestoreHierarchyState(_expandedState);
			GridEntry gridEntry = gridView.FindEquivalentGridEntry(_selectedItemTree);
			if (gridEntry == null)
			{
				return null;
			}
			gridView.SelectGridEntry(gridEntry, pageIn: true);
			int num = gridView._selectedRow - _itemRow;
			if (num != 0 && gridView.ScrollBar.Visible && _itemRow < gridView._visibleRows)
			{
				num += gridView.GetScrollOffset();
				if (num < 0)
				{
					num = 0;
				}
				else if (num > gridView.ScrollBar.Maximum)
				{
					num = gridView.ScrollBar.Maximum - 1;
				}
				gridView.SetScrollOffset(num);
			}
			return gridEntry;
		}
	}

	private class GridViewEdit : TextBox, IMouseHookClient
	{
		protected class GridViewEditAccessibleObject : ControlAccessibleObject
		{
			private readonly PropertyGridView _owningPropertyGridView;

			private readonly TextBoxBaseUiaTextProvider _textProvider;

			public override AccessibleStates State
			{
				get
				{
					AccessibleStates state = base.State;
					if (IsReadOnly)
					{
						return state | AccessibleStates.ReadOnly;
					}
					return state & ~AccessibleStates.ReadOnly;
				}
			}

			internal override global::Interop.UiaCore.IRawElementProviderFragmentRoot? FragmentRoot => _owningPropertyGridView.OwnerGrid?.AccessibilityObject;

			internal override global::Interop.UiaCore.IRawElementProviderSimple? HostRawElementProvider => null;

			public override string? Name
			{
				get
				{
					string accessibleName = base.Owner.AccessibleName;
					if (accessibleName != null)
					{
						return accessibleName;
					}
					GridEntry selectedGridEntry = _owningPropertyGridView.SelectedGridEntry;
					if (selectedGridEntry != null)
					{
						return selectedGridEntry.AccessibilityObject.Name;
					}
					return base.Name;
				}
				set
				{
					base.Name = value;
				}
			}

			internal override int[]? RuntimeId
			{
				get
				{
					int[] array = _owningPropertyGridView?.SelectedGridEntry?.AccessibilityObject?.RuntimeId;
					if (array == null)
					{
						return null;
					}
					int[] array2 = new int[array.Length + 1];
					for (int i = 0; i < array.Length; i++)
					{
						array2[i] = array[i];
					}
					array2[^1] = 1;
					return array2;
				}
			}

			internal override bool IsReadOnly
			{
				get
				{
					if (_owningPropertyGridView.SelectedGridEntry is PropertyDescriptorGridEntry propertyDescriptorGridEntry)
					{
						return propertyDescriptorGridEntry.IsPropertyReadOnly;
					}
					return true;
				}
			}

			public GridViewEditAccessibleObject(GridViewEdit owner)
				: base(owner)
			{
				_owningPropertyGridView = owner.PropertyGridView;
				_textProvider = new TextBoxBaseUiaTextProvider(owner);
				UseTextProviders(_textProvider, _textProvider);
			}

			internal override bool IsIAccessibleExSupported()
			{
				return true;
			}

			internal override global::Interop.UiaCore.IRawElementProviderFragment? FragmentNavigate(global::Interop.UiaCore.NavigateDirection direction)
			{
				if (direction == global::Interop.UiaCore.NavigateDirection.Parent && _owningPropertyGridView.SelectedGridEntry != null)
				{
					return _owningPropertyGridView.SelectedGridEntry.AccessibilityObject;
				}
				switch (direction)
				{
				case global::Interop.UiaCore.NavigateDirection.NextSibling:
					if (_owningPropertyGridView.DropDownButton.Visible)
					{
						return _owningPropertyGridView.DropDownButton.AccessibilityObject;
					}
					if (_owningPropertyGridView.DialogButton.Visible)
					{
						return _owningPropertyGridView.DialogButton.AccessibilityObject;
					}
					break;
				case global::Interop.UiaCore.NavigateDirection.PreviousSibling:
					if (_owningPropertyGridView.DropDownVisible)
					{
						return _owningPropertyGridView.DropDownControlHolder.AccessibilityObject;
					}
					break;
				}
				return base.FragmentNavigate(direction);
			}

			internal override object? GetPropertyValue(global::Interop.UiaCore.UIA propertyID)
			{
				return propertyID switch
				{
					global::Interop.UiaCore.UIA.RuntimeIdPropertyId => RuntimeId, 
					global::Interop.UiaCore.UIA.ControlTypePropertyId => global::Interop.UiaCore.UIA.EditControlTypeId, 
					global::Interop.UiaCore.UIA.NamePropertyId => Name, 
					global::Interop.UiaCore.UIA.HasKeyboardFocusPropertyId => base.Owner.Focused, 
					global::Interop.UiaCore.UIA.IsEnabledPropertyId => !IsReadOnly, 
					global::Interop.UiaCore.UIA.ClassNamePropertyId => base.Owner.GetType().ToString(), 
					global::Interop.UiaCore.UIA.FrameworkIdPropertyId => "WinForm", 
					global::Interop.UiaCore.UIA.IsValuePatternAvailablePropertyId => IsPatternSupported(global::Interop.UiaCore.UIA.ValuePatternId), 
					global::Interop.UiaCore.UIA.IsTextPatternAvailablePropertyId => IsPatternSupported(global::Interop.UiaCore.UIA.TextPatternId), 
					global::Interop.UiaCore.UIA.IsTextPattern2AvailablePropertyId => IsPatternSupported(global::Interop.UiaCore.UIA.TextPattern2Id), 
					_ => base.GetPropertyValue(propertyID), 
				};
			}

			internal override bool IsPatternSupported(global::Interop.UiaCore.UIA patternId)
			{
				return patternId switch
				{
					global::Interop.UiaCore.UIA.ValuePatternId => true, 
					global::Interop.UiaCore.UIA.TextPatternId => true, 
					global::Interop.UiaCore.UIA.TextPattern2Id => true, 
					_ => base.IsPatternSupported(patternId), 
				};
			}

			internal override void SetFocus()
			{
				RaiseAutomationEvent(global::Interop.UiaCore.UIA.AutomationFocusChangedEventId);
				base.SetFocus();
			}
		}

		private bool _inSetText;

		private bool _filter;

		private bool _dontFocus;

		private int _lastMove;

		private readonly MouseHook _mouseHook;

		internal PropertyGridView PropertyGridView { get; }

		public bool DontFocus
		{
			set
			{
				_dontFocus = value;
			}
		}

		public virtual bool Filter
		{
			get
			{
				return _filter;
			}
			set
			{
				_filter = value;
			}
		}

		internal override bool SupportsUiaProviders => true;

		public override bool Focused
		{
			get
			{
				if (!_dontFocus)
				{
					return base.Focused;
				}
				return false;
			}
		}

		public override string Text
		{
			get
			{
				return base.Text;
			}
			set
			{
				_inSetText = true;
				base.Text = value;
				_inSetText = false;
			}
		}

		public bool DisableMouseHook
		{
			set
			{
				_mouseHook.DisableMouseHook = value;
			}
		}

		public virtual bool HookMouseDown
		{
			get
			{
				return _mouseHook.HookMouseDown;
			}
			set
			{
				_mouseHook.HookMouseDown = value;
				if (value)
				{
					Focus();
				}
			}
		}

		public GridViewEdit(PropertyGridView gridView)
		{
			PropertyGridView = gridView;
			_mouseHook = new MouseHook(this, this, gridView);
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new GridViewEditAccessibleObject(this);
		}

		protected override void DestroyHandle()
		{
			_mouseHook.HookMouseDown = false;
			base.DestroyHandle();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing)
			{
				_mouseHook.Dispose();
			}
			base.Dispose(disposing);
		}

		public void FilterKeyPress(char keyChar)
		{
			if (IsInputChar(keyChar))
			{
				Focus();
				SelectAll();
				global::Interop.User32.PostMessageW(this, global::Interop.User32.WM.CHAR, (IntPtr)keyChar, (IntPtr)0);
			}
		}

		protected override bool IsInputKey(Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
			case Keys.Tab:
			case Keys.Return:
			case Keys.Escape:
			case Keys.F1:
			case Keys.F4:
				return false;
			default:
				if (PropertyGridView.NeedsCommit)
				{
					return false;
				}
				return base.IsInputKey(keyData);
			}
		}

		protected override bool IsInputChar(char keyChar)
		{
			if (keyChar == '\t' || keyChar == '\r')
			{
				return false;
			}
			return base.IsInputChar(keyChar);
		}

		protected override void OnGotFocus(EventArgs e)
		{
			base.OnGotFocus(e);
			base.AccessibilityObject.RaiseAutomationEvent(global::Interop.UiaCore.UIA.AutomationFocusChangedEventId);
		}

		protected override void OnKeyDown(KeyEventArgs e)
		{
			if (ProcessDialogKey(e.KeyData))
			{
				e.Handled = true;
			}
			else
			{
				base.OnKeyDown(e);
			}
		}

		protected override void OnKeyPress(KeyPressEventArgs e)
		{
			if (!IsInputChar(e.KeyChar))
			{
				e.Handled = true;
			}
			else
			{
				base.OnKeyPress(e);
			}
		}

		public bool OnClickHooked()
		{
			return !PropertyGridView._Commit();
		}

		protected override void OnMouseEnter(EventArgs e)
		{
			base.OnMouseEnter(e);
			if (Focused)
			{
				return;
			}
			using Graphics graphics = CreateGraphics();
			if (PropertyGridView.SelectedGridEntry != null && base.ClientRectangle.Width <= PropertyGridView.SelectedGridEntry.GetValueTextWidth(Text, graphics, Font))
			{
				PropertyGridView.ToolTip.ToolTip = (PasswordProtect ? "" : Text);
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			switch (keyData & Keys.KeyCode)
			{
			case Keys.C:
			case Keys.V:
			case Keys.X:
			case Keys.Z:
				if ((keyData & Keys.Control) != 0 && (keyData & Keys.Shift) == 0 && (keyData & Keys.Alt) == 0)
				{
					return false;
				}
				break;
			case Keys.A:
				if ((keyData & Keys.Control) != 0 && (keyData & Keys.Shift) == 0 && (keyData & Keys.Alt) == 0)
				{
					SelectAll();
					return true;
				}
				break;
			case Keys.Insert:
				if ((keyData & Keys.Alt) == 0 && (((keyData & Keys.Control) != 0) ^ ((keyData & Keys.Shift) == 0)))
				{
					return false;
				}
				break;
			case Keys.Delete:
				if ((keyData & Keys.Control) == 0 && (keyData & Keys.Shift) != 0 && (keyData & Keys.Alt) == 0)
				{
					return false;
				}
				if ((keyData & Keys.Control) == 0 && (keyData & Keys.Shift) == 0 && (keyData & Keys.Alt) == 0 && PropertyGridView.SelectedGridEntry != null && !PropertyGridView.SelectedGridEntry.Enumerable && !PropertyGridView.SelectedGridEntry.IsTextEditable && PropertyGridView.SelectedGridEntry.CanResetPropertyValue())
				{
					object propertyValue = PropertyGridView.SelectedGridEntry.PropertyValue;
					PropertyGridView.SelectedGridEntry.ResetPropertyValue();
					PropertyGridView.UnfocusSelection();
					PropertyGridView.OwnerGrid.OnPropertyValueSet(PropertyGridView.SelectedGridEntry, propertyValue);
				}
				break;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			if ((keyData & (Keys.Shift | Keys.Control | Keys.Alt)) == 0)
			{
				switch (keyData & Keys.KeyCode)
				{
				case Keys.Return:
				{
					bool flag = !PropertyGridView.NeedsCommit;
					if (PropertyGridView.UnfocusSelection() && flag && PropertyGridView.SelectedGridEntry != null)
					{
						PropertyGridView.SelectedGridEntry.OnValueReturnKey();
					}
					return true;
				}
				case Keys.Escape:
					PropertyGridView.OnEscape(this);
					return true;
				case Keys.F4:
					PropertyGridView.F4Selection(popupModalDialog: true);
					return true;
				}
			}
			if ((keyData & Keys.KeyCode) == Keys.Tab && (keyData & (Keys.Control | Keys.Alt)) == 0)
			{
				return !PropertyGridView._Commit();
			}
			return base.ProcessDialogKey(keyData);
		}

		protected override void SetVisibleCore(bool value)
		{
			if (!value && HookMouseDown)
			{
				_mouseHook.HookMouseDown = false;
			}
			base.SetVisibleCore(value);
		}

		private unsafe bool WmNotify(ref Message m)
		{
			if (m.LParam != IntPtr.Zero)
			{
				global::Interop.User32.NMHDR* ptr = (global::Interop.User32.NMHDR*)(void*)m.LParam;
				if (ptr->hwndFrom == PropertyGridView.ToolTip.Handle)
				{
					if (ptr->code == -521)
					{
						PositionTooltip(this, PropertyGridView.ToolTip, base.ClientRectangle);
						m.Result = (IntPtr)1;
						return true;
					}
					PropertyGridView.WndProc(ref m);
				}
			}
			return false;
		}

		protected override void WndProc(ref Message m)
		{
			if (_filter && PropertyGridView.FilterEditWndProc(ref m))
			{
				return;
			}
			switch ((global::Interop.User32.WM)m.Msg)
			{
			case global::Interop.User32.WM.STYLECHANGED:
				if (((uint)(int)(long)m.WParam & 0xFFFFFFECu) != 0)
				{
					PropertyGridView.Invalidate();
				}
				break;
			case global::Interop.User32.WM.MOUSEFIRST:
				if ((int)(long)m.LParam == _lastMove)
				{
					return;
				}
				_lastMove = (int)(long)m.LParam;
				break;
			case global::Interop.User32.WM.DESTROY:
				_mouseHook.HookMouseDown = false;
				break;
			case global::Interop.User32.WM.SHOWWINDOW:
				if (IntPtr.Zero == m.WParam)
				{
					_mouseHook.HookMouseDown = false;
				}
				break;
			case global::Interop.User32.WM.PASTE:
				if (base.ReadOnly)
				{
					return;
				}
				break;
			case global::Interop.User32.WM.GETDLGCODE:
				m.Result = (IntPtr)((long)m.Result | 1 | 0x80);
				if (PropertyGridView.NeedsCommit || PropertyGridView.WantsTab((Control.ModifierKeys & Keys.Shift) == 0))
				{
					m.Result = (IntPtr)((long)m.Result | 4 | 2);
				}
				return;
			case global::Interop.User32.WM.NOTIFY:
				if (WmNotify(ref m))
				{
					return;
				}
				break;
			}
			base.WndProc(ref m);
		}

		public virtual bool InSetText()
		{
			return _inSetText;
		}
	}

	internal class GridViewListBox : ListBox
	{
		private bool _inSetSelectedIndex;

		private readonly PropertyGridView _owningPropertyGridView;

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams createParams = base.CreateParams;
				createParams.Style &= -8388609;
				createParams.ExStyle &= -513;
				return createParams;
			}
		}

		internal PropertyGridView OwningPropertyGridView => _owningPropertyGridView;

		internal override bool SupportsUiaProviders => true;

		public GridViewListBox(PropertyGridView gridView)
		{
			if (gridView == null)
			{
				throw new ArgumentNullException("gridView");
			}
			base.IntegralHeight = false;
			_owningPropertyGridView = gridView;
			base.BackColor = gridView.BackColor;
		}

		protected override AccessibleObject CreateAccessibilityInstance()
		{
			return new GridViewListBoxAccessibleObject(this);
		}

		public virtual bool InSetSelectedIndex()
		{
			return _inSetSelectedIndex;
		}

		protected override void OnSelectedIndexChanged(EventArgs e)
		{
			_inSetSelectedIndex = true;
			base.OnSelectedIndexChanged(e);
			_inSetSelectedIndex = false;
		}
	}

	private class GridViewListBoxAccessibleObject : ListBox.ListBoxAccessibleObject
	{
		private readonly PropertyGridView _owningPropertyGridView;

		public override string? Name => base.Name ?? System.SR.PropertyGridEntryValuesListDefaultAccessibleName;

		internal override global::Interop.UiaCore.IRawElementProviderFragmentRoot FragmentRoot => _owningPropertyGridView.AccessibilityObject;

		public GridViewListBoxAccessibleObject(GridViewListBox owningGridViewListBox)
			: base(owningGridViewListBox)
		{
			PropertyGridView owningPropertyGridView = owningGridViewListBox.OwningPropertyGridView;
			if (owningPropertyGridView == null)
			{
				throw new ArgumentException(null, "owningGridViewListBox");
			}
			_owningPropertyGridView = owningPropertyGridView;
		}

		internal override global::Interop.UiaCore.IRawElementProviderFragment? FragmentNavigate(global::Interop.UiaCore.NavigateDirection direction)
		{
			if (direction == global::Interop.UiaCore.NavigateDirection.Parent && _owningPropertyGridView.SelectedGridEntry != null)
			{
				return _owningPropertyGridView.SelectedGridEntry.AccessibilityObject;
			}
			if (direction == global::Interop.UiaCore.NavigateDirection.NextSibling)
			{
				return _owningPropertyGridView.Edit.AccessibilityObject;
			}
			return base.FragmentNavigate(direction);
		}
	}

	private class GridViewListBoxItemAccessibleObject : ListBox.ListBoxItemAccessibleObject
	{
		private readonly GridViewListBox _owningGridViewListBox;

		private readonly ItemArray.Entry _owningItem;

		internal override global::Interop.UiaCore.IRawElementProviderFragmentRoot FragmentRoot => _owningGridViewListBox.AccessibilityObject;

		public override string? Name
		{
			get
			{
				if (_owningGridViewListBox != null)
				{
					return _owningItem.ToString();
				}
				return base.Name;
			}
		}

		internal override int[] RuntimeId => new int[3]
		{
			42,
			(int)(long)_owningGridViewListBox.Handle,
			_owningItem.GetHashCode()
		};

		public GridViewListBoxItemAccessibleObject(GridViewListBox owningGridViewListBox, ItemArray.Entry owningItem)
			: base(owningGridViewListBox, owningItem, (ListBox.ListBoxAccessibleObject)owningGridViewListBox.AccessibilityObject)
		{
			_owningGridViewListBox = owningGridViewListBox;
			_owningItem = owningItem;
		}

		internal override object? GetPropertyValue(global::Interop.UiaCore.UIA propertyID)
		{
			if (propertyID == global::Interop.UiaCore.UIA.AccessKeyPropertyId)
			{
				return KeyboardShortcut;
			}
			return base.GetPropertyValue(propertyID);
		}

		internal override bool IsPatternSupported(global::Interop.UiaCore.UIA patternId)
		{
			if (patternId != global::Interop.UiaCore.UIA.InvokePatternId)
			{
				return base.IsPatternSupported(patternId);
			}
			return true;
		}
	}

	internal interface IMouseHookClient
	{
		bool OnClickHooked();
	}

	internal class MouseHook
	{
		private class MouseHookObject
		{
			private readonly WeakReference _reference;

			public MouseHookObject(MouseHook parent)
			{
				_reference = new WeakReference(parent, trackResurrection: false);
			}

			public virtual IntPtr Callback(global::Interop.User32.HC nCode, IntPtr wparam, IntPtr lparam)
			{
				IntPtr result = IntPtr.Zero;
				try
				{
					if (_reference.Target is MouseHook mouseHook)
					{
						result = mouseHook.MouseHookProc(nCode, wparam, lparam);
					}
				}
				catch (Exception)
				{
				}
				return result;
			}
		}

		private readonly PropertyGridView _gridView;

		private readonly Control _control;

		private readonly IMouseHookClient _client;

		private uint _thisProcessId;

		private GCHandle _mouseHookRoot;

		private IntPtr _mouseHookHandle = IntPtr.Zero;

		private bool _hookDisable;

		private bool _processing;

		public bool DisableMouseHook
		{
			set
			{
				_hookDisable = value;
				if (value)
				{
					UnhookMouse();
				}
			}
		}

		public virtual bool HookMouseDown
		{
			get
			{
				GC.KeepAlive(this);
				return _mouseHookHandle != IntPtr.Zero;
			}
			set
			{
				if (value && !_hookDisable)
				{
					HookMouse();
				}
				else
				{
					UnhookMouse();
				}
			}
		}

		public MouseHook(Control control, IMouseHookClient client, PropertyGridView gridView)
		{
			_control = control;
			_gridView = gridView;
			_client = client;
		}

		public void Dispose()
		{
			UnhookMouse();
		}

		private void HookMouse()
		{
			GC.KeepAlive(this);
			lock (this)
			{
				if (!(_mouseHookHandle != IntPtr.Zero))
				{
					if (_thisProcessId == 0)
					{
						global::Interop.User32.GetWindowThreadProcessId(_control, out _thisProcessId);
					}
					global::Interop.User32.HOOKPROC hOOKPROC = new MouseHookObject(this).Callback;
					_mouseHookRoot = GCHandle.Alloc(hOOKPROC);
					_mouseHookHandle = global::Interop.User32.SetWindowsHookExW(global::Interop.User32.WH.MOUSE, hOOKPROC, IntPtr.Zero, global::Interop.Kernel32.GetCurrentThreadId());
				}
			}
		}

		private unsafe IntPtr MouseHookProc(global::Interop.User32.HC nCode, IntPtr wparam, IntPtr lparam)
		{
			GC.KeepAlive(this);
			if (nCode == global::Interop.User32.HC.ACTION)
			{
				global::Interop.User32.MOUSEHOOKSTRUCT* ptr = (global::Interop.User32.MOUSEHOOKSTRUCT*)(void*)lparam;
				if (ptr != null)
				{
					switch ((global::Interop.User32.WM)(long)wparam)
					{
					case global::Interop.User32.WM.MOUSEACTIVATE:
					case global::Interop.User32.WM.NCLBUTTONDOWN:
					case global::Interop.User32.WM.NCRBUTTONDOWN:
					case global::Interop.User32.WM.NCMBUTTONDOWN:
					case global::Interop.User32.WM.LBUTTONDOWN:
					case global::Interop.User32.WM.RBUTTONDOWN:
					case global::Interop.User32.WM.MBUTTONDOWN:
						if (ProcessMouseDown(ptr->hWnd))
						{
							return (IntPtr)1;
						}
						break;
					}
				}
			}
			return global::Interop.User32.CallNextHookEx(new HandleRef(this, _mouseHookHandle), nCode, wparam, lparam);
		}

		private void UnhookMouse()
		{
			GC.KeepAlive(this);
			lock (this)
			{
				if (_mouseHookHandle != IntPtr.Zero)
				{
					global::Interop.User32.UnhookWindowsHookEx(new HandleRef(this, _mouseHookHandle));
					_mouseHookRoot.Free();
					_mouseHookHandle = IntPtr.Zero;
				}
			}
		}

		private bool ProcessMouseDown(IntPtr hwnd)
		{
			if (_processing)
			{
				return false;
			}
			IntPtr handleInternal = _control.HandleInternal;
			if (hwnd != handleInternal)
			{
				Control control = Control.FromHandle(hwnd);
				if (control != null && !_control.Contains(control))
				{
					global::Interop.User32.GetWindowThreadProcessId(hwnd, out var lpdwProcessId);
					if (lpdwProcessId != _thisProcessId)
					{
						HookMouseDown = false;
						return false;
					}
					bool flag = control == null || !_gridView.IsSiblingControl(_control, control);
					try
					{
						_processing = true;
						if (flag && _client.OnClickHooked())
						{
							return true;
						}
					}
					finally
					{
						_processing = false;
					}
					HookMouseDown = false;
				}
			}
			return false;
		}
	}

	internal class PropertyGridViewAccessibleObject : ControlAccessibleObject
	{
		private readonly PropertyGridView _owningPropertyGridView;

		private readonly PropertyGrid _parentPropertyGrid;

		internal override global::Interop.UiaCore.IRawElementProviderFragmentRoot? FragmentRoot => _owningPropertyGridView.OwnerGrid?.AccessibilityObject;

		private bool IsSortedByCategories
		{
			get
			{
				if (_owningPropertyGridView.OwnerGrid != null)
				{
					return _owningPropertyGridView.OwnerGrid.SortedByCategories;
				}
				return false;
			}
		}

		public override string Name => base.Owner.AccessibleName ?? string.Format(System.SR.PropertyGridDefaultAccessibleNameTemplate, _owningPropertyGridView?.OwnerGrid?.AccessibilityObject.Name);

		public override AccessibleRole Role
		{
			get
			{
				AccessibleRole accessibleRole = base.Owner.AccessibleRole;
				if (accessibleRole == AccessibleRole.Default)
				{
					return AccessibleRole.Table;
				}
				return accessibleRole;
			}
		}

		internal override int RowCount
		{
			get
			{
				GridEntryCollection topLevelGridEntries = _owningPropertyGridView.TopLevelGridEntries;
				if (topLevelGridEntries == null)
				{
					return 0;
				}
				if (!IsSortedByCategories)
				{
					return topLevelGridEntries.Count;
				}
				int num = 0;
				foreach (object item in topLevelGridEntries)
				{
					if (item is CategoryGridEntry)
					{
						num++;
					}
				}
				return num;
			}
		}

		internal override int ColumnCount => 1;

		public PropertyGridViewAccessibleObject(PropertyGridView owner, PropertyGrid parentPropertyGrid)
			: base(owner)
		{
			_owningPropertyGridView = owner;
			_parentPropertyGrid = parentPropertyGrid;
		}

		internal override global::Interop.UiaCore.IRawElementProviderFragment? ElementProviderFromPoint(double x, double y)
		{
			if (!base.Owner.IsHandleCreated)
			{
				return null;
			}
			return HitTest((int)x, (int)y);
		}

		internal override global::Interop.UiaCore.IRawElementProviderFragment? FragmentNavigate(global::Interop.UiaCore.NavigateDirection direction)
		{
			if (_parentPropertyGrid.IsHandleCreated && _parentPropertyGrid.AccessibilityObject is PropertyGrid.PropertyGridAccessibleObject propertyGridAccessibleObject)
			{
				global::Interop.UiaCore.IRawElementProviderFragment rawElementProviderFragment = propertyGridAccessibleObject.ChildFragmentNavigate(this, direction);
				if (rawElementProviderFragment != null)
				{
					return rawElementProviderFragment;
				}
			}
			return direction switch
			{
				global::Interop.UiaCore.NavigateDirection.FirstChild => IsSortedByCategories ? GetCategory(0) : GetChild(0), 
				global::Interop.UiaCore.NavigateDirection.LastChild => IsSortedByCategories ? GetLastCategory() : GetLastChild(), 
				_ => base.FragmentNavigate(direction), 
			};
		}

		internal override global::Interop.UiaCore.IRawElementProviderFragment? GetFocus()
		{
			return GetFocused();
		}

		internal override object? GetPropertyValue(global::Interop.UiaCore.UIA propertyID)
		{
			return propertyID switch
			{
				global::Interop.UiaCore.UIA.ControlTypePropertyId => global::Interop.UiaCore.UIA.TableControlTypeId, 
				global::Interop.UiaCore.UIA.NamePropertyId => Name, 
				global::Interop.UiaCore.UIA.IsTablePatternAvailablePropertyId => true, 
				global::Interop.UiaCore.UIA.IsGridPatternAvailablePropertyId => true, 
				_ => base.GetPropertyValue(propertyID), 
			};
		}

		internal override bool IsPatternSupported(global::Interop.UiaCore.UIA patternId)
		{
			return patternId switch
			{
				global::Interop.UiaCore.UIA.TablePatternId => true, 
				global::Interop.UiaCore.UIA.GridPatternId => true, 
				_ => base.IsPatternSupported(patternId), 
			};
		}

		public AccessibleObject? Next(GridEntry current)
		{
			int rowFromGridEntry = ((PropertyGridView)base.Owner).GetRowFromGridEntry(current);
			return ((PropertyGridView)base.Owner).GetGridEntryFromRow(++rowFromGridEntry)?.AccessibilityObject;
		}

		internal AccessibleObject? GetCategory(int categoryIndex)
		{
			GridEntryCollection topLevelGridEntries = _owningPropertyGridView.TopLevelGridEntries;
			if (topLevelGridEntries.Count > 0 && topLevelGridEntries[categoryIndex] is CategoryGridEntry categoryGridEntry)
			{
				return categoryGridEntry.AccessibilityObject;
			}
			return null;
		}

		internal AccessibleObject? GetLastCategory()
		{
			return GetCategory(_owningPropertyGridView.TopLevelGridEntries.Count - 1);
		}

		internal AccessibleObject? GetLastChild()
		{
			int childCount = GetChildCount();
			if (childCount <= 0)
			{
				return null;
			}
			return GetChild(childCount - 1);
		}

		internal AccessibleObject? GetPreviousGridEntry(GridEntry currentGridEntry, GridEntryCollection gridEntryCollection, out bool currentGridEntryFound)
		{
			GridEntry gridEntry = null;
			currentGridEntryFound = false;
			foreach (GridEntry item in gridEntryCollection)
			{
				if (currentGridEntry == item)
				{
					currentGridEntryFound = true;
					return gridEntry?.AccessibilityObject;
				}
				gridEntry = item;
				if (item.ChildCount > 0)
				{
					AccessibleObject previousGridEntry = GetPreviousGridEntry(currentGridEntry, item.Children, out currentGridEntryFound);
					if (previousGridEntry != null)
					{
						return previousGridEntry;
					}
					if (currentGridEntryFound)
					{
						return null;
					}
				}
			}
			return null;
		}

		internal AccessibleObject? GetNextGridEntry(GridEntry currentGridEntry, GridEntryCollection gridEntryCollection, out bool currentGridEntryFound)
		{
			currentGridEntryFound = false;
			foreach (GridEntry item in gridEntryCollection)
			{
				if (currentGridEntryFound)
				{
					return item.AccessibilityObject;
				}
				if (currentGridEntry == item)
				{
					currentGridEntryFound = true;
				}
				else if (item.ChildCount > 0)
				{
					AccessibleObject nextGridEntry = GetNextGridEntry(currentGridEntry, item.Children, out currentGridEntryFound);
					if (nextGridEntry != null)
					{
						return nextGridEntry;
					}
					if (currentGridEntryFound)
					{
						return null;
					}
				}
			}
			return null;
		}

		internal AccessibleObject? GetFirstChildProperty(CategoryGridEntry current)
		{
			if (current.ChildCount > 0)
			{
				GridEntryCollection children = current.Children;
				if (children != null && children.Count > 0)
				{
					GridEntry[] array = new GridEntry[1];
					try
					{
						_owningPropertyGridView.GetGridEntriesFromOutline(children, 0, 0, array);
					}
					catch (Exception)
					{
					}
					return array[0].AccessibilityObject;
				}
			}
			return null;
		}

		internal AccessibleObject? GetLastChildProperty(CategoryGridEntry current)
		{
			if (current.ChildCount > 0)
			{
				GridEntryCollection children = current.Children;
				if (children != null && children.Count > 0)
				{
					GridEntry[] array = new GridEntry[1];
					try
					{
						_owningPropertyGridView.GetGridEntriesFromOutline(children, 0, children.Count - 1, array);
					}
					catch (Exception)
					{
					}
					return array[0].AccessibilityObject;
				}
			}
			return null;
		}

		internal AccessibleObject? GetNextCategory(CategoryGridEntry current)
		{
			int num = _owningPropertyGridView.GetRowFromGridEntry(current);
			GridEntry gridEntryFromRow;
			do
			{
				gridEntryFromRow = _owningPropertyGridView.GetGridEntryFromRow(++num);
				if (gridEntryFromRow is CategoryGridEntry)
				{
					return gridEntryFromRow.AccessibilityObject;
				}
			}
			while (gridEntryFromRow != null);
			return null;
		}

		public AccessibleObject? Previous(GridEntry current)
		{
			int rowFromGridEntry = ((PropertyGridView)base.Owner).GetRowFromGridEntry(current);
			return ((PropertyGridView)base.Owner).GetGridEntryFromRow(--rowFromGridEntry)?.AccessibilityObject;
		}

		internal AccessibleObject? GetPreviousCategory(CategoryGridEntry current)
		{
			int num = _owningPropertyGridView.GetRowFromGridEntry(current);
			GridEntry gridEntryFromRow;
			do
			{
				gridEntryFromRow = _owningPropertyGridView.GetGridEntryFromRow(--num);
				if (gridEntryFromRow is CategoryGridEntry)
				{
					return gridEntryFromRow.AccessibilityObject;
				}
			}
			while (gridEntryFromRow != null);
			return null;
		}

		public override AccessibleObject? GetChild(int index)
		{
			GridEntryCollection gridEntryCollection = ((PropertyGridView)base.Owner).AccessibilityGetGridEntries();
			if (gridEntryCollection != null && index >= 0 && index < gridEntryCollection.Count)
			{
				return gridEntryCollection.GetEntry(index).AccessibilityObject;
			}
			return null;
		}

		public override int GetChildCount()
		{
			return ((PropertyGridView)base.Owner).AccessibilityGetGridEntries()?.Count ?? 0;
		}

		public override AccessibleObject? GetFocused()
		{
			GridEntry selectedGridEntry = ((PropertyGridView)base.Owner).SelectedGridEntry;
			if (selectedGridEntry != null && selectedGridEntry.HasFocus)
			{
				return selectedGridEntry.AccessibilityObject;
			}
			return null;
		}

		public override AccessibleObject? GetSelected()
		{
			return ((PropertyGridView)base.Owner).SelectedGridEntry?.AccessibilityObject;
		}

		public override AccessibleObject? HitTest(int x, int y)
		{
			if (!base.Owner.IsHandleCreated)
			{
				return null;
			}
			Point lpPoint = new Point(x, y);
			global::Interop.User32.ScreenToClient(new HandleRef(base.Owner, base.Owner.Handle), ref lpPoint);
			Point point = ((PropertyGridView)base.Owner).FindPosition(lpPoint.X, lpPoint.Y);
			if (point != InvalidPosition)
			{
				GridEntry gridEntryFromRow = ((PropertyGridView)base.Owner).GetGridEntryFromRow(point.Y);
				if (gridEntryFromRow != null)
				{
					return gridEntryFromRow.AccessibilityObject;
				}
			}
			return null;
		}

		public override AccessibleObject? Navigate(AccessibleNavigation navdir)
		{
			if (GetChildCount() > 0)
			{
				switch (navdir)
				{
				case AccessibleNavigation.FirstChild:
					return GetChild(0);
				case AccessibleNavigation.LastChild:
					return GetChild(GetChildCount() - 1);
				}
			}
			return null;
		}

		internal override global::Interop.UiaCore.IRawElementProviderSimple? GetItem(int row, int column)
		{
			return GetChild(row);
		}
	}

	private static readonly TraceSwitch s_gridViewDebugPaint = new TraceSwitch("GridViewDebugPaint", "PropertyGridView: Debug property painting");

	private const int EditIndent = 0;

	private const int OutlineIndent = 10;

	private const int OutlineSize = 9;

	private int _outlineSize = 9;

	private const int OutlineSizeExplorerTreeStyle = 16;

	private int _outlineSizeExplorerTreeStyle = 16;

	private const int PaintWidth = 20;

	private int _paintWidth = 20;

	private const int PaintIndent = 26;

	private int _paintIndent = 26;

	private const int MaxListBoxHeight = 200;

	private int _maxListBoxHeight = 200;

	private const int RowLabel = 1;

	private const int RowValue = 2;

	internal const short GdiPlusSpace = 2;

	internal const int MaxRecurseExpand = 10;

	private const int DotDotDotIconWidth = 7;

	private const int DotDotDotIconHeight = 8;

	private const int DownArrowIconWidth = 16;

	private const int DownArrowIconHeight = 16;

	private const int Offset2Pixels = 2;

	private int _offset2Units = 2;

	private Font _boldFont;

	private Color _grayTextColor;

	private bool _grayTextColorModified;

	private GridEntryCollection _allGridEntries;

	private int _visibleRows = -1;

	private int _labelWidth = -1;

	public double _labelRatio = 2.0;

	private short _requiredLabelPaintMargin = 2;

	private int _selectedRow = -1;

	private GridEntry _selectedGridEntry;

	private int _tipInfo = -1;

	private GridViewEdit _edit;

	private DropDownButton _dropDownButton;

	private DropDownButton _dialogButton;

	private GridViewListBox _listBox;

	private DropDownHolder _dropDownHolder;

	private Rectangle _lastClientRect = Rectangle.Empty;

	private Control _currentEditor;

	private ScrollBar _scrollBar;

	private GridToolTip _toolTip;

	private GridErrorDialog _errorDialog;

	private const short FlagNeedsRefresh = 1;

	private const short FlagIsNewSelection = 2;

	private const short FlagIsSplitterMove = 4;

	private const short FlagIsSpecialKey = 8;

	private const short FlagInPropertySet = 16;

	private const short FlagDropDownClosing = 32;

	private const short FlagDropDownCommit = 64;

	private const short FlagNeedUpdateUIBasedOnFont = 128;

	private const short FlagBtnLaunchedEditor = 256;

	private const short FlagNoDefault = 512;

	private const short FlagResizableDropDown = 1024;

	private short _flags = 131;

	private ErrorState _errorState;

	private Point _location = new Point(1, 1);

	private string _originalTextValue;

	private int _cumulativeVerticalWheelDelta;

	private long _rowSelectTime;

	private Point _rowSelectPos = Point.Empty;

	private Point _lastMouseDown = InvalidPosition;

	private int _lastMouseMove;

	private GridEntry _lastClickedEntry;

	private IServiceProvider _serviceProvider;

	private IHelpService _topHelpService;

	private IHelpService _helpService;

	private readonly EventHandler _valueClick;

	private readonly EventHandler _labelClick;

	private readonly EventHandler _outlineClick;

	private readonly EventHandler _valueDoubleClick;

	private readonly EventHandler _labelDoubleClick;

	private readonly GridEntryRecreateChildrenEventHandler _recreateChildren;

	private int _cachedRowHeight = -1;

	private GridPositionData _positionData;

	protected static Point InvalidPoint { get; } = new Point(int.MinValue, int.MinValue);


	protected static Point InvalidPosition { get; } = new Point(int.MinValue, int.MinValue);


	public int TotalProperties { get; private set; } = -1;


	public override Color BackColor
	{
		get
		{
			return base.BackColor;
		}
		set
		{
			base.BackColor = value;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool CanCopy
	{
		get
		{
			if (_selectedGridEntry == null)
			{
				return false;
			}
			if (!Edit.Focused)
			{
				string propertyTextValue = _selectedGridEntry.GetPropertyTextValue();
				if (propertyTextValue != null)
				{
					return propertyTextValue.Length > 0;
				}
				return false;
			}
			return true;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool CanCut
	{
		get
		{
			if (CanCopy && _selectedGridEntry != null)
			{
				return _selectedGridEntry.IsTextEditable;
			}
			return false;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool CanPaste
	{
		get
		{
			if (_selectedGridEntry != null)
			{
				return _selectedGridEntry.IsTextEditable;
			}
			return false;
		}
	}

	[Browsable(false)]
	[EditorBrowsable(EditorBrowsableState.Advanced)]
	public bool CanUndo
	{
		get
		{
			if (!Edit.Visible || !Edit.Focused)
			{
				return false;
			}
			return global::Interop.User32.SendMessageW(Edit, (global::Interop.User32.WM)198u, (IntPtr)0, (IntPtr)0) != IntPtr.Zero;
		}
	}

	internal DropDownButton DropDownButton
	{
		get
		{
			if (_dropDownButton == null)
			{
				_dropDownButton = new DropDownButton
				{
					UseComboBoxTheme = true
				};
				Bitmap image = CreateResizedBitmap("Arrow", 16, 16);
				_dropDownButton.Image = image;
				_dropDownButton.BackColor = SystemColors.Control;
				_dropDownButton.ForeColor = SystemColors.ControlText;
				_dropDownButton.Click += OnButtonClick;
				_dropDownButton.GotFocus += OnDropDownButtonGotFocus;
				_dropDownButton.LostFocus += OnChildLostFocus;
				_dropDownButton.TabIndex = 2;
				CommonEditorSetup(_dropDownButton);
				_dropDownButton.Size = (DpiHelper.IsScalingRequirementMet ? new Size(SystemInformation.VerticalScrollBarArrowHeightForDpi(_deviceDpi), RowHeight) : new Size(SystemInformation.VerticalScrollBarArrowHeight, RowHeight));
			}
			return _dropDownButton;
		}
	}

	private Button DialogButton
	{
		get
		{
			if (_dialogButton == null)
			{
				_dialogButton = new DropDownButton
				{
					BackColor = SystemColors.Control,
					ForeColor = SystemColors.ControlText,
					TabIndex = 3,
					Image = CreateResizedBitmap("dotdotdot", 7, 8)
				};
				_dialogButton.Click += OnButtonClick;
				_dialogButton.KeyDown += OnButtonKeyDown;
				_dialogButton.GotFocus += OnDropDownButtonGotFocus;
				_dialogButton.LostFocus += OnChildLostFocus;
				_dialogButton.Size = (DpiHelper.IsScalingRequirementMet ? new Size(SystemInformation.VerticalScrollBarArrowHeightForDpi(_deviceDpi), RowHeight) : new Size(SystemInformation.VerticalScrollBarArrowHeight, RowHeight));
				CommonEditorSetup(_dialogButton);
			}
			return _dialogButton;
		}
	}

	private GridViewEdit Edit
	{
		get
		{
			if (_edit == null)
			{
				_edit = new GridViewEdit(this)
				{
					BorderStyle = BorderStyle.None,
					AutoSize = false,
					TabStop = false,
					AcceptsReturn = true,
					BackColor = BackColor,
					ForeColor = ForeColor
				};
				_edit.KeyDown += OnEditKeyDown;
				_edit.KeyPress += OnEditKeyPress;
				_edit.GotFocus += OnEditGotFocus;
				_edit.LostFocus += OnEditLostFocus;
				_edit.MouseDown += OnEditMouseDown;
				_edit.TextChanged += OnEditChange;
				_edit.TabIndex = 1;
				CommonEditorSetup(_edit);
			}
			return _edit;
		}
	}

	internal AccessibleObject EditAccessibleObject => Edit.AccessibilityObject;

	internal GridViewListBox DropDownListBox
	{
		get
		{
			if (_listBox == null)
			{
				_listBox = new GridViewListBox(this)
				{
					DrawMode = DrawMode.OwnerDrawFixed
				};
				_listBox.MouseUp += OnListMouseUp;
				_listBox.DrawItem += OnListDrawItem;
				_listBox.SelectedIndexChanged += OnListChange;
				_listBox.KeyDown += OnListKeyDown;
				_listBox.LostFocus += OnChildLostFocus;
				_listBox.Visible = true;
				_listBox.ItemHeight = RowHeight;
			}
			return _listBox;
		}
	}

	internal AccessibleObject DropDownListBoxAccessibleObject
	{
		get
		{
			if (!DropDownListBox.Visible)
			{
				return null;
			}
			return DropDownListBox.AccessibilityObject;
		}
	}

	internal bool DrawValuesRightToLeft
	{
		get
		{
			if (_edit != null && _edit.IsHandleCreated)
			{
				return ((int)(long)global::Interop.User32.GetWindowLong(_edit, global::Interop.User32.GWL.EXSTYLE) & 0x2000) != 0;
			}
			return false;
		}
	}

	internal DropDownHolder DropDownControlHolder => _dropDownHolder;

	internal bool DropDownVisible
	{
		get
		{
			if (_dropDownHolder != null)
			{
				return _dropDownHolder.Visible;
			}
			return false;
		}
	}

	public bool FocusInside
	{
		get
		{
			if (!base.ContainsFocus)
			{
				return DropDownVisible;
			}
			return true;
		}
	}

	internal Color GrayTextColor
	{
		get
		{
			if (_grayTextColorModified)
			{
				return _grayTextColor;
			}
			if (ForeColor.ToArgb() == SystemColors.WindowText.ToArgb())
			{
				return SystemColors.GrayText;
			}
			int num = ForeColor.ToArgb();
			int num2 = (num >> 24) & 0xFF;
			if (num2 != 0)
			{
				num2 /= 2;
				num &= 0xFFFFFF;
				num |= (int)((num2 << 24) & 0xFF000000u);
			}
			else
			{
				num /= 2;
			}
			return Color.FromArgb(num);
		}
		set
		{
			_grayTextColor = value;
			_grayTextColorModified = true;
		}
	}

	private GridErrorDialog ErrorDialog => _errorDialog ?? (_errorDialog = new GridErrorDialog(OwnerGrid));

	private bool HasEntries
	{
		get
		{
			if (TopLevelGridEntries != null)
			{
				return TopLevelGridEntries.Count > 0;
			}
			return false;
		}
	}

	protected int InternalLabelWidth
	{
		get
		{
			if (GetFlag(128))
			{
				UpdateUIBasedOnFont(layoutRequired: true);
			}
			if (_labelWidth == -1)
			{
				SetConstants();
			}
			return _labelWidth;
		}
	}

	internal int LabelPaintMargin
	{
		set
		{
			_requiredLabelPaintMargin = (short)Math.Max(Math.Max(value, _requiredLabelPaintMargin), 2);
		}
	}

	protected bool NeedsCommit
	{
		get
		{
			if (_edit == null || !Edit.Visible)
			{
				return false;
			}
			string text = Edit.Text;
			if (((text == null || text.Length == 0) && (_originalTextValue == null || _originalTextValue.Length == 0)) || (text != null && _originalTextValue != null && text.Equals(_originalTextValue)))
			{
				return false;
			}
			return true;
		}
	}

	public PropertyGrid OwnerGrid { get; private set; }

	protected int RowHeight
	{
		get
		{
			if (_cachedRowHeight == -1)
			{
				_cachedRowHeight = Font.Height + 2;
			}
			return _cachedRowHeight;
		}
	}

	public Point ContextMenuDefaultLocation
	{
		get
		{
			Rectangle rectangle = GetRectangle(_selectedRow, 1);
			Point point = PointToScreen(new Point(rectangle.X, rectangle.Y));
			return new Point(point.X + rectangle.Width / 2, point.Y + rectangle.Height / 2);
		}
	}

	private ScrollBar ScrollBar
	{
		get
		{
			if (_scrollBar == null)
			{
				_scrollBar = new VScrollBar();
				_scrollBar.Scroll += OnScroll;
				base.Controls.Add(_scrollBar);
			}
			return _scrollBar;
		}
	}

	internal GridEntry SelectedGridEntry
	{
		get
		{
			return _selectedGridEntry;
		}
		set
		{
			if (_allGridEntries != null)
			{
				foreach (GridEntry allGridEntry in _allGridEntries)
				{
					if (allGridEntry == value)
					{
						SelectGridEntry(value, pageIn: true);
						return;
					}
				}
			}
			GridEntry gridEntry = FindEquivalentGridEntry(new GridEntryCollection(null, new GridEntry[1] { value }));
			if (gridEntry != null)
			{
				SelectGridEntry(gridEntry, pageIn: true);
				return;
			}
			throw new ArgumentException(System.SR.PropertyGridInvalidGridEntry);
		}
	}

	public IServiceProvider ServiceProvider
	{
		get
		{
			return _serviceProvider;
		}
		set
		{
			if (value != _serviceProvider)
			{
				_serviceProvider = value;
				_topHelpService = null;
				if (_helpService is IDisposable disposable)
				{
					disposable.Dispose();
				}
				_helpService = null;
			}
		}
	}

	internal override bool SupportsUiaProviders => true;

	private int TipColumn
	{
		get
		{
			return (_tipInfo & -65536) >> 16;
		}
		set
		{
			_tipInfo &= 65535;
			_tipInfo |= (value & 0xFFFF) << 16;
		}
	}

	private int TipRow
	{
		get
		{
			return _tipInfo & 0xFFFF;
		}
		set
		{
			_tipInfo &= -65536;
			_tipInfo |= value & 0xFFFF;
		}
	}

	private GridToolTip ToolTip
	{
		get
		{
			if (_toolTip == null)
			{
				_toolTip = new GridToolTip(new Control[2] { this, Edit })
				{
					ToolTip = string.Empty,
					Font = Font
				};
			}
			return _toolTip;
		}
	}

	internal GridEntryCollection TopLevelGridEntries { get; private set; }

	internal bool IsEditTextBoxCreated
	{
		get
		{
			if (_edit != null)
			{
				return _edit.IsHandleCreated;
			}
			return false;
		}
	}

	internal bool IsExplorerTreeSupported
	{
		get
		{
			if (OwnerGrid.CanShowVisualStyleGlyphs)
			{
				return VisualStyleRenderer.IsSupported;
			}
			return false;
		}
	}

	public PropertyGridView(IServiceProvider serviceProvider, PropertyGrid propertyGrid)
	{
		if (DpiHelper.IsScalingRequired)
		{
			_paintWidth = DpiHelper.LogicalToDeviceUnitsX(20);
			_paintIndent = DpiHelper.LogicalToDeviceUnitsX(26);
			_outlineSizeExplorerTreeStyle = DpiHelper.LogicalToDeviceUnitsX(16);
			_outlineSize = DpiHelper.LogicalToDeviceUnitsX(9);
			_maxListBoxHeight = DpiHelper.LogicalToDeviceUnitsY(200);
		}
		_valueClick = OnGridEntryValueClick;
		_labelClick = OnGridEntryLabelClick;
		_outlineClick = OnGridEntryOutlineClick;
		_valueDoubleClick = OnGridEntryValueDoubleClick;
		_labelDoubleClick = OnGridEntryLabelDoubleClick;
		_recreateChildren = OnRecreateChildren;
		OwnerGrid = propertyGrid;
		_serviceProvider = serviceProvider;
		SetStyle(ControlStyles.OptimizedDoubleBuffer, value: true);
		SetStyle(ControlStyles.ResizeRedraw, value: false);
		SetStyle(ControlStyles.UserMouse, value: true);
		BackColor = SystemColors.Window;
		ForeColor = SystemColors.WindowText;
		_grayTextColor = SystemColors.GrayText;
		base.TabStop = true;
		Text = "PropertyGridView";
		CreateUI();
		LayoutWindow(invalidate: true);
	}

	private static Bitmap GetBitmapFromIcon(string iconName, int iconWidth, int iconHeight)
	{
		Size size = new Size(iconWidth, iconHeight);
		Icon icon = new Icon(new Icon(typeof(PropertyGrid), iconName), size);
		Bitmap bitmap = icon.ToBitmap();
		icon.Dispose();
		if (bitmap.Size.Width != iconWidth || bitmap.Size.Height != iconHeight)
		{
			Bitmap bitmap2 = DpiHelper.CreateResizedBitmap(bitmap, size);
			if (bitmap2 != null)
			{
				bitmap.Dispose();
				bitmap = bitmap2;
			}
		}
		return bitmap;
	}

	internal GridEntryCollection AccessibilityGetGridEntries()
	{
		return GetAllGridEntries();
	}

	internal Rectangle AccessibilityGetGridEntryBounds(GridEntry gridEntry)
	{
		int rowFromGridEntry = GetRowFromGridEntry(gridEntry);
		if (rowFromGridEntry < 0)
		{
			return Rectangle.Empty;
		}
		Rectangle rectangle = GetRectangle(rowFromGridEntry, 3);
		Point lpPoint = new Point(rectangle.X, rectangle.Y);
		global::Interop.User32.ClientToScreen(new HandleRef(this, base.Handle), ref lpPoint);
		int num = gridEntry.OwnerGrid.GridViewAccessibleObject.Bounds.Bottom - 1;
		if (lpPoint.Y > num)
		{
			return Rectangle.Empty;
		}
		if (lpPoint.Y + rectangle.Height > num)
		{
			rectangle.Height = num - lpPoint.Y;
		}
		return new Rectangle(lpPoint.X, lpPoint.Y, rectangle.Width, rectangle.Height);
	}

	internal int AccessibilityGetGridEntryChildID(GridEntry gridEntry)
	{
		GridEntryCollection allGridEntries = GetAllGridEntries();
		if (allGridEntries == null)
		{
			return -1;
		}
		for (int i = 0; i < allGridEntries.Count; i++)
		{
			if (allGridEntries[i].Equals(gridEntry))
			{
				return i;
			}
		}
		return -1;
	}

	internal void AccessibilitySelect(GridEntry entry)
	{
		SelectGridEntry(entry, pageIn: true);
		Focus();
	}

	private void AddGridEntryEvents(GridEntryCollection entries, int startIndex, int count)
	{
		if (entries == null)
		{
			return;
		}
		if (count == -1)
		{
			count = entries.Count - startIndex;
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			if (entries[i] != null)
			{
				GridEntry entry = entries.GetEntry(i);
				entry.AddOnValueClick(_valueClick);
				entry.AddOnLabelClick(_labelClick);
				entry.AddOnOutlineClick(_outlineClick);
				entry.AddOnOutlineDoubleClick(_outlineClick);
				entry.AddOnValueDoubleClick(_valueDoubleClick);
				entry.AddOnLabelDoubleClick(_labelDoubleClick);
				entry.AddOnRecreateChildren(_recreateChildren);
			}
		}
	}

	protected virtual void AdjustOrigin(Graphics g, Point newOrigin, ref Rectangle r)
	{
		g.ResetTransform();
		g.TranslateTransform(newOrigin.X, newOrigin.Y);
		r.Offset(-newOrigin.X, -newOrigin.Y);
	}

	private void CancelSplitterMove()
	{
		if (GetFlag(4))
		{
			SetFlag(4, value: false);
			base.Capture = false;
			if (_selectedRow != -1)
			{
				SelectRow(_selectedRow);
			}
		}
	}

	internal GridPositionData CaptureGridPositionData()
	{
		return new GridPositionData(this);
	}

	private void ClearGridEntryEvents(GridEntryCollection entries, int startIndex, int count)
	{
		if (entries == null)
		{
			return;
		}
		if (count == -1)
		{
			count = entries.Count - startIndex;
		}
		for (int i = startIndex; i < startIndex + count; i++)
		{
			if (entries[i] != null)
			{
				GridEntry entry = entries.GetEntry(i);
				entry.RemoveOnValueClick(_valueClick);
				entry.RemoveOnLabelClick(_labelClick);
				entry.RemoveOnOutlineClick(_outlineClick);
				entry.RemoveOnOutlineDoubleClick(_outlineClick);
				entry.RemoveOnValueDoubleClick(_valueDoubleClick);
				entry.RemoveOnLabelDoubleClick(_labelDoubleClick);
				entry.RemoveOnRecreateChildren(_recreateChildren);
			}
		}
	}

	public void ClearProps()
	{
		if (HasEntries)
		{
			CommonEditorHide();
			TopLevelGridEntries = null;
			ClearGridEntryEvents(_allGridEntries, 0, -1);
			_allGridEntries = null;
			_selectedRow = -1;
			_tipInfo = -1;
		}
	}

	public void CloseDropDown()
	{
		CloseDropDownInternal(resetFocus: true);
	}

	private void CloseDropDownInternal(bool resetFocus)
	{
		if (GetFlag(32) || _dropDownHolder == null || !_dropDownHolder.Visible)
		{
			return;
		}
		try
		{
			SetFlag(32, value: true);
			if (_dropDownHolder.Component == DropDownListBox && GetFlag(64))
			{
				OnListClick(null, null);
			}
			Edit.Filter = false;
			_dropDownHolder.SetComponent(null, resizable: false);
			_dropDownHolder.Visible = false;
			if (resetFocus)
			{
				if (DialogButton.Visible)
				{
					DialogButton.Focus();
				}
				else if (DropDownButton.Visible)
				{
					DropDownButton.Focus();
				}
				else if (Edit.Visible)
				{
					Edit.Focus();
				}
				else
				{
					Focus();
				}
				if (_selectedRow != -1)
				{
					SelectRow(_selectedRow);
				}
			}
			if (_selectedRow != -1)
			{
				GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
				if (gridEntryFromRow != null)
				{
					gridEntryFromRow.AccessibilityObject.RaiseAutomationEvent(global::Interop.UiaCore.UIA.AutomationFocusChangedEventId);
					gridEntryFromRow.AccessibilityObject.RaiseAutomationPropertyChangedEvent(global::Interop.UiaCore.UIA.ExpandCollapseExpandCollapseStatePropertyId, global::Interop.UiaCore.ExpandCollapseState.Expanded, global::Interop.UiaCore.ExpandCollapseState.Collapsed);
				}
			}
		}
		finally
		{
			SetFlag(32, value: false);
		}
	}

	private void CommonEditorHide()
	{
		CommonEditorHide(always: false);
	}

	private void CommonEditorHide(bool always)
	{
		if (!always && !HasEntries)
		{
			return;
		}
		CloseDropDown();
		bool flag = false;
		if ((Edit.Focused || DialogButton.Focused || DropDownButton.Focused) && base.IsHandleCreated && base.Visible && base.Enabled)
		{
			flag = IntPtr.Zero != global::Interop.User32.SetFocus(new HandleRef(this, base.Handle));
		}
		try
		{
			Edit.DontFocus = true;
			if (Edit.Focused && !flag)
			{
				flag = Focus();
			}
			Edit.Visible = false;
			Edit.SelectionStart = 0;
			Edit.SelectionLength = 0;
			if (DialogButton.Focused && !flag)
			{
				flag = Focus();
			}
			DialogButton.Visible = false;
			if (DropDownButton.Focused && !flag)
			{
				flag = Focus();
			}
			DropDownButton.Visible = false;
			_currentEditor = null;
		}
		finally
		{
			Edit.DontFocus = false;
		}
	}

	protected virtual void CommonEditorSetup(Control control)
	{
		control.Visible = false;
		base.Controls.Add(control);
	}

	protected virtual void CommonEditorUse(Control control, Rectangle targetRectangle)
	{
		Rectangle bounds = control.Bounds;
		Rectangle clientRectangle = base.ClientRectangle;
		clientRectangle.Inflate(-1, -1);
		try
		{
			targetRectangle = Rectangle.Intersect(clientRectangle, targetRectangle);
			if (!targetRectangle.IsEmpty)
			{
				if (!targetRectangle.Equals(bounds))
				{
					control.SetBounds(targetRectangle.X, targetRectangle.Y, targetRectangle.Width, targetRectangle.Height);
				}
				control.Visible = true;
			}
		}
		catch
		{
			targetRectangle = Rectangle.Empty;
		}
		if (targetRectangle.IsEmpty)
		{
			control.Visible = false;
		}
		_currentEditor = control;
	}

	private int CountPropsFromOutline(GridEntryCollection entries)
	{
		if (entries == null)
		{
			return 0;
		}
		int num = entries.Count;
		for (int i = 0; i < entries.Count; i++)
		{
			if (((GridEntry)entries[i]).InternalExpanded)
			{
				num += CountPropsFromOutline(((GridEntry)entries[i]).Children);
			}
		}
		return num;
	}

	protected override AccessibleObject CreateAccessibilityInstance()
	{
		return new PropertyGridViewAccessibleObject(this, OwnerGrid);
	}

	private Bitmap CreateResizedBitmap(string icon, int width, int height)
	{
		int num = width;
		int num2 = height;
		try
		{
			if (DpiHelper.IsPerMonitorV2Awareness)
			{
				num = LogicalToDeviceUnits(width);
				num2 = LogicalToDeviceUnits(height);
			}
			else if (DpiHelper.IsScalingRequired)
			{
				num = DpiHelper.LogicalToDeviceUnitsX(width);
				num2 = DpiHelper.LogicalToDeviceUnitsY(height);
			}
			return GetBitmapFromIcon(icon, num, num2);
		}
		catch (Exception)
		{
			return new Bitmap(num, num2);
		}
	}

	protected virtual void CreateUI()
	{
		UpdateUIBasedOnFont(layoutRequired: false);
	}

	protected override void Dispose(bool disposing)
	{
		if (disposing)
		{
			_scrollBar?.Dispose();
			_listBox?.Dispose();
			_dropDownHolder?.Dispose();
			_scrollBar = null;
			_listBox = null;
			_dropDownHolder = null;
			OwnerGrid = null;
			TopLevelGridEntries = null;
			_allGridEntries = null;
			_serviceProvider = null;
			_topHelpService = null;
			if (_helpService != null && _helpService is IDisposable disposable)
			{
				disposable.Dispose();
			}
			_helpService = null;
			_edit?.Dispose();
			_edit = null;
			_boldFont?.Dispose();
			_boldFont = null;
			_dropDownButton?.Dispose();
			_dropDownButton = null;
			_dialogButton?.Dispose();
			_dialogButton = null;
			_toolTip?.Dispose();
			_toolTip = null;
		}
		base.Dispose(disposing);
	}

	public void DoCopyCommand()
	{
		if (CanCopy)
		{
			if (Edit.Focused)
			{
				Edit.Copy();
			}
			else if (_selectedGridEntry != null)
			{
				Clipboard.SetDataObject(_selectedGridEntry.GetPropertyTextValue());
			}
		}
	}

	public void DoCutCommand()
	{
		if (CanCut)
		{
			DoCopyCommand();
			if (Edit.Visible)
			{
				Edit.Cut();
			}
		}
	}

	public void DoPasteCommand()
	{
		if (!CanPaste || !Edit.Visible)
		{
			return;
		}
		if (Edit.Focused)
		{
			Edit.Paste();
			return;
		}
		IDataObject dataObject = Clipboard.GetDataObject();
		if (dataObject != null)
		{
			string text = (string)dataObject.GetData(typeof(string));
			if (text != null)
			{
				Edit.Focus();
				Edit.Text = text;
				SetCommitError(ErrorState.None, capture: true);
			}
		}
	}

	public void DoUndoCommand()
	{
		if (CanUndo && Edit.Visible)
		{
			global::Interop.User32.SendMessageW(Edit, global::Interop.User32.WM.UNDO, (IntPtr)0, (IntPtr)0);
		}
	}

	private int GetEntryLabelIndent(GridEntry gridEntry)
	{
		return gridEntry.PropertyLabelIndent + 1;
	}

	private int GetEntryLabelLength(Graphics g, GridEntry gridEntry)
	{
		Size size = Size.Ceiling(PropertyGrid.MeasureTextHelper.MeasureText(OwnerGrid, g, gridEntry.PropertyLabel, Font));
		return _location.X + GetEntryLabelIndent(gridEntry) + size.Width;
	}

	private bool IsEntryLabelLong(Graphics g, GridEntry gridEntry)
	{
		if (gridEntry == null)
		{
			return false;
		}
		return GetEntryLabelLength(g, gridEntry) > _location.X + InternalLabelWidth;
	}

	protected void DrawLabel(Graphics g, int row, Rectangle rect, bool selected, bool longLabelrequest, Rectangle clipRect)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(row);
		if (gridEntryFromRow == null || rect.IsEmpty)
		{
			return;
		}
		Point newOrigin = new Point(rect.X, rect.Y);
		clipRect = Rectangle.Intersect(rect, clipRect);
		if (clipRect.IsEmpty)
		{
			return;
		}
		AdjustOrigin(g, newOrigin, ref rect);
		clipRect.Offset(-newOrigin.X, -newOrigin.Y);
		try
		{
			bool paintFullLabel = false;
			GetEntryLabelIndent(gridEntryFromRow);
			if (longLabelrequest)
			{
				GetEntryLabelLength(g, gridEntryFromRow);
				paintFullLabel = IsEntryLabelLong(g, gridEntryFromRow);
			}
			gridEntryFromRow.PaintLabel(g, rect, clipRect, selected, paintFullLabel);
		}
		catch (Exception)
		{
		}
		finally
		{
			ResetOrigin(g);
		}
	}

	protected virtual void DrawValueEntry(Graphics g, int row, Rectangle clipRect)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(row);
		if (gridEntryFromRow == null)
		{
			return;
		}
		Rectangle r = GetRectangle(row, 2);
		Point newOrigin = new Point(r.X, r.Y);
		clipRect = Rectangle.Intersect(clipRect, r);
		if (clipRect.IsEmpty)
		{
			return;
		}
		AdjustOrigin(g, newOrigin, ref r);
		clipRect.Offset(-newOrigin.X, -newOrigin.Y);
		try
		{
			gridEntryFromRow.PaintValue(null, g, r, clipRect, GridEntry.PaintValueFlags.FetchValue | GridEntry.PaintValueFlags.CheckShouldSerialize | GridEntry.PaintValueFlags.PaintInPlace);
		}
		catch (Exception)
		{
		}
		finally
		{
			ResetOrigin(g);
		}
	}

	private void F4Selection(bool popupModalDialog)
	{
		if (GetGridEntryFromRow(_selectedRow) == null)
		{
			return;
		}
		if (_errorState != 0 && Edit.Visible)
		{
			Edit.Focus();
		}
		else if (DropDownButton.Visible)
		{
			PopupDialog(_selectedRow);
		}
		else if (DialogButton.Visible)
		{
			if (popupModalDialog)
			{
				PopupDialog(_selectedRow);
			}
			else
			{
				DialogButton.Focus();
			}
		}
		else if (Edit.Visible)
		{
			Edit.Focus();
			SelectEdit();
		}
	}

	public void DoubleClickRow(int row, bool toggleExpand, int type)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(row);
		if (gridEntryFromRow == null)
		{
			return;
		}
		if (!toggleExpand || type == 2)
		{
			try
			{
				if (gridEntryFromRow.DoubleClickPropertyValue())
				{
					SelectRow(row);
					return;
				}
			}
			catch (Exception ex)
			{
				SetCommitError(ErrorState.Thrown);
				ShowInvalidMessage(gridEntryFromRow.PropertyLabel, ex);
				return;
			}
		}
		SelectGridEntry(gridEntryFromRow, pageIn: true);
		if (type == 1 && toggleExpand && gridEntryFromRow.Expandable)
		{
			SetExpand(gridEntryFromRow, !gridEntryFromRow.InternalExpanded);
			return;
		}
		if (gridEntryFromRow.IsValueEditable && gridEntryFromRow.Enumerable)
		{
			int currentValueIndex = GetCurrentValueIndex(gridEntryFromRow);
			if (currentValueIndex != -1)
			{
				object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
				currentValueIndex = ((propertyValueList != null && currentValueIndex < propertyValueList.Length - 1) ? (currentValueIndex + 1) : 0);
				CommitValue(propertyValueList[currentValueIndex]);
				SelectRow(_selectedRow);
				Refresh();
				return;
			}
		}
		if (Edit.Visible)
		{
			Edit.Focus();
			SelectEdit();
		}
	}

	public Font GetBaseFont()
	{
		return Font;
	}

	public Font GetBoldFont()
	{
		return _boldFont ?? (_boldFont = new Font(Font, FontStyle.Bold));
	}

	internal GridEntry GetElementFromPoint(int x, int y)
	{
		Point pt = new Point(x, y);
		GridEntryCollection allGridEntries = GetAllGridEntries();
		GridEntry[] array = new GridEntry[allGridEntries.Count];
		try
		{
			GetGridEntriesFromOutline(allGridEntries, 0, allGridEntries.Count - 1, array);
		}
		catch (Exception)
		{
		}
		GridEntry[] array2 = array;
		foreach (GridEntry gridEntry in array2)
		{
			if (gridEntry.AccessibilityObject.Bounds.Contains(pt))
			{
				return gridEntry;
			}
		}
		return null;
	}

	private bool GetFlag(short flag)
	{
		return (_flags & flag) != 0;
	}

	public virtual Color GetLineColor()
	{
		return OwnerGrid.LineColor;
	}

	public virtual Color GetSelectedItemWithFocusForeColor()
	{
		return OwnerGrid.SelectedItemWithFocusForeColor;
	}

	public virtual Color GetSelectedItemWithFocusBackColor()
	{
		return OwnerGrid.SelectedItemWithFocusBackColor;
	}

	public virtual IntPtr GetHostHandle()
	{
		return base.Handle;
	}

	public virtual int GetLabelWidth()
	{
		return InternalLabelWidth;
	}

	public virtual int GetOutlineIconSize()
	{
		if (!IsExplorerTreeSupported)
		{
			return _outlineSize;
		}
		return _outlineSizeExplorerTreeStyle;
	}

	public virtual int GetGridEntryHeight()
	{
		return RowHeight;
	}

	internal int GetPropertyLocation(string propName, bool getXY, bool rowValue)
	{
		if (_allGridEntries == null || _allGridEntries.Count <= 0)
		{
			return -1;
		}
		for (int i = 0; i < _allGridEntries.Count; i++)
		{
			if (string.Compare(propName, _allGridEntries.GetEntry(i).PropertyLabel, ignoreCase: true, CultureInfo.InvariantCulture) != 0)
			{
				continue;
			}
			if (getXY)
			{
				int rowFromGridEntry = GetRowFromGridEntry(_allGridEntries.GetEntry(i));
				if (rowFromGridEntry < 0 || rowFromGridEntry >= _visibleRows)
				{
					return -1;
				}
				Rectangle rectangle = GetRectangle(rowFromGridEntry, (!rowValue) ? 1 : 2);
				return global::Interop.PARAM.ToInt(rectangle.X, rectangle.Y);
			}
			return i;
		}
		return -1;
	}

	public new object GetService(Type classService)
	{
		if (classService == typeof(IWindowsFormsEditorService))
		{
			return this;
		}
		if (ServiceProvider != null)
		{
			return _serviceProvider.GetService(classService);
		}
		return null;
	}

	public virtual int GetSplitterWidth()
	{
		return 1;
	}

	public virtual int GetTotalWidth()
	{
		return GetLabelWidth() + GetSplitterWidth() + GetValueWidth();
	}

	public virtual int GetValuePaintIndent()
	{
		return _paintIndent;
	}

	public virtual int GetValuePaintWidth()
	{
		return _paintWidth;
	}

	public virtual int GetValueStringIndent()
	{
		return 0;
	}

	public virtual int GetValueWidth()
	{
		return (int)((double)InternalLabelWidth * (_labelRatio - 1.0));
	}

	public void DropDownControl(Control ctl)
	{
		if (_dropDownHolder == null)
		{
			_dropDownHolder = new DropDownHolder(this);
		}
		_dropDownHolder.Visible = false;
		_dropDownHolder.SetComponent(ctl, GetFlag(1024));
		Rectangle rectangle = GetRectangle(_selectedRow, 2);
		Size size = _dropDownHolder.Size;
		Point point = PointToScreen(new Point(0, 0));
		Rectangle workingArea = Screen.FromControl(Edit).WorkingArea;
		size.Width = Math.Max(rectangle.Width + 1, size.Width);
		point.X = Math.Min(workingArea.X + workingArea.Width - size.Width, Math.Max(workingArea.X, point.X + rectangle.X + rectangle.Width - size.Width));
		point.Y += rectangle.Y;
		if (workingArea.Y + workingArea.Height < size.Height + point.Y + Edit.Height)
		{
			point.Y -= size.Height;
			_dropDownHolder.ResizeUp = true;
		}
		else
		{
			point.Y += rectangle.Height + 1;
			_dropDownHolder.ResizeUp = false;
		}
		global::Interop.User32.SetWindowLong(_dropDownHolder, global::Interop.User32.GWL.HWNDPARENT, new HandleRef(this, base.Handle));
		_dropDownHolder.SetBounds(point.X, point.Y, size.Width, size.Height);
		global::Interop.User32.ShowWindow(_dropDownHolder, global::Interop.User32.SW.SHOWNA);
		Edit.Filter = true;
		_dropDownHolder.Visible = true;
		_dropDownHolder.FocusComponent();
		SelectEdit();
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		if (gridEntryFromRow != null)
		{
			gridEntryFromRow.AccessibilityObject.RaiseAutomationEvent(global::Interop.UiaCore.UIA.AutomationFocusChangedEventId);
			gridEntryFromRow.AccessibilityObject.RaiseAutomationPropertyChangedEvent(global::Interop.UiaCore.UIA.ExpandCollapseExpandCollapseStatePropertyId, global::Interop.UiaCore.ExpandCollapseState.Collapsed, global::Interop.UiaCore.ExpandCollapseState.Expanded);
		}
		try
		{
			DropDownButton.IgnoreMouse = true;
			_dropDownHolder.DoModalLoop();
		}
		finally
		{
			DropDownButton.IgnoreMouse = false;
		}
		if (_selectedRow != -1)
		{
			Focus();
			SelectRow(_selectedRow);
		}
	}

	public virtual void DropDownDone()
	{
		CloseDropDown();
	}

	public virtual void DropDownUpdate()
	{
		if (_dropDownHolder != null && _dropDownHolder.GetUsed())
		{
			int selectedRow = _selectedRow;
			GridEntry gridEntryFromRow = GetGridEntryFromRow(selectedRow);
			Edit.Text = gridEntryFromRow.GetPropertyTextValue();
		}
	}

	public bool EnsurePendingChangesCommitted()
	{
		CloseDropDown();
		return Commit();
	}

	private bool FilterEditWndProc(ref Message m)
	{
		if (_dropDownHolder != null && _dropDownHolder.Visible && m.Msg == 256 && (int)m.WParam != 9)
		{
			Control component = _dropDownHolder.Component;
			if (component != null)
			{
				m.Result = global::Interop.User32.SendMessageW(component, (global::Interop.User32.WM)m.Msg, m.WParam, m.LParam);
				return true;
			}
		}
		return false;
	}

	private bool FilterReadOnlyEditKeyPress(char keyChar)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		if (gridEntryFromRow.Enumerable && gridEntryFromRow.IsValueEditable)
		{
			int currentValueIndex = GetCurrentValueIndex(gridEntryFromRow);
			object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
			string strB = new string(new char[1] { keyChar });
			for (int i = 0; i < propertyValueList.Length; i++)
			{
				object value = propertyValueList[(i + currentValueIndex + 1) % propertyValueList.Length];
				string propertyTextValue = gridEntryFromRow.GetPropertyTextValue(value);
				if (propertyTextValue != null && propertyTextValue.Length > 0 && string.Compare(propertyTextValue.Substring(0, 1), strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					CommitValue(value);
					if (Edit.Focused)
					{
						SelectEdit();
					}
					return true;
				}
			}
		}
		return false;
	}

	public virtual bool WillFilterKeyPress(char charPressed)
	{
		if (!Edit.Visible)
		{
			return false;
		}
		if (((uint)Control.ModifierKeys & 0xFFFEFFFFu) != 0)
		{
			return false;
		}
		if (_selectedGridEntry != null)
		{
			switch (charPressed)
			{
			case '*':
			case '+':
			case '-':
				return !_selectedGridEntry.Expandable;
			case '\t':
				return false;
			}
		}
		return true;
	}

	public void FilterKeyPress(char keyChar)
	{
		if (GetGridEntryFromRow(_selectedRow) != null)
		{
			Edit.FilterKeyPress(keyChar);
		}
	}

	private GridEntry FindEquivalentGridEntry(GridEntryCollection ipeHier)
	{
		if (ipeHier == null || ipeHier.Count == 0)
		{
			return null;
		}
		GridEntryCollection allGridEntries = GetAllGridEntries();
		if (allGridEntries == null || allGridEntries.Count == 0)
		{
			return null;
		}
		GridEntry gridEntry = null;
		int i = 0;
		int num = allGridEntries.Count;
		for (int j = 0; j < ipeHier.Count; j++)
		{
			if (ipeHier[j] == null)
			{
				continue;
			}
			if (gridEntry != null)
			{
				if (!gridEntry.InternalExpanded)
				{
					SetExpand(gridEntry, value: true);
					allGridEntries = GetAllGridEntries();
				}
				num = gridEntry.VisibleChildCount;
			}
			int num2 = i;
			gridEntry = null;
			for (; i < allGridEntries.Count && i - num2 <= num; i++)
			{
				if (ipeHier.GetEntry(j).NonParentEquals(allGridEntries[i]))
				{
					gridEntry = allGridEntries.GetEntry(i);
					i++;
					break;
				}
			}
			if (gridEntry == null)
			{
				break;
			}
		}
		return gridEntry;
	}

	protected virtual Point FindPosition(int x, int y)
	{
		if (RowHeight == -1)
		{
			return InvalidPosition;
		}
		Size ourSize = GetOurSize();
		if (x < 0 || x > ourSize.Width + _location.X)
		{
			return InvalidPosition;
		}
		Point result = new Point(1, 0);
		if (x > InternalLabelWidth + _location.X)
		{
			result.X = 2;
		}
		result.Y = (y - _location.Y) / (1 + RowHeight);
		return result;
	}

	public virtual void Flush()
	{
		if (Commit() && Edit.Focused)
		{
			Focus();
		}
	}

	private GridEntryCollection GetAllGridEntries()
	{
		return GetAllGridEntries(updateCache: false);
	}

	private GridEntryCollection GetAllGridEntries(bool updateCache)
	{
		if (_visibleRows == -1 || TotalProperties == -1 || !HasEntries)
		{
			return null;
		}
		if (_allGridEntries != null && !updateCache)
		{
			return _allGridEntries;
		}
		GridEntry[] array = new GridEntry[TotalProperties];
		try
		{
			GetGridEntriesFromOutline(TopLevelGridEntries, 0, 0, array);
		}
		catch (Exception)
		{
		}
		_allGridEntries = new GridEntryCollection(null, array);
		AddGridEntryEvents(_allGridEntries, 0, -1);
		return _allGridEntries;
	}

	private int GetCurrentValueIndex(GridEntry gridEntry)
	{
		if (!gridEntry.Enumerable)
		{
			return -1;
		}
		try
		{
			object[] propertyValueList = gridEntry.GetPropertyValueList();
			object propertyValue = gridEntry.PropertyValue;
			string strA = gridEntry.TypeConverter.ConvertToString(gridEntry, propertyValue);
			if (propertyValueList == null || propertyValueList.Length == 0)
			{
				return -1;
			}
			int num = -1;
			int num2 = -1;
			for (int i = 0; i < propertyValueList.Length; i++)
			{
				object obj = propertyValueList[i];
				string strB = gridEntry.TypeConverter.ConvertToString(obj);
				if (propertyValue == obj || string.Compare(strA, strB, ignoreCase: true, CultureInfo.InvariantCulture) == 0)
				{
					num = i;
				}
				if (propertyValue != null && obj != null && obj.Equals(propertyValue))
				{
					num2 = i;
				}
				if (num == num2 && num != -1)
				{
					return num;
				}
			}
			if (num != -1)
			{
				return num;
			}
			if (num2 != -1)
			{
				return num2;
			}
			return -1;
		}
		catch (Exception)
		{
		}
		return -1;
	}

	public virtual int GetDefaultOutlineIndent()
	{
		return 10;
	}

	private IHelpService GetHelpService()
	{
		if (_helpService == null && ServiceProvider.TryGetService<IHelpService>(out _topHelpService))
		{
			IHelpService helpService = _topHelpService.CreateLocalContext(HelpContextType.ToolWindowSelection);
			if (helpService != null)
			{
				_helpService = helpService;
			}
		}
		return _helpService;
	}

	public virtual int GetScrollOffset()
	{
		if (_scrollBar == null)
		{
			return 0;
		}
		return ScrollBar.Value;
	}

	private GridEntryCollection GetGridEntryHierarchy(GridEntry gridEntry)
	{
		if (gridEntry == null)
		{
			return null;
		}
		int propertyDepth = gridEntry.PropertyDepth;
		if (propertyDepth > 0)
		{
			GridEntry[] array = new GridEntry[propertyDepth + 1];
			while (gridEntry != null && propertyDepth >= 0)
			{
				array[propertyDepth] = gridEntry;
				gridEntry = gridEntry.ParentGridEntry;
				propertyDepth = gridEntry.PropertyDepth;
			}
			return new GridEntryCollection(null, array);
		}
		return new GridEntryCollection(null, new GridEntry[1] { gridEntry });
	}

	private GridEntry GetGridEntryFromRow(int row)
	{
		return GetGridEntryFromOffset(row + GetScrollOffset());
	}

	private GridEntry GetGridEntryFromOffset(int offset)
	{
		GridEntryCollection allGridEntries = GetAllGridEntries();
		if (allGridEntries != null && offset >= 0 && offset < allGridEntries.Count)
		{
			return allGridEntries.GetEntry(offset);
		}
		return null;
	}

	private int GetGridEntriesFromOutline(GridEntryCollection entries, int current, int target, GridEntry[] targetEntries)
	{
		if (entries == null || entries.Count == 0)
		{
			return current;
		}
		current--;
		for (int i = 0; i < entries.Count; i++)
		{
			current++;
			if (current >= target + targetEntries.Length)
			{
				break;
			}
			GridEntry entry = entries.GetEntry(i);
			if (current >= target)
			{
				targetEntries[current - target] = entry;
			}
			if (entry.InternalExpanded)
			{
				GridEntryCollection children = entry.Children;
				if (children != null && children.Count > 0)
				{
					current = GetGridEntriesFromOutline(children, current + 1, target, targetEntries);
				}
			}
		}
		return current;
	}

	private Size GetOurSize()
	{
		Size clientSize = base.ClientSize;
		if (clientSize.Width == 0)
		{
			Size size = base.Size;
			if (size.Width > 10)
			{
				clientSize.Width = size.Width;
				clientSize.Height = size.Height;
			}
		}
		if (!GetScrollbarHidden())
		{
			Size size2 = ScrollBar.Size;
			clientSize.Width -= size2.Width;
		}
		clientSize.Width -= 2;
		clientSize.Height -= 2;
		return clientSize;
	}

	public Rectangle GetRectangle(int row, int flRow)
	{
		Rectangle result = new Rectangle(0, 0, 0, 0);
		Size ourSize = GetOurSize();
		result.X = _location.X;
		bool flag = (flRow & 1) != 0;
		bool flag2 = (flRow & 2) != 0;
		if (flag && flag2)
		{
			result.X = 1;
			result.Width = ourSize.Width - 1;
		}
		else if (flag)
		{
			result.X = 1;
			result.Width = InternalLabelWidth - 1;
		}
		else if (flag2)
		{
			result.X = _location.X + InternalLabelWidth;
			result.Width = ourSize.Width - InternalLabelWidth;
		}
		result.Y = row * (RowHeight + 1) + 1 + _location.Y;
		result.Height = RowHeight;
		return result;
	}

	internal int GetRowFromGridEntry(GridEntry gridEntry)
	{
		GridEntryCollection allGridEntries = GetAllGridEntries();
		if (gridEntry == null || allGridEntries == null)
		{
			return -1;
		}
		int num = -1;
		for (int i = 0; i < allGridEntries.Count; i++)
		{
			if (gridEntry == allGridEntries[i])
			{
				return i - GetScrollOffset();
			}
			if (num == -1 && gridEntry.Equals(allGridEntries[i]))
			{
				num = i - GetScrollOffset();
			}
		}
		if (num != -1)
		{
			return num;
		}
		return -1 - GetScrollOffset();
	}

	public virtual bool GetInPropertySet()
	{
		return GetFlag(16);
	}

	protected virtual bool GetScrollbarHidden()
	{
		if (_scrollBar != null)
		{
			return !ScrollBar.Visible;
		}
		return true;
	}

	public virtual string GetTestingInfo(int entry)
	{
		GridEntry gridEntry = ((entry < 0) ? GetGridEntryFromRow(_selectedRow) : GetGridEntryFromOffset(entry));
		if (gridEntry != null)
		{
			return gridEntry.GetTestingInfo();
		}
		return string.Empty;
	}

	public Color GetTextColor()
	{
		return ForeColor;
	}

	private void LayoutWindow(bool invalidate)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		Size size = new Size(clientRectangle.Width, clientRectangle.Height);
		if (_scrollBar != null)
		{
			Rectangle bounds = ScrollBar.Bounds;
			bounds.X = size.Width - bounds.Width - 1;
			bounds.Y = 1;
			bounds.Height = size.Height - 2;
			ScrollBar.Bounds = bounds;
		}
		if (invalidate)
		{
			Invalidate();
		}
	}

	internal void InvalidateGridEntryValue(GridEntry ge)
	{
		int rowFromGridEntry = GetRowFromGridEntry(ge);
		if (rowFromGridEntry != -1)
		{
			InvalidateRows(rowFromGridEntry, rowFromGridEntry, 2);
		}
	}

	private void InvalidateRow(int row)
	{
		InvalidateRows(row, row, 3);
	}

	private void InvalidateRows(int startRow, int endRow)
	{
		InvalidateRows(startRow, endRow, 3);
	}

	private void InvalidateRows(int startRow, int endRow, int type)
	{
		if (endRow == -1)
		{
			Rectangle rectangle = GetRectangle(startRow, type);
			rectangle.Height = base.Size.Height - rectangle.Y - 1;
			Invalidate(rectangle);
			return;
		}
		for (int i = startRow; i <= endRow; i++)
		{
			Rectangle rectangle = GetRectangle(i, type);
			Invalidate(rectangle);
		}
	}

	protected override bool IsInputKey(Keys keyData)
	{
		switch (keyData & Keys.KeyCode)
		{
		case Keys.Tab:
		case Keys.Escape:
		case Keys.F4:
			return false;
		case Keys.Return:
			if (Edit.Focused)
			{
				return false;
			}
			break;
		}
		return base.IsInputKey(keyData);
	}

	private bool IsMyChild(Control control)
	{
		if (control == this || control == null)
		{
			return false;
		}
		for (Control parentInternal = control.ParentInternal; parentInternal != null; parentInternal = parentInternal.ParentInternal)
		{
			if (parentInternal == this)
			{
				return true;
			}
		}
		return false;
	}

	private bool IsScrollValueValid(int newValue)
	{
		if (newValue == ScrollBar.Value || newValue < 0 || newValue > ScrollBar.Maximum || newValue + (ScrollBar.LargeChange - 1) >= TotalProperties)
		{
			return false;
		}
		return true;
	}

	internal bool IsSiblingControl(Control control1, Control control2)
	{
		Control parentInternal = control1.ParentInternal;
		for (Control parentInternal2 = control2.ParentInternal; parentInternal2 != null; parentInternal2 = parentInternal2.ParentInternal)
		{
			if (parentInternal == parentInternal2)
			{
				return true;
			}
		}
		return false;
	}

	private void MoveSplitterTo(int xPosition)
	{
		int width = GetOurSize().Width;
		int x = _location.X;
		int num = Math.Max(Math.Min(xPosition, width - 10), GetOutlineIconSize() * 2);
		int internalLabelWidth = InternalLabelWidth;
		_labelRatio = (double)width / (double)(num - x);
		SetConstants();
		if (_selectedRow != -1)
		{
			SelectRow(_selectedRow);
		}
		Rectangle clientRectangle = base.ClientRectangle;
		if (internalLabelWidth > InternalLabelWidth)
		{
			int num2 = InternalLabelWidth - _requiredLabelPaintMargin;
			Invalidate(new Rectangle(num2, 0, base.Size.Width - num2, base.Size.Height));
		}
		else
		{
			clientRectangle.X = internalLabelWidth - _requiredLabelPaintMargin;
			clientRectangle.Width -= clientRectangle.X;
			Invalidate(clientRectangle);
		}
	}

	private void OnButtonClick(object sender, EventArgs e)
	{
		if (GetFlag(256) || (sender == DialogButton && !Commit()))
		{
			return;
		}
		SetCommitError(ErrorState.None);
		try
		{
			Commit();
			SetFlag(256, value: true);
			PopupDialog(_selectedRow);
		}
		finally
		{
			SetFlag(256, value: false);
		}
	}

	private void OnButtonKeyDown(object sender, KeyEventArgs ke)
	{
		OnKeyDown(sender, ke);
	}

	private void OnChildLostFocus(object sender, EventArgs e)
	{
		InvokeLostFocus(this, e);
	}

	private void OnDropDownButtonGotFocus(object sender, EventArgs e)
	{
		if (sender is DropDownButton dropDownButton)
		{
			dropDownButton.AccessibilityObject.SetFocus();
		}
	}

	protected override void OnGotFocus(EventArgs e)
	{
		base.OnGotFocus(e);
		if (e != null && !GetInPropertySet() && !Commit())
		{
			Edit.Focus();
			return;
		}
		if (_selectedGridEntry != null && GetRowFromGridEntry(_selectedGridEntry) != -1)
		{
			_selectedGridEntry.HasFocus = true;
			SelectGridEntry(_selectedGridEntry, pageIn: false);
		}
		else
		{
			SelectRow(0);
		}
		if (_selectedGridEntry != null && _selectedGridEntry.GetValueOwner() != null)
		{
			UpdateHelpAttributes(null, _selectedGridEntry);
		}
		if (TotalProperties > 0)
		{
			return;
		}
		int num = 2 * _offset2Units;
		if (base.Size.Width > num && base.Size.Height > num)
		{
			using (Graphics graphics = CreateGraphicsInternal())
			{
				ControlPaint.DrawFocusRectangle(graphics, new Rectangle(_offset2Units, _offset2Units, base.Size.Width - num, base.Size.Height - num));
			}
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
		SystemEvents.UserPreferenceChanged += OnSysColorChange;
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		SystemEvents.UserPreferenceChanged -= OnSysColorChange;
		if (_toolTip != null && !base.RecreatingHandle)
		{
			_toolTip.Dispose();
			_toolTip = null;
		}
		base.OnHandleDestroyed(e);
	}

	private void OnListChange(object sender, EventArgs e)
	{
		if (!DropDownListBox.InSetSelectedIndex())
		{
			GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
			Edit.Text = gridEntryFromRow.GetPropertyTextValue(DropDownListBox.SelectedItem);
			Edit.Focus();
			SelectEdit();
		}
		SetFlag(64, value: true);
	}

	private void OnListMouseUp(object sender, MouseEventArgs me)
	{
		OnListClick(sender, me);
	}

	private void OnListClick(object sender, EventArgs e)
	{
		GetGridEntryFromRow(_selectedRow);
		if (DropDownListBox.Items.Count == 0)
		{
			CommonEditorHide();
			SetCommitError(ErrorState.None);
			SelectRow(_selectedRow);
			return;
		}
		object selectedItem = DropDownListBox.SelectedItem;
		SetFlag(64, value: false);
		if (selectedItem != null && !CommitText((string)selectedItem))
		{
			SetCommitError(ErrorState.None);
			SelectRow(_selectedRow);
		}
	}

	private void OnListDrawItem(object sender, DrawItemEventArgs e)
	{
		if (e.Index < 0 || _selectedGridEntry == null)
		{
			return;
		}
		string text = (string)DropDownListBox.Items[e.Index];
		e.DrawBackground();
		e.DrawFocusRectangle();
		Rectangle bounds = e.Bounds;
		bounds.Y++;
		bounds.X--;
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		try
		{
			gridEntryFromRow.PaintValue(gridEntryFromRow.ConvertTextToValue(text), e.GraphicsInternal, bounds, bounds, e.State.HasFlag(DrawItemState.Selected) ? GridEntry.PaintValueFlags.DrawSelected : GridEntry.PaintValueFlags.None);
		}
		catch (FormatException ex)
		{
			ShowFormatExceptionMessage(gridEntryFromRow.PropertyLabel, ex);
			if (DropDownListBox.IsHandleCreated)
			{
				DropDownListBox.Visible = false;
			}
		}
	}

	private void OnListKeyDown(object sender, KeyEventArgs e)
	{
		if (e.KeyCode == Keys.Return)
		{
			OnListClick(null, null);
			if (_selectedGridEntry != null)
			{
				_selectedGridEntry.OnValueReturnKey();
			}
		}
		OnKeyDown(sender, e);
	}

	protected override void OnLostFocus(EventArgs e)
	{
		if (e != null)
		{
			base.OnLostFocus(e);
		}
		if (FocusInside)
		{
			base.OnLostFocus(e);
			return;
		}
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		if (gridEntryFromRow != null)
		{
			gridEntryFromRow.HasFocus = false;
			CommonEditorHide();
			InvalidateRow(_selectedRow);
		}
		base.OnLostFocus(e);
		if (TotalProperties > 0)
		{
			return;
		}
		Rectangle rectangle = new Rectangle(1, 1, base.Size.Width - 2, base.Size.Height - 2);
		Color backColor = BackColor;
		if (backColor.HasTransparency())
		{
			using (Graphics graphics = CreateGraphicsInternal())
			{
				RefCountedCache<SolidBrush, Color, Color>.Scope scope = backColor.GetCachedSolidBrushScope();
				try
				{
					graphics.FillRectangle((SolidBrush)scope, rectangle);
					return;
				}
				finally
				{
					scope.Dispose();
				}
			}
		}
		using global::Interop.User32.GetDcScope hdc = new global::Interop.User32.GetDcScope(base.Handle);
		global::Interop.Gdi32.CreateBrushScope scope2 = new global::Interop.Gdi32.CreateBrushScope(backColor);
		try
		{
			hdc.FillRectangle(scope2, rectangle);
		}
		finally
		{
			scope2.Dispose();
		}
	}

	private void OnEditChange(object sender, EventArgs e)
	{
		SetCommitError(ErrorState.None, Edit.Focused);
		ToolTip.ToolTip = string.Empty;
		ToolTip.Visible = false;
		if (!Edit.InSetText())
		{
			GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
			if (gridEntryFromRow != null && gridEntryFromRow.IsImmediatelyEditable)
			{
				Commit();
			}
		}
	}

	private void OnEditGotFocus(object sender, EventArgs e)
	{
		if (!Edit.Visible)
		{
			Focus();
			return;
		}
		switch (_errorState)
		{
		case ErrorState.MessageBoxUp:
			return;
		case ErrorState.Thrown:
			if (Edit.Visible)
			{
				Edit.HookMouseDown = true;
			}
			break;
		default:
			if (NeedsCommit)
			{
				SetCommitError(ErrorState.None, capture: true);
			}
			break;
		}
		if (_selectedGridEntry != null && GetRowFromGridEntry(_selectedGridEntry) != -1)
		{
			_selectedGridEntry.HasFocus = true;
			InvalidateRow(_selectedRow);
			(Edit.AccessibilityObject as ControlAccessibleObject).NotifyClients(AccessibleEvents.Focus);
			Edit.AccessibilityObject.SetFocus();
		}
		else
		{
			SelectRow(0);
		}
	}

	private bool ProcessEnumUpAndDown(GridEntry entry, Keys keyCode, bool closeDropDown = true)
	{
		object propertyValue = entry.PropertyValue;
		object[] propertyValueList = entry.GetPropertyValueList();
		if (propertyValueList == null)
		{
			return false;
		}
		for (int i = 0; i < propertyValueList.Length; i++)
		{
			object obj = propertyValueList[i];
			if (propertyValue != null && obj != null && propertyValue.GetType() != obj.GetType() && entry.TypeConverter.CanConvertTo(entry, propertyValue.GetType()))
			{
				obj = entry.TypeConverter.ConvertTo(entry, CultureInfo.CurrentCulture, obj, propertyValue.GetType());
			}
			bool flag = propertyValue == obj || (propertyValue?.Equals(obj) ?? false);
			if (!flag && propertyValue is string strA && obj != null)
			{
				flag = string.Compare(strA, obj.ToString(), ignoreCase: true, CultureInfo.CurrentCulture) == 0;
			}
			if (!flag)
			{
				continue;
			}
			object value;
			if (keyCode == Keys.Up)
			{
				if (i == 0)
				{
					return true;
				}
				value = propertyValueList[i - 1];
			}
			else
			{
				if (i == propertyValueList.Length - 1)
				{
					return true;
				}
				value = propertyValueList[i + 1];
			}
			CommitValue(entry, value, closeDropDown);
			SelectEdit();
			return true;
		}
		return false;
	}

	private void OnEditKeyDown(object sender, KeyEventArgs e)
	{
		if (!e.Alt && (e.KeyCode == Keys.Up || e.KeyCode == Keys.Down))
		{
			GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
			if (!gridEntryFromRow.Enumerable || !gridEntryFromRow.IsValueEditable)
			{
				return;
			}
			e.Handled = true;
			if (ProcessEnumUpAndDown(gridEntryFromRow, e.KeyCode))
			{
				return;
			}
		}
		else if ((e.KeyCode == Keys.Left || e.KeyCode == Keys.Right) && ((uint)e.Modifiers & 0xFFFEFFFFu) != 0)
		{
			return;
		}
		OnKeyDown(sender, e);
	}

	private void OnEditKeyPress(object sender, KeyPressEventArgs e)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		if (gridEntryFromRow != null && !gridEntryFromRow.IsTextEditable)
		{
			e.Handled = FilterReadOnlyEditKeyPress(e.KeyChar);
		}
	}

	private void OnEditLostFocus(object sender, EventArgs e)
	{
		if (Edit.Focused || _errorState == ErrorState.MessageBoxUp || _errorState == ErrorState.Thrown || GetInPropertySet())
		{
			return;
		}
		if (_dropDownHolder != null && _dropDownHolder.Visible)
		{
			bool flag = false;
			IntPtr intPtr = global::Interop.User32.GetForegroundWindow();
			while (intPtr != IntPtr.Zero)
			{
				if (intPtr == _dropDownHolder.Handle)
				{
					flag = true;
				}
				intPtr = global::Interop.User32.GetParent(intPtr);
			}
			if (flag)
			{
				return;
			}
		}
		if (!FocusInside)
		{
			if (!Commit())
			{
				Edit.Focus();
			}
			else
			{
				InvokeLostFocus(this, EventArgs.Empty);
			}
		}
	}

	private void OnEditMouseDown(object sender, MouseEventArgs e)
	{
		if (!FocusInside)
		{
			SelectGridEntry(_selectedGridEntry, pageIn: false);
		}
		if (e.Clicks % 2 == 0)
		{
			DoubleClickRow(_selectedRow, toggleExpand: false, 2);
			Edit.SelectAll();
		}
		if (_rowSelectTime != 0L && (int)((DateTime.Now.Ticks - _rowSelectTime) / 10000) < SystemInformation.DoubleClickTime)
		{
			Point point = Edit.PointToScreen(new Point(e.X, e.Y));
			if (Math.Abs(point.X - _rowSelectPos.X) < SystemInformation.DoubleClickSize.Width && Math.Abs(point.Y - _rowSelectPos.Y) < SystemInformation.DoubleClickSize.Height)
			{
				DoubleClickRow(_selectedRow, toggleExpand: false, 2);
				global::Interop.User32.SendMessageW(Edit, global::Interop.User32.WM.LBUTTONUP, IntPtr.Zero, global::Interop.PARAM.FromLowHigh(e.X, e.Y));
				Edit.SelectAll();
			}
			_rowSelectPos = Point.Empty;
			_rowSelectTime = 0L;
		}
	}

	private bool OnF4(Control sender)
	{
		if (Control.ModifierKeys != 0)
		{
			return false;
		}
		if (sender == this || sender == OwnerGrid)
		{
			F4Selection(popupModalDialog: true);
		}
		else
		{
			UnfocusSelection();
		}
		return true;
	}

	private bool OnEscape(Control sender)
	{
		if ((Control.ModifierKeys & (Keys.Control | Keys.Alt)) != 0)
		{
			return false;
		}
		SetFlag(64, value: false);
		if (sender != Edit || !Edit.Focused)
		{
			if (sender != this)
			{
				CloseDropDown();
				Focus();
			}
			return false;
		}
		if (_errorState == ErrorState.None)
		{
			Edit.Text = _originalTextValue;
			Focus();
			return true;
		}
		if (NeedsCommit)
		{
			bool flag = false;
			Edit.Text = _originalTextValue;
			bool flag2 = true;
			if (_selectedGridEntry != null)
			{
				string propertyTextValue = _selectedGridEntry.GetPropertyTextValue();
				flag2 = _originalTextValue != propertyTextValue && (!string.IsNullOrEmpty(_originalTextValue) || !string.IsNullOrEmpty(propertyTextValue));
			}
			if (flag2)
			{
				try
				{
					flag = CommitText(_originalTextValue);
				}
				catch
				{
				}
			}
			else
			{
				flag = true;
			}
			if (!flag)
			{
				Edit.Focus();
				SelectEdit();
				return true;
			}
		}
		SetCommitError(ErrorState.None);
		Focus();
		return true;
	}

	protected override void OnKeyDown(KeyEventArgs e)
	{
		OnKeyDown(this, e);
	}

	private void OnKeyDown(object sender, KeyEventArgs e)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		if (gridEntryFromRow == null)
		{
			return;
		}
		e.Handled = true;
		bool control = e.Control;
		bool shift = e.Shift;
		bool flag = control && shift;
		bool alt = e.Alt;
		Keys keyCode = e.KeyCode;
		bool flag2 = false;
		if (keyCode == Keys.Tab && ProcessDialogKey(e.KeyData))
		{
			e.Handled = true;
			return;
		}
		if (keyCode == Keys.Down && alt && DropDownButton.Visible)
		{
			F4Selection(popupModalDialog: false);
			return;
		}
		if (keyCode == Keys.Up && alt && DropDownButton.Visible && _dropDownHolder != null && _dropDownHolder.Visible)
		{
			UnfocusSelection();
			return;
		}
		if (ToolTip.Visible)
		{
			ToolTip.ToolTip = string.Empty;
		}
		if (flag || sender == this || sender == OwnerGrid)
		{
			int num;
			int row;
			switch (keyCode)
			{
			case Keys.Down:
				num = _selectedRow + 1;
				goto IL_01c6;
			case Keys.Up:
				num = _selectedRow - 1;
				goto IL_01c6;
			case Keys.Left:
				if (control)
				{
					MoveSplitterTo(InternalLabelWidth - 3);
				}
				else if (gridEntryFromRow.InternalExpanded)
				{
					SetExpand(gridEntryFromRow, value: false);
				}
				else
				{
					SelectGridEntry(GetGridEntryFromRow(_selectedRow - 1), pageIn: true);
				}
				return;
			case Keys.Right:
				if (control)
				{
					MoveSplitterTo(InternalLabelWidth + 3);
				}
				else if (gridEntryFromRow.Expandable)
				{
					if (gridEntryFromRow.InternalExpanded)
					{
						SelectGridEntry(gridEntryFromRow.Children.GetEntry(0), pageIn: true);
					}
					else
					{
						SetExpand(gridEntryFromRow, value: true);
					}
				}
				else
				{
					SelectGridEntry(GetGridEntryFromRow(_selectedRow + 1), pageIn: true);
				}
				return;
			case Keys.Return:
				if (gridEntryFromRow.Expandable)
				{
					SetExpand(gridEntryFromRow, !gridEntryFromRow.InternalExpanded);
				}
				else
				{
					gridEntryFromRow.OnValueReturnKey();
				}
				return;
			case Keys.End:
			case Keys.Home:
			{
				GridEntryCollection allGridEntries = GetAllGridEntries();
				SelectGridEntry(allGridEntries.GetEntry((keyCode != Keys.Home) ? (allGridEntries.Count - 1) : 0), pageIn: true);
				return;
			}
			case Keys.Add:
			case Keys.Subtract:
			case Keys.Oemplus:
			case Keys.OemMinus:
				if (gridEntryFromRow.Expandable)
				{
					SetFlag(8, value: true);
					bool value = keyCode == Keys.Add || keyCode == Keys.Oemplus;
					SetExpand(gridEntryFromRow, value);
					Invalidate();
					e.Handled = true;
					return;
				}
				break;
			case Keys.D8:
				if (!shift)
				{
					break;
				}
				goto case Keys.Multiply;
			case Keys.Multiply:
				SetFlag(8, value: true);
				RecursivelyExpand(gridEntryFromRow, initialize: true, expand: true, 10);
				e.Handled = false;
				return;
			case Keys.Prior:
			case Keys.Next:
			{
				bool flag3 = keyCode == Keys.Next;
				int num2 = (flag3 ? (_visibleRows - 1) : (1 - _visibleRows));
				int row2 = _selectedRow;
				if (control && !shift)
				{
					return;
				}
				if (_selectedRow != -1)
				{
					int scrollOffset = GetScrollOffset();
					SetScrollOffset(scrollOffset + num2);
					SetConstants();
					if (GetScrollOffset() != scrollOffset + num2)
					{
						row2 = (flag3 ? (_visibleRows - 1) : 0);
					}
				}
				SelectRow(row2);
				Refresh();
				return;
			}
			case Keys.Insert:
				if (!shift || control || alt)
				{
					goto case Keys.C;
				}
				flag2 = true;
				goto case Keys.V;
			case Keys.C:
				if (control && !alt && !shift)
				{
					DoCopyCommand();
					return;
				}
				break;
			case Keys.Delete:
				if (!shift || control || alt)
				{
					break;
				}
				flag2 = true;
				goto case Keys.X;
			case Keys.X:
				if (flag2 || (control && !alt && !shift))
				{
					Clipboard.SetDataObject(gridEntryFromRow.GetPropertyTextValue());
					CommitText("");
					return;
				}
				break;
			case Keys.V:
				if (flag2 || (control && !alt && !shift))
				{
					DoPasteCommand();
				}
				break;
			case Keys.A:
				{
					if (control && !alt && !shift && Edit.Visible)
					{
						Edit.Focus();
						Edit.SelectAll();
					}
					break;
				}
				IL_01c6:
				row = num;
				SelectGridEntry(GetGridEntryFromRow(row), pageIn: true);
				SetFlag(512, value: false);
				return;
			}
		}
		if (gridEntryFromRow != null && e.KeyData == (Keys.C | Keys.Shift | Keys.Control | Keys.Alt))
		{
			Clipboard.SetDataObject(gridEntryFromRow.GetTestingInfo());
			return;
		}
		if (_selectedGridEntry != null && _selectedGridEntry.Enumerable && _dropDownHolder != null && _dropDownHolder.Visible && (keyCode == Keys.Up || keyCode == Keys.Down))
		{
			ProcessEnumUpAndDown(_selectedGridEntry, keyCode, closeDropDown: false);
		}
		e.Handled = false;
	}

	protected override void OnKeyPress(KeyPressEventArgs e)
	{
		if (WillFilterKeyPress(e.KeyChar))
		{
			FilterKeyPress(e.KeyChar);
		}
		SetFlag(8, value: false);
	}

	protected override void OnMouseDown(MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Left && SplitterInside(e.X) && TotalProperties != 0)
		{
			if (Commit())
			{
				if (e.Clicks == 2)
				{
					MoveSplitterTo(base.Width / 2);
					return;
				}
				UnfocusSelection();
				SetFlag(4, value: true);
				_tipInfo = -1;
				base.Capture = true;
			}
			return;
		}
		Point point = FindPosition(e.X, e.Y);
		if (point == InvalidPosition)
		{
			return;
		}
		GridEntry gridEntryFromRow = GetGridEntryFromRow(point.Y);
		if (gridEntryFromRow != null)
		{
			Rectangle rectangle = GetRectangle(point.Y, 1);
			_lastMouseDown = new Point(e.X, e.Y);
			if (e.Button == MouseButtons.Left)
			{
				gridEntryFromRow.OnMouseClick(e.X - rectangle.X, e.Y - rectangle.Y, e.Clicks, e.Button);
			}
			else
			{
				SelectGridEntry(gridEntryFromRow, pageIn: false);
			}
			_lastMouseDown = InvalidPosition;
			gridEntryFromRow.HasFocus = true;
			SetFlag(512, value: false);
		}
	}

	protected override void OnMouseLeave(EventArgs e)
	{
		if (!GetFlag(4))
		{
			Cursor = Cursors.Default;
		}
		base.OnMouseLeave(e);
	}

	protected override void OnMouseMove(MouseEventArgs e)
	{
		bool flag = false;
		int num;
		Point point;
		if (e == null)
		{
			num = -1;
			point = InvalidPosition;
		}
		else
		{
			point = FindPosition(e.X, e.Y);
			if (point == InvalidPosition || (point.X != 1 && point.X != 2))
			{
				num = -1;
				ToolTip.ToolTip = string.Empty;
			}
			else
			{
				num = point.Y;
				flag = point.X == 1;
			}
		}
		if (point == InvalidPosition || e == null)
		{
			return;
		}
		if (GetFlag(4))
		{
			MoveSplitterTo(e.X);
		}
		if ((num != TipRow || point.X != TipColumn) && !GetFlag(4))
		{
			GridEntry gridEntryFromRow = GetGridEntryFromRow(num);
			string toolTip = string.Empty;
			_tipInfo = -1;
			if (gridEntryFromRow != null)
			{
				Rectangle rectangle = GetRectangle(point.Y, point.X);
				if (flag && gridEntryFromRow.GetLabelToolTipLocation(e.X - rectangle.X, e.Y - rectangle.Y) != InvalidPoint)
				{
					toolTip = gridEntryFromRow.LabelToolTipText;
					TipRow = num;
					TipColumn = point.X;
				}
				else if (!flag && gridEntryFromRow.ValueToolTipLocation != InvalidPoint && !Edit.Focused)
				{
					if (!NeedsCommit)
					{
						toolTip = gridEntryFromRow.GetPropertyTextValue();
					}
					TipRow = num;
					TipColumn = point.X;
				}
			}
			if (global::Interop.User32.IsChild(global::Interop.User32.GetForegroundWindow(), new HandleRef(this, base.Handle)).IsTrue())
			{
				if (_dropDownHolder == null || _dropDownHolder.Component == null || num == _selectedRow)
				{
					ToolTip.ToolTip = toolTip;
				}
			}
			else
			{
				ToolTip.ToolTip = string.Empty;
			}
		}
		if (TotalProperties != 0 && (SplitterInside(e.X) || GetFlag(4)))
		{
			Cursor = Cursors.VSplit;
		}
		else
		{
			Cursor = Cursors.Default;
		}
		base.OnMouseMove(e);
	}

	protected override void OnMouseUp(MouseEventArgs e)
	{
		CancelSplitterMove();
	}

	protected override void OnMouseWheel(MouseEventArgs e)
	{
		OwnerGrid.OnGridViewMouseWheel(e);
		if (e is HandledMouseEventArgs handledMouseEventArgs)
		{
			if (handledMouseEventArgs.Handled)
			{
				return;
			}
			handledMouseEventArgs.Handled = true;
		}
		if ((Control.ModifierKeys & (Keys.Shift | Keys.Alt)) != 0 || Control.MouseButtons != 0)
		{
			return;
		}
		int mouseWheelScrollLines = SystemInformation.MouseWheelScrollLines;
		if (mouseWheelScrollLines == 0)
		{
			return;
		}
		if (_selectedGridEntry != null && _selectedGridEntry.Enumerable && Edit.Focused && _selectedGridEntry.IsValueEditable)
		{
			int currentValueIndex = GetCurrentValueIndex(_selectedGridEntry);
			if (currentValueIndex != -1)
			{
				int num = ((e.Delta <= 0) ? 1 : (-1));
				object[] propertyValueList = _selectedGridEntry.GetPropertyValueList();
				currentValueIndex = ((num <= 0 || currentValueIndex < propertyValueList.Length - 1) ? ((num >= 0 || currentValueIndex != 0) ? (currentValueIndex + num) : (propertyValueList.Length - 1)) : 0);
				CommitValue(propertyValueList[currentValueIndex]);
				SelectGridEntry(_selectedGridEntry, pageIn: true);
				Edit.Focus();
				return;
			}
		}
		int scrollOffset = GetScrollOffset();
		_cumulativeVerticalWheelDelta += e.Delta;
		float num2 = (float)_cumulativeVerticalWheelDelta / 120f;
		int num3 = (int)num2;
		if (mouseWheelScrollLines == -1)
		{
			if (num3 != 0)
			{
				int num4 = scrollOffset;
				int num5 = num3 * _scrollBar.LargeChange;
				int val = Math.Max(0, scrollOffset - num5);
				val = Math.Min(val, TotalProperties - _visibleRows + 1);
				scrollOffset -= num3 * _scrollBar.LargeChange;
				if (Math.Abs(scrollOffset - num4) >= Math.Abs(num3 * _scrollBar.LargeChange))
				{
					_cumulativeVerticalWheelDelta -= num3 * 120;
				}
				else
				{
					_cumulativeVerticalWheelDelta = 0;
				}
				if (!ScrollRows(val))
				{
					_cumulativeVerticalWheelDelta = 0;
				}
			}
			return;
		}
		int num6 = (int)((float)mouseWheelScrollLines * num2);
		if (num6 != 0)
		{
			if (ToolTip.Visible)
			{
				ToolTip.ToolTip = string.Empty;
			}
			int val2 = Math.Max(0, scrollOffset - num6);
			val2 = Math.Min(val2, TotalProperties - _visibleRows + 1);
			if (num6 > 0)
			{
				if (_scrollBar.Value <= _scrollBar.Minimum)
				{
					_cumulativeVerticalWheelDelta = 0;
				}
				else
				{
					_cumulativeVerticalWheelDelta -= (int)((float)num6 * (120f / (float)mouseWheelScrollLines));
				}
			}
			else if (_scrollBar.Value > _scrollBar.Maximum - _visibleRows + 1)
			{
				_cumulativeVerticalWheelDelta = 0;
			}
			else
			{
				_cumulativeVerticalWheelDelta -= (int)((float)num6 * (120f / (float)mouseWheelScrollLines));
			}
			if (!ScrollRows(val2))
			{
				_cumulativeVerticalWheelDelta = 0;
			}
		}
		else
		{
			_cumulativeVerticalWheelDelta = 0;
		}
	}

	protected override void OnMove(EventArgs e)
	{
		CloseDropDown();
	}

	protected override void OnPaintBackground(PaintEventArgs pevent)
	{
	}

	protected override void OnPaint(PaintEventArgs e)
	{
		Graphics graphics = e.Graphics;
		int num = 0;
		int num2 = 0;
		int num3 = _visibleRows - 1;
		Rectangle clipRectangle = e.ClipRectangle;
		clipRectangle.Inflate(0, 2);
		try
		{
			Size size = base.Size;
			Point point = FindPosition(clipRectangle.X, clipRectangle.Y);
			Point point2 = FindPosition(clipRectangle.X, clipRectangle.Y + clipRectangle.Height);
			if (point != InvalidPosition)
			{
				num2 = Math.Max(0, point.Y);
			}
			if (point2 != InvalidPosition)
			{
				num3 = point2.Y;
			}
			int num4 = Math.Min(TotalProperties - GetScrollOffset(), 1 + _visibleRows);
			SetFlag(1, value: false);
			Size ourSize = GetOurSize();
			Point location = _location;
			if (GetGridEntryFromRow(num4 - 1) == null)
			{
				num4--;
			}
			if (TotalProperties > 0)
			{
				num4 = Math.Min(num4, num3 + 1);
				RefCountedCache<Pen, Color, Color>.Scope scope = OwnerGrid.LineColor.GetCachedPenScope(GetSplitterWidth());
				try
				{
					graphics.DrawLine(scope, _labelWidth, location.Y, _labelWidth, num4 * (RowHeight + 1) + location.Y);
					RefCountedCache<Pen, Color, Color>.Scope scope2 = graphics.FindNearestColor(OwnerGrid.LineColor).GetCachedPenScope();
					try
					{
						int num5 = 0;
						int x = location.X + ourSize.Width;
						int x2 = location.X;
						GetTotalWidth();
						for (int i = num2; i < num4; i++)
						{
							try
							{
								num5 = i * (RowHeight + 1) + location.Y;
								graphics.DrawLine(scope2, x2, num5, x, num5);
								DrawValueEntry(graphics, i, clipRectangle);
								Rectangle rectangle = GetRectangle(i, 1);
								num = rectangle.Y + rectangle.Height;
								DrawLabel(graphics, i, rectangle, i == _selectedRow, longLabelrequest: false, clipRectangle);
								if (i == _selectedRow)
								{
									Edit.Invalidate();
								}
							}
							catch
							{
							}
						}
						num5 = num4 * (RowHeight + 1) + location.Y;
						graphics.DrawLine(scope2, x2, num5, x, num5);
					}
					finally
					{
						scope2.Dispose();
					}
				}
				finally
				{
					scope.Dispose();
				}
			}
			if (num < base.Size.Height)
			{
				num++;
				Rectangle rect = new Rectangle(1, num, base.Size.Width - 2, base.Size.Height - num - 1);
				RefCountedCache<SolidBrush, Color, Color>.Scope scope3 = BackColor.GetCachedSolidBrushScope();
				try
				{
					graphics.FillRectangle((SolidBrush)scope3, rect);
				}
				finally
				{
					scope3.Dispose();
				}
			}
			RefCountedCache<Pen, Color, Color>.Scope scope4 = OwnerGrid.ViewBorderColor.GetCachedPenScope();
			try
			{
				graphics.DrawRectangle(scope4, 0, 0, size.Width - 1, size.Height - 1);
				_boldFont = null;
			}
			finally
			{
				scope4.Dispose();
			}
		}
		catch
		{
		}
	}

	private void OnGridEntryLabelDoubleClick(object s, EventArgs e)
	{
		GridEntry gridEntry = (GridEntry)s;
		if (gridEntry == _lastClickedEntry)
		{
			int rowFromGridEntry = GetRowFromGridEntry(gridEntry);
			DoubleClickRow(rowFromGridEntry, gridEntry.Expandable, 1);
		}
	}

	private void OnGridEntryValueDoubleClick(object s, EventArgs e)
	{
		GridEntry gridEntry = (GridEntry)s;
		if (gridEntry == _lastClickedEntry)
		{
			int rowFromGridEntry = GetRowFromGridEntry(gridEntry);
			DoubleClickRow(rowFromGridEntry, gridEntry.Expandable, 2);
		}
	}

	private void OnGridEntryLabelClick(object sender, EventArgs e)
	{
		_lastClickedEntry = (GridEntry)sender;
		SelectGridEntry(_lastClickedEntry, pageIn: true);
	}

	private void OnGridEntryOutlineClick(object sender, EventArgs e)
	{
		GridEntry gridEntry = (GridEntry)sender;
		Cursor cursor = Cursor;
		if (!ShouldSerializeCursor())
		{
			cursor = null;
		}
		Cursor = Cursors.WaitCursor;
		try
		{
			SetExpand(gridEntry, !gridEntry.InternalExpanded);
			SelectGridEntry(gridEntry, pageIn: false);
		}
		finally
		{
			Cursor = cursor;
		}
	}

	private void OnGridEntryValueClick(object sender, EventArgs e)
	{
		_lastClickedEntry = (GridEntry)sender;
		bool num = sender != _selectedGridEntry;
		SelectGridEntry(_lastClickedEntry, pageIn: true);
		Edit.Focus();
		if (_lastMouseDown != InvalidPosition)
		{
			_rowSelectTime = 0L;
			Point p = PointToScreen(_lastMouseDown);
			p = Edit.PointToClient(p);
			global::Interop.User32.SendMessageW(Edit, global::Interop.User32.WM.LBUTTONDOWN, IntPtr.Zero, global::Interop.PARAM.FromLowHigh(p.X, p.Y));
			global::Interop.User32.SendMessageW(Edit, global::Interop.User32.WM.LBUTTONUP, IntPtr.Zero, global::Interop.PARAM.FromLowHigh(p.X, p.Y));
		}
		if (num)
		{
			_rowSelectTime = DateTime.Now.Ticks;
			_rowSelectPos = PointToScreen(_lastMouseDown);
		}
		else
		{
			_rowSelectTime = 0L;
			_rowSelectPos = Point.Empty;
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		_cachedRowHeight = -1;
		if (!base.Disposing && ParentInternal != null && !ParentInternal.Disposing)
		{
			_boldFont = null;
			ToolTip.Font = Font;
			SetFlag(128, value: true);
			UpdateUIBasedOnFont(layoutRequired: true);
			base.OnFontChanged(e);
			if (_selectedGridEntry != null)
			{
				SelectGridEntry(_selectedGridEntry, pageIn: true);
			}
		}
	}

	protected override void OnVisibleChanged(EventArgs e)
	{
		if (base.Disposing || ParentInternal == null || ParentInternal.Disposing)
		{
			return;
		}
		if (base.Visible && ParentInternal != null)
		{
			SetConstants();
			if (_selectedGridEntry != null)
			{
				SelectGridEntry(_selectedGridEntry, pageIn: true);
			}
			if (_toolTip != null)
			{
				ToolTip.Font = Font;
			}
		}
		base.OnVisibleChanged(e);
	}

	protected virtual void OnRecreateChildren(object s, GridEntryRecreateChildrenEventArgs e)
	{
		GridEntry gridEntry = (GridEntry)s;
		if (gridEntry.Expanded)
		{
			GridEntry[] array = new GridEntry[_allGridEntries.Count];
			_allGridEntries.CopyTo(array, 0);
			int num = -1;
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i] == gridEntry)
				{
					num = i;
					break;
				}
			}
			ClearGridEntryEvents(_allGridEntries, num + 1, e.OldChildCount);
			if (e.OldChildCount != e.NewChildCount)
			{
				GridEntry[] array2 = new GridEntry[array.Length + (e.NewChildCount - e.OldChildCount)];
				Array.Copy(array, 0, array2, 0, num + 1);
				Array.Copy(array, num + e.OldChildCount + 1, array2, num + e.NewChildCount + 1, array.Length - (num + e.OldChildCount + 1));
				array = array2;
			}
			GridEntryCollection children = gridEntry.Children;
			int count = children.Count;
			for (int j = 0; j < count; j++)
			{
				array[num + j + 1] = children.GetEntry(j);
			}
			_allGridEntries.Clear();
			_allGridEntries.AddRange(array);
			AddGridEntryEvents(_allGridEntries, num + 1, count);
		}
		if (e.OldChildCount != e.NewChildCount)
		{
			TotalProperties = CountPropsFromOutline(TopLevelGridEntries);
			SetConstants();
		}
		Invalidate();
	}

	protected override void OnResize(EventArgs e)
	{
		Rectangle clientRectangle = base.ClientRectangle;
		int num = ((!(_lastClientRect == Rectangle.Empty)) ? (clientRectangle.Height - _lastClientRect.Height) : 0);
		if (!_lastClientRect.IsEmpty && clientRectangle.Width > _lastClientRect.Width)
		{
			Rectangle rc = new Rectangle(_lastClientRect.Width - 1, 0, clientRectangle.Width - _lastClientRect.Width + 1, _lastClientRect.Height);
			Invalidate(rc);
		}
		if (!_lastClientRect.IsEmpty && num > 0)
		{
			Rectangle rc2 = new Rectangle(0, _lastClientRect.Height - 1, _lastClientRect.Width, clientRectangle.Height - _lastClientRect.Height + 1);
			Invalidate(rc2);
		}
		int scrollOffset = GetScrollOffset();
		SetScrollOffset(0);
		SetConstants();
		SetScrollOffset(scrollOffset);
		if (DpiHelper.IsScalingRequirementMet)
		{
			SetFlag(128, value: true);
			UpdateUIBasedOnFont(layoutRequired: true);
			base.OnFontChanged(e);
		}
		CommonEditorHide();
		LayoutWindow(invalidate: false);
		bool pageIn = _selectedGridEntry != null && _selectedRow >= 0 && _selectedRow <= _visibleRows;
		SelectGridEntry(_selectedGridEntry, pageIn);
		_lastClientRect = clientRectangle;
	}

	private void OnScroll(object sender, ScrollEventArgs e)
	{
		if (!Commit() || !IsScrollValueValid(e.NewValue))
		{
			e.NewValue = ScrollBar.Value;
			return;
		}
		int num = -1;
		GridEntry selectedGridEntry = _selectedGridEntry;
		if (_selectedGridEntry != null)
		{
			num = GetRowFromGridEntry(selectedGridEntry);
		}
		ScrollBar.Value = e.NewValue;
		if (selectedGridEntry != null)
		{
			_selectedRow = -1;
			SelectGridEntry(selectedGridEntry, ScrollBar.Value == TotalProperties);
			int rowFromGridEntry = GetRowFromGridEntry(selectedGridEntry);
			if (num != rowFromGridEntry)
			{
				Invalidate();
			}
		}
		else
		{
			Invalidate();
		}
	}

	private void OnSysColorChange(object sender, UserPreferenceChangedEventArgs e)
	{
		if (e.Category == UserPreferenceCategory.Color || e.Category == UserPreferenceCategory.Accessibility)
		{
			SetFlag(128, value: true);
		}
	}

	public virtual void PopupDialog(int row)
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(row);
		if (gridEntryFromRow == null)
		{
			return;
		}
		if (_dropDownHolder != null && _dropDownHolder.GetUsed())
		{
			CloseDropDown();
			return;
		}
		bool needsDropDownButton = gridEntryFromRow.NeedsDropDownButton;
		bool enumerable = gridEntryFromRow.Enumerable;
		bool needsCustomEditorButton = gridEntryFromRow.NeedsCustomEditorButton;
		if (enumerable && !needsDropDownButton)
		{
			DropDownListBox.Items.Clear();
			_ = gridEntryFromRow.PropertyValue;
			object[] propertyValueList = gridEntryFromRow.GetPropertyValueList();
			int num = 0;
			global::Interop.User32.GetDcScope dcScope = new global::Interop.User32.GetDcScope(DropDownListBox.Handle);
			try
			{
				global::Interop.Gdi32.TEXTMETRICW lptm = default(global::Interop.Gdi32.TEXTMETRICW);
				int num2 = -1;
				global::Interop.Gdi32.HFONT hFONT = (global::Interop.Gdi32.HFONT)Font.ToHfont();
				using (new global::Interop.Gdi32.ObjectScope(hFONT))
				{
					using (new global::Interop.Gdi32.SelectObjectScope(dcScope, hFONT))
					{
						num2 = GetCurrentValueIndex(gridEntryFromRow);
						if (propertyValueList != null && propertyValueList.Length != 0)
						{
							Size psizl = default(Size);
							for (int i = 0; i < propertyValueList.Length; i++)
							{
								string propertyTextValue = gridEntryFromRow.GetPropertyTextValue(propertyValueList[i]);
								DropDownListBox.Items.Add(propertyTextValue);
								global::Interop.Gdi32.GetTextExtentPoint32W(new HandleRef(DropDownListBox, dcScope), propertyTextValue, propertyTextValue.Length, ref psizl);
								num = Math.Max(psizl.Width, num);
							}
						}
						global::Interop.Gdi32.GetTextMetricsW(dcScope, ref lptm);
						num += 2 + lptm.tmMaxCharWidth + SystemInformation.VerticalScrollBarWidth;
					}
				}
				if (num2 != -1)
				{
					DropDownListBox.SelectedIndex = num2;
				}
				SetFlag(64, value: false);
				DropDownListBox.Height = Math.Max(lptm.tmHeight + 2, Math.Min(_maxListBoxHeight, DropDownListBox.PreferredHeight));
				DropDownListBox.Width = Math.Max(num, GetRectangle(row, 2).Width);
				try
				{
					bool value = DropDownListBox.Items.Count > DropDownListBox.Height / DropDownListBox.ItemHeight;
					SetFlag(1024, value);
					DropDownControl(DropDownListBox);
				}
				finally
				{
					SetFlag(1024, value: false);
				}
				Refresh();
				return;
			}
			finally
			{
				dcScope.Dispose();
			}
		}
		if (!(needsCustomEditorButton || needsDropDownButton))
		{
			return;
		}
		try
		{
			SetFlag(16, value: true);
			Edit.DisableMouseHook = true;
			try
			{
				SetFlag(1024, gridEntryFromRow.UITypeEditor.IsDropDownResizable);
				gridEntryFromRow.EditPropertyValue(this);
			}
			finally
			{
				SetFlag(1024, value: false);
			}
		}
		finally
		{
			SetFlag(16, value: false);
			Edit.DisableMouseHook = false;
		}
		Refresh();
		if (FocusInside)
		{
			SelectGridEntry(gridEntryFromRow, pageIn: false);
		}
	}

	internal static void PositionTooltip(Control parent, GridToolTip toolTip, Rectangle itemRect)
	{
		toolTip.Visible = false;
		global::Interop.RECT lParam = itemRect;
		global::Interop.User32.SendMessageW(toolTip, (global::Interop.User32.WM)1055u, (IntPtr)1, ref lParam);
		Point location = (toolTip.Location = parent.PointToScreen(new Point(lParam.left, lParam.top)));
		int num = toolTip.Location.X + toolTip.Size.Width - SystemInformation.VirtualScreen.Width;
		if (num > 0)
		{
			location.X -= num;
			toolTip.Location = location;
		}
		toolTip.Visible = true;
	}

	protected override bool ProcessDialogKey(Keys keyData)
	{
		if (HasEntries)
		{
			switch (keyData & Keys.KeyCode)
			{
			case Keys.F4:
				if (FocusInside)
				{
					return OnF4(this);
				}
				break;
			case Keys.Tab:
			{
				if ((keyData & Keys.Control) != 0 || (keyData & Keys.Alt) != 0)
				{
					break;
				}
				bool flag = (keyData & Keys.Shift) == 0;
				Control control = Control.FromHandle(global::Interop.User32.GetFocus());
				if (control == null || !IsMyChild(control))
				{
					if (flag)
					{
						TabSelection();
						control = Control.FromHandle(global::Interop.User32.GetFocus());
						if (IsMyChild(control))
						{
							return true;
						}
						return base.ProcessDialogKey(keyData);
					}
				}
				else if (Edit.Focused)
				{
					if (!flag)
					{
						SelectGridEntry(GetGridEntryFromRow(_selectedRow), pageIn: false);
						return true;
					}
					if (DropDownButton.Visible)
					{
						DropDownButton.Focus();
						return true;
					}
					if (DialogButton.Visible)
					{
						DialogButton.Focus();
						return true;
					}
				}
				else if ((DialogButton.Focused || DropDownButton.Focused) && !flag && Edit.Visible)
				{
					Edit.Focus();
					return true;
				}
				break;
			}
			case Keys.Left:
			case Keys.Up:
			case Keys.Right:
			case Keys.Down:
				return false;
			case Keys.Return:
				if (DialogButton.Focused || DropDownButton.Focused)
				{
					OnButtonClick(DialogButton.Focused ? DialogButton : DropDownButton, EventArgs.Empty);
					return true;
				}
				if (_selectedGridEntry != null && _selectedGridEntry.Expandable)
				{
					SetExpand(_selectedGridEntry, !_selectedGridEntry.InternalExpanded);
					return true;
				}
				break;
			}
		}
		return base.ProcessDialogKey(keyData);
	}

	protected virtual void RecalculateProperties()
	{
		int num = CountPropsFromOutline(TopLevelGridEntries);
		if (TotalProperties != num)
		{
			TotalProperties = num;
			ClearGridEntryEvents(_allGridEntries, 0, -1);
			_allGridEntries = null;
		}
	}

	internal void RecursivelyExpand(GridEntry gridEntry, bool initialize, bool expand, int maxExpands)
	{
		if (gridEntry == null || (expand && --maxExpands < 0))
		{
			return;
		}
		SetExpand(gridEntry, expand);
		GridEntryCollection children = gridEntry.Children;
		if (children != null)
		{
			for (int i = 0; i < children.Count; i++)
			{
				RecursivelyExpand(children.GetEntry(i), initialize: false, expand, maxExpands);
			}
		}
		if (initialize)
		{
			GridEntry selectedGridEntry = _selectedGridEntry;
			Refresh();
			SelectGridEntry(selectedGridEntry, pageIn: false);
			Invalidate();
		}
	}

	public override void Refresh()
	{
		Refresh(fullRefresh: false, -1, -1);
		if (TopLevelGridEntries != null && DpiHelper.IsScalingRequirementMet)
		{
			int outlineIconSize = GetOutlineIconSize();
			foreach (GridEntry topLevelGridEntry in TopLevelGridEntries)
			{
				if (topLevelGridEntry.OutlineRect.Height != outlineIconSize || topLevelGridEntry.OutlineRect.Width != outlineIconSize)
				{
					ResetOutline(topLevelGridEntry);
				}
			}
		}
		Invalidate();
	}

	public void Refresh(bool fullRefresh)
	{
		Refresh(fullRefresh, -1, -1);
	}

	private void Refresh(bool fullRefresh, int startRow, int endRow)
	{
		SetFlag(1, value: true);
		GridEntry gridEntry = null;
		if (base.IsDisposed)
		{
			return;
		}
		bool pageIn = true;
		if (startRow == -1)
		{
			startRow = 0;
		}
		if (fullRefresh || OwnerGrid.HavePropEntriesChanged())
		{
			if (HasEntries && !GetInPropertySet() && !Commit())
			{
				OnEscape(this);
			}
			int totalProperties = TotalProperties;
			object obj = ((TopLevelGridEntries == null || TopLevelGridEntries.Count == 0) ? null : ((GridEntry)TopLevelGridEntries[0]).GetValueOwner());
			if (fullRefresh)
			{
				OwnerGrid.RefreshProperties(clearCached: true);
			}
			if (totalProperties > 0 && !GetFlag(512))
			{
				_positionData = CaptureGridPositionData();
				CommonEditorHide(always: true);
			}
			UpdateHelpAttributes(_selectedGridEntry, null);
			_selectedGridEntry = null;
			SetFlag(2, value: true);
			TopLevelGridEntries = OwnerGrid.GetPropEntries();
			ClearGridEntryEvents(_allGridEntries, 0, -1);
			_allGridEntries = null;
			RecalculateProperties();
			int totalProperties2 = TotalProperties;
			if (totalProperties2 > 0)
			{
				if (totalProperties2 < totalProperties)
				{
					SetScrollbarLength();
					SetScrollOffset(0);
				}
				SetConstants();
				if (_positionData != null)
				{
					gridEntry = _positionData.Restore(this);
					object obj2 = ((TopLevelGridEntries == null || TopLevelGridEntries.Count == 0) ? null : ((GridEntry)TopLevelGridEntries[0]).GetValueOwner());
					pageIn = gridEntry == null || totalProperties != totalProperties2 || obj2 != obj;
				}
				if (gridEntry == null)
				{
					gridEntry = OwnerGrid.GetDefaultGridEntry();
					SetFlag(512, gridEntry == null && TotalProperties > 0);
				}
				InvalidateRows(startRow, endRow);
				if (gridEntry == null)
				{
					_selectedRow = 0;
					_selectedGridEntry = GetGridEntryFromRow(_selectedRow);
				}
			}
			else
			{
				if (totalProperties == 0)
				{
					return;
				}
				SetConstants();
			}
			_positionData = null;
			_lastClickedEntry = null;
		}
		if (!HasEntries)
		{
			CommonEditorHide(_selectedRow != -1);
			OwnerGrid.SetStatusBox(null, null);
			SetScrollOffset(0);
			_selectedRow = -1;
			Invalidate();
		}
		else
		{
			OwnerGrid.ClearCachedValues();
			InvalidateRows(startRow, endRow);
			if (gridEntry != null)
			{
				SelectGridEntry(gridEntry, pageIn);
			}
		}
	}

	public virtual void Reset()
	{
		GridEntry gridEntryFromRow = GetGridEntryFromRow(_selectedRow);
		if (gridEntryFromRow != null)
		{
			gridEntryFromRow.ResetPropertyValue();
			SelectRow(_selectedRow);
		}
	}

	protected virtual void ResetOrigin(Graphics g)
	{
		g.ResetTransform();
	}

	internal void RestoreHierarchyState(ArrayList expandedItems)
	{
		if (expandedItems == null)
		{
			return;
		}
		foreach (GridEntryCollection expandedItem in expandedItems)
		{
			FindEquivalentGridEntry(expandedItem);
		}
	}

	internal ArrayList SaveHierarchyState(GridEntryCollection entries)
	{
		return SaveHierarchyState(entries, null);
	}

	private ArrayList SaveHierarchyState(GridEntryCollection entries, ArrayList expandedItems)
	{
		if (entries == null)
		{
			return new ArrayList();
		}
		if (expandedItems == null)
		{
			expandedItems = new ArrayList();
		}
		for (int i = 0; i < entries.Count; i++)
		{
			if (((GridEntry)entries[i]).InternalExpanded)
			{
				GridEntry entry = entries.GetEntry(i);
				expandedItems.Add(GetGridEntryHierarchy(entry.Children.GetEntry(0)));
				SaveHierarchyState(entry.Children, expandedItems);
			}
		}
		return expandedItems;
	}

	private bool ScrollRows(int newOffset)
	{
		GridEntry selectedGridEntry = _selectedGridEntry;
		if (!IsScrollValueValid(newOffset) || !Commit())
		{
			return false;
		}
		bool visible = Edit.Visible;
		bool visible2 = DropDownButton.Visible;
		bool visible3 = DialogButton.Visible;
		Edit.Visible = false;
		DialogButton.Visible = false;
		DropDownButton.Visible = false;
		SetScrollOffset(newOffset);
		if (selectedGridEntry != null)
		{
			int rowFromGridEntry = GetRowFromGridEntry(selectedGridEntry);
			if (rowFromGridEntry >= 0 && rowFromGridEntry < _visibleRows - 1)
			{
				Edit.Visible = visible;
				DialogButton.Visible = visible3;
				DropDownButton.Visible = visible2;
				SelectGridEntry(selectedGridEntry, pageIn: true);
			}
			else
			{
				CommonEditorHide();
			}
		}
		else
		{
			CommonEditorHide();
		}
		Invalidate();
		return true;
	}

	private void SelectEdit()
	{
		Edit?.SelectAll();
	}

	internal void SelectGridEntry(GridEntry entry, bool pageIn)
	{
		if (entry == null)
		{
			return;
		}
		int rowFromGridEntry = GetRowFromGridEntry(entry);
		if (rowFromGridEntry + GetScrollOffset() < 0)
		{
			return;
		}
		int num = (int)Math.Ceiling((double)GetOurSize().Height / (double)(1 + RowHeight));
		if (!pageIn || (rowFromGridEntry >= 0 && rowFromGridEntry < num - 1))
		{
			SelectRow(rowFromGridEntry);
			return;
		}
		_selectedRow = -1;
		int scrollOffset = GetScrollOffset();
		if (rowFromGridEntry < 0)
		{
			SetScrollOffset(rowFromGridEntry + scrollOffset);
			Invalidate();
			SelectRow(0);
			return;
		}
		int num2 = rowFromGridEntry + scrollOffset - (num - 2);
		if (num2 >= ScrollBar.Minimum && num2 < ScrollBar.Maximum)
		{
			SetScrollOffset(num2);
		}
		Invalidate();
		SelectGridEntry(entry, pageIn: false);
	}

	private void SelectRow(int row)
	{
		if (!GetFlag(2))
		{
			if (FocusInside)
			{
				if (_errorState != 0 || (row != _selectedRow && !Commit()))
				{
					return;
				}
			}
			else
			{
				Focus();
			}
		}
		GridEntry gridEntryFromRow = GetGridEntryFromRow(row);
		if (row != _selectedRow)
		{
			UpdateResetCommand(gridEntryFromRow);
		}
		if (GetFlag(2) && GetGridEntryFromRow(_selectedRow) == null)
		{
			CommonEditorHide();
		}
		UpdateHelpAttributes(_selectedGridEntry, gridEntryFromRow);
		if (_selectedGridEntry != null)
		{
			_selectedGridEntry.HasFocus = false;
		}
		if (row < 0 || row >= _visibleRows)
		{
			CommonEditorHide();
			_selectedRow = row;
			_selectedGridEntry = gridEntryFromRow;
			Refresh();
		}
		else
		{
			if (gridEntryFromRow == null)
			{
				return;
			}
			bool flag = false;
			int selectedRow = _selectedRow;
			if (_selectedRow != row || !gridEntryFromRow.Equals(_selectedGridEntry))
			{
				CommonEditorHide();
				flag = true;
			}
			if (!flag)
			{
				CloseDropDown();
			}
			Rectangle rectangle = GetRectangle(row, 2);
			string propertyTextValue = gridEntryFromRow.GetPropertyTextValue();
			bool flag2 = gridEntryFromRow.NeedsDropDownButton | gridEntryFromRow.Enumerable;
			bool needsCustomEditorButton = gridEntryFromRow.NeedsCustomEditorButton;
			bool isCustomPaint = gridEntryFromRow.IsCustomPaint;
			rectangle.X++;
			rectangle.Width--;
			if ((needsCustomEditorButton || flag2) && !gridEntryFromRow.ShouldRenderReadOnly && FocusInside)
			{
				Control control = (flag2 ? DropDownButton : DialogButton);
				Size size = (DpiHelper.IsScalingRequirementMet ? new Size(SystemInformation.VerticalScrollBarArrowHeightForDpi(_deviceDpi), RowHeight) : new Size(SystemInformation.VerticalScrollBarArrowHeight, RowHeight));
				Rectangle targetRectangle = new Rectangle(rectangle.X + rectangle.Width - size.Width, rectangle.Y, size.Width, rectangle.Height);
				CommonEditorUse(control, targetRectangle);
				size = control.Size;
				rectangle.Width -= size.Width;
				control.Invalidate();
			}
			if (isCustomPaint)
			{
				rectangle.X += _paintIndent + 1;
				rectangle.Width -= _paintIndent + 1;
			}
			else
			{
				rectangle.X++;
				rectangle.Width--;
			}
			if ((GetFlag(2) || !Edit.Focused) && propertyTextValue != null && !propertyTextValue.Equals(Edit.Text))
			{
				Edit.Text = propertyTextValue;
				_originalTextValue = propertyTextValue;
				Edit.SelectionStart = 0;
				Edit.SelectionLength = 0;
			}
			Edit.AccessibleName = gridEntryFromRow.Label;
			if (gridEntryFromRow.ShouldSerializePropertyValue())
			{
				Edit.Font = GetBoldFont();
			}
			else
			{
				Edit.Font = Font;
			}
			if (GetFlag(4) || !gridEntryFromRow.HasValue || !FocusInside)
			{
				Edit.Visible = false;
			}
			else
			{
				rectangle.Offset(1, 1);
				rectangle.Height--;
				rectangle.Width--;
				CommonEditorUse(Edit, rectangle);
				bool shouldRenderReadOnly = gridEntryFromRow.ShouldRenderReadOnly;
				Edit.ForeColor = (shouldRenderReadOnly ? GrayTextColor : ForeColor);
				Edit.BackColor = BackColor;
				Edit.ReadOnly = shouldRenderReadOnly || !gridEntryFromRow.IsTextEditable;
				Edit.UseSystemPasswordChar = gridEntryFromRow.ShouldRenderPassword;
			}
			GridEntry selectedGridEntry = _selectedGridEntry;
			_selectedRow = row;
			_selectedGridEntry = gridEntryFromRow;
			OwnerGrid.SetStatusBox(gridEntryFromRow.PropertyLabel, gridEntryFromRow.PropertyDescription);
			if (_selectedGridEntry != null)
			{
				_selectedGridEntry.HasFocus = FocusInside;
			}
			if (!GetFlag(2))
			{
				Focus();
			}
			InvalidateRow(selectedRow);
			InvalidateRow(row);
			if (FocusInside)
			{
				SetFlag(2, value: false);
			}
			try
			{
				if (_selectedGridEntry != selectedGridEntry)
				{
					OwnerGrid.OnSelectedGridItemChanged(selectedGridEntry, _selectedGridEntry);
				}
			}
			catch
			{
			}
		}
	}

	public virtual void SetConstants()
	{
		_visibleRows = (int)Math.Ceiling((double)GetOurSize().Height / (double)(1 + RowHeight));
		Size ourSize = GetOurSize();
		if (ourSize.Width >= 0)
		{
			_labelRatio = Math.Max(Math.Min(_labelRatio, 9.0), 1.1);
			_labelWidth = _location.X + (int)((double)ourSize.Width / _labelRatio);
		}
		int labelWidth = _labelWidth;
		bool num = SetScrollbarLength();
		GridEntryCollection allGridEntries = GetAllGridEntries();
		if (allGridEntries != null)
		{
			int scrollOffset = GetScrollOffset();
			if (scrollOffset + _visibleRows >= allGridEntries.Count)
			{
				_visibleRows = allGridEntries.Count - scrollOffset;
			}
		}
		if (num && ourSize.Width >= 0)
		{
			_labelRatio = (double)GetOurSize().Width / (double)(labelWidth - _location.X);
		}
	}

	private void SetCommitError(ErrorState error)
	{
		SetCommitError(error, error == ErrorState.Thrown);
	}

	private void SetCommitError(ErrorState error, bool capture)
	{
		_errorState = error;
		if (error != 0)
		{
			CancelSplitterMove();
		}
		Edit.HookMouseDown = capture;
	}

	internal void SetExpand(GridEntry entry, bool value)
	{
		if (entry == null || !entry.Expandable)
		{
			return;
		}
		int rowFromGridEntry = GetRowFromGridEntry(entry);
		int selectedRow = _selectedRow;
		if (_selectedRow != -1 && rowFromGridEntry < _selectedRow && Edit.Visible)
		{
			Focus();
		}
		int scrollOffset = GetScrollOffset();
		int totalProperties = TotalProperties;
		entry.InternalExpanded = value;
		global::Interop.UiaCore.ExpandCollapseState expandCollapseState = ((!value) ? global::Interop.UiaCore.ExpandCollapseState.Expanded : global::Interop.UiaCore.ExpandCollapseState.Collapsed);
		global::Interop.UiaCore.ExpandCollapseState expandCollapseState2 = (value ? global::Interop.UiaCore.ExpandCollapseState.Expanded : global::Interop.UiaCore.ExpandCollapseState.Collapsed);
		_selectedGridEntry?.AccessibilityObject?.RaiseAutomationPropertyChangedEvent(global::Interop.UiaCore.UIA.ExpandCollapseExpandCollapseStatePropertyId, expandCollapseState, expandCollapseState2);
		RecalculateProperties();
		GridEntry gridEntry = _selectedGridEntry;
		if (!value)
		{
			for (GridEntry gridEntry2 = gridEntry; gridEntry2 != null; gridEntry2 = gridEntry2.ParentGridEntry)
			{
				if (gridEntry2.Equals(entry))
				{
					gridEntry = entry;
				}
			}
		}
		rowFromGridEntry = GetRowFromGridEntry(entry);
		SetConstants();
		int num = TotalProperties - totalProperties;
		if (value && num > 0 && num < _visibleRows && rowFromGridEntry + num >= _visibleRows && num < selectedRow)
		{
			SetScrollOffset(TotalProperties - totalProperties + scrollOffset);
		}
		Invalidate();
		SelectGridEntry(gridEntry, pageIn: false);
		int scrollOffset2 = GetScrollOffset();
		SetScrollOffset(0);
		SetConstants();
		SetScrollOffset(scrollOffset2);
	}

	private void SetFlag(short flag, bool value)
	{
		if (value)
		{
			_flags = (short)((ushort)_flags | (ushort)flag);
		}
		else
		{
			_flags &= (short)(~flag);
		}
	}

	public virtual void SetScrollOffset(int offset)
	{
		int num = Math.Max(0, Math.Min(TotalProperties - _visibleRows + 1, offset));
		int value = ScrollBar.Value;
		if (num != value && IsScrollValueValid(num) && _visibleRows > 0)
		{
			ScrollBar.Value = num;
			Invalidate();
			_selectedRow = GetRowFromGridEntry(_selectedGridEntry);
		}
	}

	internal virtual bool _Commit()
	{
		return Commit();
	}

	private bool Commit()
	{
		if (_errorState == ErrorState.MessageBoxUp)
		{
			return false;
		}
		if (!NeedsCommit)
		{
			SetCommitError(ErrorState.None);
			return true;
		}
		if (GetInPropertySet())
		{
			return false;
		}
		if (GetGridEntryFromRow(_selectedRow) == null)
		{
			return true;
		}
		bool flag = false;
		try
		{
			flag = CommitText(Edit.Text);
		}
		finally
		{
			if (!flag)
			{
				Edit.Focus();
				SelectEdit();
			}
			else
			{
				SetCommitError(ErrorState.None);
			}
		}
		return flag;
	}

	private bool CommitValue(object value)
	{
		GridEntry gridEntry = _selectedGridEntry;
		if (_selectedGridEntry == null && _selectedRow != -1)
		{
			gridEntry = GetGridEntryFromRow(_selectedRow);
		}
		if (gridEntry == null)
		{
			return true;
		}
		return CommitValue(gridEntry, value);
	}

	internal bool CommitValue(GridEntry entry, object value, bool closeDropDown = true)
	{
		int childCount = entry.ChildCount;
		bool hookMouseDown = Edit.HookMouseDown;
		object oldValue = null;
		try
		{
			oldValue = entry.PropertyValue;
		}
		catch
		{
		}
		try
		{
			SetFlag(16, value: true);
			if ((entry?.Enumerable ?? false) && closeDropDown)
			{
				CloseDropDown();
			}
			try
			{
				Edit.DisableMouseHook = true;
				entry.PropertyValue = value;
			}
			finally
			{
				Edit.DisableMouseHook = false;
				Edit.HookMouseDown = hookMouseDown;
			}
		}
		catch (Exception ex)
		{
			SetCommitError(ErrorState.Thrown);
			ShowInvalidMessage(entry.PropertyLabel, ex);
			return false;
		}
		finally
		{
			SetFlag(16, value: false);
		}
		SetCommitError(ErrorState.None);
		string propertyTextValue = entry.GetPropertyTextValue();
		if (!string.Equals(propertyTextValue, Edit.Text))
		{
			Edit.Text = propertyTextValue;
			Edit.SelectionStart = 0;
			Edit.SelectionLength = 0;
		}
		_originalTextValue = propertyTextValue;
		UpdateResetCommand(entry);
		if (entry.ChildCount != childCount)
		{
			ClearGridEntryEvents(_allGridEntries, 0, -1);
			_allGridEntries = null;
			SelectGridEntry(entry, pageIn: true);
		}
		if (entry.Disposed)
		{
			bool num = _edit != null && _edit.Focused;
			SelectGridEntry(entry, pageIn: true);
			entry = _selectedGridEntry;
			if (num && _edit != null)
			{
				_edit.Focus();
			}
		}
		OwnerGrid.OnPropertyValueSet(entry, oldValue);
		return true;
	}

	private bool CommitText(string text)
	{
		GridEntry gridEntry = _selectedGridEntry;
		if (_selectedGridEntry == null && _selectedRow != -1)
		{
			gridEntry = GetGridEntryFromRow(_selectedRow);
		}
		if (gridEntry == null)
		{
			return true;
		}
		object value;
		try
		{
			value = gridEntry.ConvertTextToValue(text);
		}
		catch (Exception ex)
		{
			SetCommitError(ErrorState.Thrown);
			ShowInvalidMessage(gridEntry.PropertyLabel, ex);
			return false;
		}
		SetCommitError(ErrorState.None);
		return CommitValue(value);
	}

	internal void ReverseFocus()
	{
		if (_selectedGridEntry == null)
		{
			Focus();
			return;
		}
		SelectGridEntry(_selectedGridEntry, pageIn: true);
		if (DialogButton.Visible)
		{
			DialogButton.Focus();
		}
		else if (DropDownButton.Visible)
		{
			DropDownButton.Focus();
		}
		else if (Edit.Visible)
		{
			Edit.SelectAll();
			Edit.Focus();
		}
	}

	private bool SetScrollbarLength()
	{
		if (TotalProperties == -1)
		{
			return false;
		}
		if (TotalProperties < _visibleRows)
		{
			SetScrollOffset(0);
		}
		else if (GetScrollOffset() > TotalProperties)
		{
			SetScrollOffset(TotalProperties + 1 - _visibleRows);
		}
		bool flag = !ScrollBar.Visible;
		if (_visibleRows > 0)
		{
			ScrollBar.LargeChange = _visibleRows - 1;
		}
		bool result = false;
		ScrollBar.Maximum = Math.Max(0, TotalProperties - 1);
		if (flag != TotalProperties < _visibleRows)
		{
			result = true;
			ScrollBar.Visible = flag;
			Size ourSize = GetOurSize();
			if (_labelWidth != -1 && ourSize.Width > 0)
			{
				if (_labelWidth > _location.X + ourSize.Width)
				{
					_labelWidth = _location.X + (int)((double)ourSize.Width / _labelRatio);
				}
				else
				{
					_labelRatio = (double)GetOurSize().Width / (double)(_labelWidth - _location.X);
				}
			}
			Invalidate();
		}
		return result;
	}

	public DialogResult ShowDialog(Form dialog)
	{
		if (dialog.StartPosition == FormStartPosition.CenterScreen)
		{
			Control control = this;
			if (control != null)
			{
				while (control.ParentInternal != null)
				{
					control = control.ParentInternal;
				}
				if (control.Size.Equals(dialog.Size))
				{
					dialog.StartPosition = FormStartPosition.Manual;
					Point location = control.Location;
					location.Offset(25, 25);
					dialog.Location = location;
				}
			}
		}
		IntPtr focus = global::Interop.User32.GetFocus();
		IUIService service;
		DialogResult result = ((!TryGetService<IUIService>(out service)) ? dialog.ShowDialog(this) : service.ShowDialog(dialog));
		if (focus != IntPtr.Zero)
		{
			global::Interop.User32.SetFocus(focus);
		}
		return result;
	}

	private void ShowFormatExceptionMessage(string propertyName, Exception ex)
	{
		if (propertyName == null)
		{
			propertyName = "(unknown)";
		}
		bool hookMouseDown = Edit.HookMouseDown;
		Edit.DisableMouseHook = true;
		SetCommitError(ErrorState.MessageBoxUp, capture: false);
		global::Interop.User32.MSG msg = default(global::Interop.User32.MSG);
		while (global::Interop.User32.PeekMessageW(ref msg, IntPtr.Zero, global::Interop.User32.WM.MOUSEFIRST, global::Interop.User32.WM.MOUSEHWHEEL, global::Interop.User32.PM.REMOVE).IsTrue())
		{
		}
		if (ex is TargetInvocationException)
		{
			ex = ex.InnerException;
		}
		string message = ex.Message;
		while (message == null || message.Length == 0)
		{
			ex = ex.InnerException;
			if (ex == null)
			{
				break;
			}
			message = ex.Message;
		}
		ErrorDialog.Message = System.SR.PBRSFormatExceptionMessage;
		ErrorDialog.Text = System.SR.PBRSErrorTitle;
		ErrorDialog.Details = message;
		IUIService service;
		bool flag = ((!TryGetService<IUIService>(out service)) ? (DialogResult.Cancel == ShowDialog(ErrorDialog)) : (DialogResult.Cancel == service.ShowDialog(ErrorDialog)));
		Edit.DisableMouseHook = false;
		if (hookMouseDown)
		{
			SelectGridEntry(_selectedGridEntry, pageIn: true);
		}
		SetCommitError(ErrorState.Thrown, hookMouseDown);
		if (flag)
		{
			OnEscape(Edit);
		}
	}

	internal void ShowInvalidMessage(string propertyName, Exception ex)
	{
		if (propertyName == null)
		{
			propertyName = "(unknown)";
		}
		bool hookMouseDown = Edit.HookMouseDown;
		Edit.DisableMouseHook = true;
		SetCommitError(ErrorState.MessageBoxUp, capture: false);
		global::Interop.User32.MSG msg = default(global::Interop.User32.MSG);
		while (global::Interop.User32.PeekMessageW(ref msg, IntPtr.Zero, global::Interop.User32.WM.MOUSEFIRST, global::Interop.User32.WM.MOUSEHWHEEL, global::Interop.User32.PM.REMOVE).IsTrue())
		{
		}
		if (ex is TargetInvocationException)
		{
			ex = ex.InnerException;
		}
		string message = ex.Message;
		while (message == null || message.Length == 0)
		{
			ex = ex.InnerException;
			if (ex == null)
			{
				break;
			}
			message = ex.Message;
		}
		ErrorDialog.Message = System.SR.PBRSErrorInvalidPropertyValue;
		ErrorDialog.Text = System.SR.PBRSErrorTitle;
		ErrorDialog.Details = message;
		IUIService service;
		bool flag = ((!TryGetService<IUIService>(out service)) ? (DialogResult.Cancel == ShowDialog(ErrorDialog)) : (DialogResult.Cancel == service.ShowDialog(ErrorDialog)));
		Edit.DisableMouseHook = false;
		if (hookMouseDown)
		{
			SelectGridEntry(_selectedGridEntry, pageIn: true);
		}
		SetCommitError(ErrorState.Thrown, hookMouseDown);
		if (flag)
		{
			OnEscape(Edit);
		}
	}

	private bool SplitterInside(int x)
	{
		return Math.Abs(x - InternalLabelWidth) < 4;
	}

	private void TabSelection()
	{
		if (GetGridEntryFromRow(_selectedRow) != null)
		{
			if (Edit.Visible)
			{
				Edit.Focus();
				SelectEdit();
			}
			else if (_dropDownHolder != null && _dropDownHolder.Visible)
			{
				_dropDownHolder.FocusComponent();
			}
			else if (_currentEditor != null)
			{
				_currentEditor.Focus();
			}
		}
	}

	internal void RemoveSelectedEntryHelpAttributes()
	{
		UpdateHelpAttributes(_selectedGridEntry, null);
	}

	private void UpdateHelpAttributes(GridEntry oldEntry, GridEntry newEntry)
	{
		IHelpService helpService = GetHelpService();
		if (helpService == null || oldEntry == newEntry)
		{
			return;
		}
		GridEntry gridEntry = oldEntry;
		if (oldEntry != null && !oldEntry.Disposed)
		{
			while (gridEntry != null)
			{
				helpService.RemoveContextAttribute("Keyword", gridEntry.HelpKeyword);
				gridEntry = gridEntry.ParentGridEntry;
			}
		}
		if (newEntry != null)
		{
			gridEntry = newEntry;
			UpdateHelpAttributes(helpService, gridEntry, addAsF1: true);
		}
	}

	private void UpdateHelpAttributes(IHelpService helpService, GridEntry entry, bool addAsF1)
	{
		if (entry != null)
		{
			UpdateHelpAttributes(helpService, entry.ParentGridEntry, addAsF1: false);
			string helpKeyword = entry.HelpKeyword;
			if (helpKeyword != null)
			{
				helpService.AddContextAttribute("Keyword", helpKeyword, (!addAsF1) ? HelpKeywordType.GeneralKeyword : HelpKeywordType.F1Keyword);
			}
		}
	}

	private void UpdateUIBasedOnFont(bool layoutRequired)
	{
		if (!base.IsHandleCreated || !GetFlag(128))
		{
			return;
		}
		try
		{
			if (_listBox != null)
			{
				DropDownListBox.ItemHeight = RowHeight + 2;
			}
			if (_dropDownButton != null)
			{
				bool isScalingRequirementMet = DpiHelper.IsScalingRequirementMet;
				if (isScalingRequirementMet)
				{
					_dropDownButton.Size = new Size(SystemInformation.VerticalScrollBarArrowHeightForDpi(_deviceDpi), RowHeight);
				}
				else
				{
					_dropDownButton.Size = new Size(SystemInformation.VerticalScrollBarArrowHeight, RowHeight);
				}
				if (_dialogButton != null)
				{
					DialogButton.Size = DropDownButton.Size;
					if (isScalingRequirementMet)
					{
						_dialogButton.Image = CreateResizedBitmap("dotdotdot", 7, 8);
					}
				}
				if (isScalingRequirementMet)
				{
					_dropDownButton.Image = CreateResizedBitmap("Arrow", 16, 16);
				}
			}
			if (layoutRequired)
			{
				LayoutWindow(invalidate: true);
			}
		}
		finally
		{
			SetFlag(128, value: false);
		}
	}

	private bool UnfocusSelection()
	{
		if (GetGridEntryFromRow(_selectedRow) == null)
		{
			return true;
		}
		bool num = Commit();
		if (num && FocusInside)
		{
			Focus();
		}
		return num;
	}

	private void UpdateResetCommand(GridEntry gridEntry)
	{
		if (TotalProperties > 0 && TryGetService<IMenuCommandService>(out IMenuCommandService service))
		{
			MenuCommand menuCommand = service.FindCommand(PropertyGridCommands.Reset);
			if (menuCommand != null)
			{
				menuCommand.Enabled = gridEntry?.CanResetPropertyValue() ?? false;
			}
		}
	}

	internal bool WantsTab(bool forward)
	{
		if (forward)
		{
			if (Focused)
			{
				if (DropDownButton.Visible || DialogButton.Visible || Edit.Visible)
				{
					return true;
				}
			}
			else if (Edit.Focused && (DropDownButton.Visible || DialogButton.Visible))
			{
				return true;
			}
			return OwnerGrid.WantsTab(forward);
		}
		if (Edit.Focused || DropDownButton.Focused || DialogButton.Focused)
		{
			return true;
		}
		return OwnerGrid.WantsTab(forward);
	}

	private unsafe bool WmNotify(ref Message m)
	{
		if (m.LParam != IntPtr.Zero)
		{
			global::Interop.User32.NMHDR* ptr = (global::Interop.User32.NMHDR*)(void*)m.LParam;
			if (ptr->hwndFrom == ToolTip.Handle)
			{
				global::Interop.ComCtl32.TTN code = (global::Interop.ComCtl32.TTN)ptr->code;
				if (code != global::Interop.ComCtl32.TTN.POP && code == global::Interop.ComCtl32.TTN.SHOW)
				{
					Point position = Cursor.Position;
					position = PointToClient(position);
					position = FindPosition(position.X, position.Y);
					if (!(position == InvalidPosition))
					{
						GridEntry gridEntryFromRow = GetGridEntryFromRow(position.Y);
						if (gridEntryFromRow != null)
						{
							Rectangle rectangle = GetRectangle(position.Y, position.X);
							Point empty = Point.Empty;
							if (position.X == 1)
							{
								empty = gridEntryFromRow.GetLabelToolTipLocation(position.X - rectangle.X, position.Y - rectangle.Y);
							}
							else
							{
								if (position.X != 2)
								{
									goto IL_0135;
								}
								empty = gridEntryFromRow.ValueToolTipLocation;
							}
							if (empty != InvalidPoint)
							{
								rectangle.Offset(empty);
								PositionTooltip(this, ToolTip, rectangle);
								m.Result = (IntPtr)1;
								return true;
							}
						}
					}
				}
			}
		}
		goto IL_0135;
		IL_0135:
		return false;
	}

	protected override void WndProc(ref Message m)
	{
		switch (m.Msg)
		{
		case 21:
			Invalidate();
			break;
		case 7:
			if (!GetInPropertySet() && Edit.Visible && (_errorState != 0 || !Commit()))
			{
				base.WndProc(ref m);
				Edit.Focus();
				return;
			}
			break;
		case 269:
			Edit.Focus();
			Edit.Clear();
			global::Interop.User32.PostMessageW(Edit, global::Interop.User32.WM.IME_STARTCOMPOSITION, (IntPtr)0, (IntPtr)0);
			return;
		case 271:
			Edit.Focus();
			global::Interop.User32.PostMessageW(Edit, global::Interop.User32.WM.IME_COMPOSITION, m.WParam, m.LParam);
			return;
		case 135:
		{
			int num = 129;
			if (_selectedGridEntry != null && (Control.ModifierKeys & Keys.Shift) == 0 && _edit.Visible)
			{
				num |= 2;
			}
			m.Result = (IntPtr)num;
			return;
		}
		case 512:
			if ((int)(long)m.LParam == _lastMouseMove)
			{
				return;
			}
			_lastMouseMove = (int)(long)m.LParam;
			break;
		case 78:
			if (WmNotify(ref m))
			{
				return;
			}
			break;
		case 1111:
			m.Result = (IntPtr)GetRowFromGridEntry(_selectedGridEntry);
			return;
		case 1110:
			m.Result = (IntPtr)Math.Min(_visibleRows, TotalProperties);
			return;
		}
		base.WndProc(ref m);
	}

	protected override void RescaleConstantsForDpi(int deviceDpiOld, int deviceDpiNew)
	{
		base.RescaleConstantsForDpi(deviceDpiOld, deviceDpiNew);
		RescaleConstants();
	}

	private void RescaleConstants()
	{
		if (!DpiHelper.IsScalingRequirementMet)
		{
			return;
		}
		_cachedRowHeight = -1;
		_paintWidth = LogicalToDeviceUnits(20);
		_paintIndent = LogicalToDeviceUnits(26);
		_outlineSizeExplorerTreeStyle = LogicalToDeviceUnits(16);
		_outlineSize = LogicalToDeviceUnits(9);
		_maxListBoxHeight = LogicalToDeviceUnits(200);
		_offset2Units = LogicalToDeviceUnits(2);
		if (TopLevelGridEntries == null)
		{
			return;
		}
		foreach (GridEntry topLevelGridEntry in TopLevelGridEntries)
		{
			ResetOutline(topLevelGridEntry);
		}
	}

	private void ResetOutline(GridEntry entry)
	{
		entry.OutlineRect = Rectangle.Empty;
		if (entry.ChildCount <= 0)
		{
			return;
		}
		foreach (GridEntry child in entry.Children)
		{
			ResetOutline(child);
		}
	}
}
