// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.EmbeddedComboBox

using System;
using System.Drawing;
using System.Drawing.Text;
using System.Globalization;
using System.Security.Permissions;
using System.Windows.Forms;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;



namespace BlackbirdSql.Shared.Controls.Grid;


public class EmbeddedComboBox : ComboBox, IBsGridEmbeddedControl, IBsGridEmbeddedControlManagement2, IBsGridEmbeddedControlManagement
{
	protected int m_MarginsWidth;

	protected long m_RowIndex = -1L;

	protected int m_ColumnIndex = -1;

	protected StringFormat m_myStringFormat;

	protected TextFormatFlags m_TextFormatFlags;

	public bool WantMouseClick => false;

	public int ColumnIndex
	{
		get
		{
			return m_ColumnIndex;
		}
		set
		{
			m_ColumnIndex = value;
		}
	}

	public long RowIndex
	{
		get
		{
			return m_RowIndex;
		}
		set
		{
			m_RowIndex = value;
		}
	}

	public new bool Enabled
	{
		get
		{
			return base.Enabled;
		}
		set
		{
			base.Enabled = value;
		}
	}

	public event ContentsChangedEventHandler ContentsChangedEvent;

	protected EmbeddedComboBox()
	{
	}

	public EmbeddedComboBox(Control parent, int MarginsWidth, ComboBoxStyle style)
	{
		m_MarginsWidth = MarginsWidth;
		m_myStringFormat = new StringFormat(StringFormatFlags.NoWrap | StringFormatFlags.LineLimit)
		{
			HotkeyPrefix = HotkeyPrefix.None,
			Trimming = StringTrimming.EllipsisCharacter,
			LineAlignment = StringAlignment.Center,
			Alignment = StringAlignment.Near
		};
		m_TextFormatFlags = TextFormatFlags.EndEllipsis | TextFormatFlags.NoPrefix | TextFormatFlags.SingleLine | TextFormatFlags.VerticalCenter;
		Parent = parent;
		DropDownStyle = style;
		Visible = false;
		IntegralHeight = false;
		DrawMode = DrawMode.OwnerDrawFixed;
		SetStringFormatRTL(RightToLeft == RightToLeft.Yes);
		if (style == ComboBoxStyle.DropDown)
		{
			TextChanged += OnContentsChanged;
		}
		else
		{
			SelectedIndexChanged += OnContentsChanged;
		}
	}

	protected override void Dispose(bool disposing)
	{
		if (m_myStringFormat != null)
		{
			m_myStringFormat.Dispose();
			m_myStringFormat = null;
		}

		base.Dispose(disposing);
	}

	protected override void OnHandleDestroyed(EventArgs e)
	{
		base.OnHandleDestroyed(e);
	}

	protected override void OnRightToLeftChanged(EventArgs a)
	{
		base.OnRightToLeftChanged(a);
		SetStringFormatRTL(RightToLeft == RightToLeft.Yes);
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		Rectangle bounds = Bounds;
		base.SetBoundsCore(x, y, width, height, specified);
		if (bounds.Height != height && IsHandleCreated)
		{
			ItemHeight = Math.Max(1, height - SystemInformation.Border3DSize.Height - 3 * SystemInformation.BorderSize.Height);
		}
	}

	protected override void OnDrawItem(DrawItemEventArgs a)
	{
		if (a.Index == -1)
		{
			return;
		}

		a.DrawBackground();
		a.DrawFocusRectangle();
		Rectangle bounds = a.Bounds;
		bounds.Offset(2, 0);
		using SolidBrush solidBrush = new SolidBrush(a.ForeColor);
		object obj = Items[a.Index];
		if (obj != null)
		{
			TextRenderer.DrawText(a.Graphics, obj.ToString(), a.Font, bounds, solidBrush.Color, m_TextFormatFlags);
		}
		else
		{
			TextRenderer.DrawText(a.Graphics, null, a.Font, bounds, solidBrush.Color, m_TextFormatFlags);
		}
	}

