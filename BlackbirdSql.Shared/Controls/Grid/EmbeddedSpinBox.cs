// Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.UI.Grid.EmbeddedSpinBox

using System;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Security.Permissions;
using System.Windows.Forms;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;



namespace BlackbirdSql.Shared.Controls.Grid;


internal class EmbeddedSpinBox : NumericUpDown, IBsGridEmbeddedControl, IBsGridEmbeddedControlManagement2, IBsGridEmbeddedControlManagement, IBsGridEmbeddedSpinControl
{
	protected int m_MarginsWidth;

	protected long m_RowIndex = -1L;

	protected int m_ColumnIndex = -1;

	protected int m_myDefWidth = 120;

	protected override Size DefaultSize => new Size(m_myDefWidth, Height);

	public bool WantMouseClick => true;

	public new decimal Increment
	{
		get
		{
			return base.Increment;
		}
		set
		{
			base.Increment = value;
		}
	}

	public new decimal Minimum
	{
		get
		{
			return base.Minimum;
		}
		set
		{
			base.Minimum = value;
		}
	}

	public new decimal Maximum
	{
		get
		{
			return base.Maximum;
		}
		set
		{
			base.Maximum = value;
		}
	}

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

	protected EmbeddedSpinBox()
	{
	}

	public EmbeddedSpinBox(Control parent, int MarginsWidth)
	{
		m_MarginsWidth = MarginsWidth;
		OnFontChanged(new EventArgs());
		Parent = parent;
		Value = Minimum;
		Visible = false;
		ValueChanged += OnValueChanged;
		foreach (Control control in Controls)
		{
			if (control is TextBox)
			{
				control.TextChanged += OnValueChanged;
			}
		}
	}

	protected override void OnFontChanged(EventArgs e)
	{
		base.OnFontChanged(e);
		foreach (Control control in Controls)
		{
			if (control is TextBox)
			{
				Native.SendMessage(control.Handle, 211, (IntPtr)3, Native.UtilI.MAKELPARAM(m_MarginsWidth, m_MarginsWidth));
			}
		}
	}

	[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
	protected override bool ProcessKeyPreview(ref Message m)
	{
		Keys keys = (Keys)(int)m.WParam;
		Keys modifierKeys = ModifierKeys;
		if ((keys | modifierKeys) == Keys.Return || (keys | modifierKeys) == Keys.Escape)
		{
			if (m.Msg == Native.WM_CHAR)
			{
				return true;
			}

			ValidateEditText();
			return base.ProcessKeyPreview(ref m);
		}

		if (m.Msg == Native.WM_CHAR)
		{
			return ProcessKeyEventArgs(ref m);
		}

		Keys keys2 = keys & Keys.KeyCode;
		if (keys2 == Keys.Tab || (uint)(keys2 - 33) <= 1u)
		{
			return base.ProcessKeyPreview(ref m);
		}

		return ProcessKeyEventArgs(ref m);
	}

	protected override void OnTextBoxResize(object source, EventArgs e)
	{
		int height = Height;
		base.OnTextBoxResize(source, e);
		Height = height;
	}

	protected override void SetBoundsCore(int x, int y, int width, int height, BoundsSpecified specified)
	{
		Rectangle bounds = Bounds;
		if (bounds.X == x && bounds.Y == y && bounds.Width == width && bounds.Height == height)
		{
			return;
		}

		if (Handle != (IntPtr)0)
		{
			int num = 20;
			if (bounds.X == x && bounds.Y == y)
			{
				num |= 2;
			}

			if (bounds.Width == width && bounds.Height == height)
			{
				num |= 1;
			}

			Native.SetWindowPos(Handle, (IntPtr)0, x, y, width, height, num);
		}
		else
		{
			UpdateBounds(x, y, width, height);
		}
	}

	public void SetHorizontalAlignment(HorizontalAlignment alignment)
	{
		TextAlign = alignment;
	}

	public void ReceiveKeyboardEvent(KeyEventArgs ke)
	{
		bool flag = false;
		if (!ReadOnly && ke.Modifiers == Keys.None)
		{
			if (ke.KeyCode == Keys.F2)
			{
				Select(Text.Length, 0);
				flag = true;
			}
			else if (ke.KeyCode == Keys.Back || ke.KeyCode == Keys.Delete)
			{
				Text = "";
				flag = true;
			}

			if (!ContainsFocus && flag)
			{
				Focus();
			}
		}
	}

	public void ReceiveChar(char c)
	{
		if (!ReadOnly)
		{
			try
			{
				Text = c.ToString();
				Select(1, 0);
			}
			catch (Exception ex)
			{
				Trace.TraceInformation(ex.Message);
			}
		}
	}

	public void PostProcessFocusFromKeyboard(Keys keyStroke, Keys modifiers)
	{
		string text = Text;
		if (text != null)
		{
			Select(0, text.Length);
		}
	}

	public void ClearData()
	{
		Text = Minimum.ToString(CultureInfo.CurrentUICulture);
		Select(0, 0);
	}

	public int AddDataAsString(string Item)
	{
		SetDataInternal(Item);
		return VSConstants.S_OK;
	}

	public int SetCurSelectionAsString(string strNewSel)
	{
		SetDataInternal(strNewSel);
		return VSConstants.S_OK;
	}

	public void SetCurSelectionIndex(int nIndex)
	{
	}

	public int GetCurSelectionIndex()
	{
		return VSConstants.S_OK;
	}

	public string GetCurSelectionAsString()
	{
		return Text;
	}

	private void SetDataInternal(string myText)
	{
		Text = myText;
		Select(0, 0);
	}

	private void OnValueChanged(object sender, EventArgs args)
	{
		ContentsChangedEvent?.Invoke(this, EventArgs.Empty);
	}
}
