#region Assembly Microsoft.SqlServer.GridControl, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLCommon\Microsoft.SqlServer.GridControl.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Security.Permissions;
using System.Windows.Forms;

using BlackbirdSql.Common;
using BlackbirdSql.Common.Controls.Events;
using BlackbirdSql.Common.Controls.Interfaces;

using Microsoft.VisualStudio;




// namespace Microsoft.SqlServer.Management.UI.Grid
namespace BlackbirdSql.Common.Controls.Grid
{
	public class EmbeddedTextBox : TextBox, IBGridEmbeddedControl, IBGridEmbeddedControlManagement2, IBGridEmbeddedControlManagement
	{
		protected int m_MarginsWidth;

		protected long m_RowIndex = -1L;

		protected int m_ColumnIndex = -1;

		protected bool m_alwaysShowContextMenu = true;

		public bool AlwaysShowContextMenu
		{
			get
			{
				return m_alwaysShowContextMenu;
			}
			set
			{
				m_alwaysShowContextMenu = value;
			}
		}

		public bool WantMouseClick => true;

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

		protected EmbeddedTextBox()
		{
		}

		public EmbeddedTextBox(Control parent, int MarginsWidth)
		{
			m_MarginsWidth = MarginsWidth;
			Parent = parent;
			Multiline = false;
			Text = "";
			Visible = false;
			AutoSize = false;
			BorderStyle = !Application.RenderWithVisualStyles ? BorderStyle.FixedSingle : BorderStyle.Fixed3D;
			TextChanged += OnTextChanged;
		}

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override bool ProcessKeyMessage(ref Message m)
		{
			Keys keys = (Keys)(int)m.WParam;
			Keys modifierKeys = ModifierKeys;
			if ((keys | modifierKeys) == Keys.Return || (keys | modifierKeys) == Keys.Escape)
			{
				if (m.Msg == Native.WM_CHAR)
				{
					return true;
				}

				return ProcessKeyPreview(ref m);
			}

			if (m.Msg == Native.WM_CHAR)
			{
				return ProcessKeyEventArgs(ref m);
			}

			switch (keys & Keys.KeyCode)
			{
				case Keys.Tab:
				case Keys.Prior:
				case Keys.Next:
				case Keys.Up:
				case Keys.Down:
					return ProcessKeyPreview(ref m);
				default:
					return ProcessKeyEventArgs(ref m);
			}
		}

		[SecurityPermission(SecurityAction.InheritanceDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case 48:
					WmSetFont(ref m);
					break;
				case 123:
					if (ContextMenu != null || (m_alwaysShowContextMenu && ContextMenu == null))
					{
						base.WndProc(ref m);
					}

					break;
				default:
					base.WndProc(ref m);
					break;
			}
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
					Text = string.Empty;
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
				Text = c.ToString();
				SelectedText = string.Empty;
				Select(1, 0);
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

		public void SetHorizontalAlignment(HorizontalAlignment alignment)
		{
			TextAlign = alignment;
		}

		public void ClearData()
		{
			Text = "";
			PasswordChar = '\0';
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

		private void WmSetFont(ref Message m)
		{
			base.WndProc(ref m);
			Native.SendMessage(Handle, 211, (IntPtr)3, Native.Util.MAKELPARAM(m_MarginsWidth, m_MarginsWidth));
		}

		private void SetDataInternal(string myText)
		{
			Text = myText;
			Select(0, 0);
		}

		private void OnTextChanged(object sender, EventArgs args)
		{
			ContentsChangedEvent?.Invoke(this, args);
		}
	}
}