	protected override void OnHandleCreated(EventArgs e)
	{
		base.OnHandleCreated(e);
	}

	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyMessage(ref Message m)
	{
		Keys keys = (Keys)(int)m.WParam;
		_ = ModifierKeys;
		if (m.Msg == Native.WM_CHAR)
		{
			return ProcessKeyEventArgs(ref m);
		}

		switch (keys & Keys.KeyCode)
		{
			case Keys.Tab:
				return base.ProcessKeyPreview(ref m);
			case Keys.Return:
			case Keys.Escape:
				if (DroppedDown)
				{
					return ProcessKeyEventArgs(ref m);
				}

				return base.ProcessKeyPreview(ref m);
			default:
				return ProcessKeyEventArgs(ref m);
		}
	}

	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override void WndProc(ref Message m)
	{
		if (m.Msg == Native.WM_SETFONT)
		{
			WmSetFont(ref m);
		}
		else
		{
			base.WndProc(ref m);
		}
	}

	public void ReceiveKeyboardEvent(KeyEventArgs ke)
	{
		if (ke.KeyCode == Keys.F2 && ke.Modifiers == Keys.None)
		{
			if (!ContainsFocus)
			{
				Focus();
			}

			Select(Text.Length, 0);
		}
		else if (ke.KeyCode == Keys.F4 || ke.KeyCode == Keys.Down && ke.Alt)
		{
			if (!ContainsFocus)
			{
				Focus();
			}

			DroppedDown = true;
		}
	}

	public void ReceiveChar(char c)
	{
		if (!Enabled)
			return;

		if (DropDownStyle == ComboBoxStyle.DropDownList)
		{
			if (c.ToString() == null)
				return;

			string value = c.ToString().ToLower(CultureInfo.CurrentUICulture);
			string value2 = c.ToString().ToUpper(CultureInfo.CurrentUICulture);
			for (int i = 0; i < Items.Count; i++)
			{
				string text = Items[i].ToString();
				if (text != null && (text.StartsWith(value) || text.StartsWith(value2)))
				{
					SelectedIndex = i;
					break;
				}
			}
		}
		else
		{
			Text = c.ToString();
			SelectedText = "";
			Select(1, 0);
		}
	}

	public void PostProcessFocusFromKeyboard(Keys keyStroke, Keys modifiers)
	{
	}

	public void SetHorizontalAlignment(HorizontalAlignment alignment)
	{
	}

	public void ClearData()
	{
		Items.Clear();
		Text = "";
		if (IsHandleCreated)
		{
			Select(0, 1);
		}
	}

	public int AddDataAsString(string Item)
	{
		return Items.Add(Item);
	}

	public int SetCurSelectionAsString(string strNewSel)
	{
		int num = FindStringExact(strNewSel);
		if (num == -1)
		{
			if (DropDownStyle != ComboBoxStyle.DropDown)
			{
				InvalidOperationException ex = new(ControlsResources.ExInvalidSelStringInEmbeddedCombo);
				Diag.Ex(ex);
				throw ex;
			}

			Text = strNewSel;
			Select(0, 1);
		}
		else
		{
			SetCurSelectionIndex(num);
		}

		return num;
	}

	public void SetCurSelectionIndex(int nIndex)
	{
		if (nIndex < 0 || nIndex >= Items.Count)
		{
			ArgumentException ex = new(ControlsResources.ExInvalidSelIndexInEmbeddedCombo, "nIndex");
			Diag.Ex(ex);
			throw ex;
		}

		SelectedIndex = nIndex;
		Select(0, 1);
	}

	public int GetCurSelectionIndex()
	{
		return SelectedIndex;
	}

	public string GetCurSelectionAsString()
	{
		return Text;
	}

	private void SetStringFormatRTL(bool bRtl)
	{
		if (bRtl)
		{
			m_myStringFormat.FormatFlags |= StringFormatFlags.DirectionRightToLeft;
			m_TextFormatFlags |= TextFormatFlags.RightToLeft;
		}
		else
		{
			m_myStringFormat.FormatFlags &= ~StringFormatFlags.DirectionRightToLeft;
			m_TextFormatFlags &= ~TextFormatFlags.RightToLeft;
		}
	}

	private void WmSetFont(ref Message m)
	{
		base.WndProc(ref m);
		if (DropDownStyle != ComboBoxStyle.DropDownList)
		{
			IntPtr window = Native.GetWindow(Handle, 5);
			if (window != (IntPtr)0)
			{
				Native.SendMessage(window, 211, (IntPtr)3, Native.Util.MAKELPARAM(m_MarginsWidth, m_MarginsWidth));
			}
		}
	}

	private void OnContentsChanged(object sender, EventArgs args)
	{
		ContentsChangedEvent?.Invoke(this, EventArgs.Empty);
	}
}
