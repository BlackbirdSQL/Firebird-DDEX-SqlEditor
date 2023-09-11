#region Assembly Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Text;
using System.Windows.Forms;

using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Events;

using FirebirdSql.Data.FirebirdClient;

namespace BlackbirdSql.Common.Exceptions
{
	public sealed class ExceptionMessageBoxDlg : Form
	{
		public enum BeepType
		{
			Standard = -1,
			Default = 0,
			Hand = 0x10,
			Question = 0x20,
			Exclamation = 48,
			Asterisk = 0x40
		}

		private Exception exMessage;

		private EnExceptionMessageBoxDefaultButton defButton;

		private EnExceptionMessageBoxSymbol symbol = EnExceptionMessageBoxSymbol.Warning;

		private EnExceptionMessageBoxButtons buttons;

		private EnExceptionMessageBoxOptions options;

		// private readonly ArrayList lnkArray = new ArrayList();

		private bool m_showHelpButton = true;

		private int m_messageLimitCount = -1;

		private Icon m_formIcon;

		private Icon m_iconSymbol;

		private string[] m_buttonTextArray;

		private int m_buttonCount;

		private readonly ArrayList m_helpUrlArray = new ArrayList(5);

		private int m_helpUrlCount;

		private EnExceptionMessageBoxDialogResult m_customDR;

		private Bitmap m_customSymbol;

		private bool m_showCheckbox;

		private bool isButtonPressed;

		private bool m_doBeep = true;

		private readonly ToolStripDropDown m_dropdown = new ToolStripDropDown();

		private const string C_HelpUrlBaseKey = "HelpLink.BaseHelpUrl";

		private const string C_PrefixKey = "HelpLink.";

		private const string C_AdvancedKey = "AdvancedInformation.";

		private const string C_HttpPrefixKey = "HTTP";

		// private const int C_MaxAdditionalPanelHeight = 150;

		// private const int C_WM_USER = 1024;

		private const int C_WM_GETDLGID = 7690;

		private IContainer components;

		private TableLayoutPanel pnlForm;

		private Panel pnlIcon;

		private TableLayoutPanel pnlMessage;

		private LinkLabel lblTopMessage;

		private Label lblAdditionalInfo;

		private TableLayoutPanel pnlAdditional;

		private WrappingCheckBox chkDontShow;

		private GroupBox grpSeparator;

		private ImageList imgIcons;

		private Button button1;

		private Button button2;

		private Button button3;

		private Button button4;

		private Button button5;

		private TableLayoutPanel pnlButtons;

		private ToolStrip toolStrip1;

		private ToolStripDropDownButton tbBtnHelp;

		private ToolStripButton tbBtnCopy;

		private ToolStripButton tbBtnAdvanced;

		private ToolStripButton tbBtnHelpSingle;

		public bool DoBeep
		{
			get
			{
				return m_doBeep;
			}
			set
			{
				m_doBeep = value;
			}
		}

		public Exception Message
		{
			get
			{
				return exMessage;
			}
			set
			{
				exMessage = value;
			}
		}

		public string Caption
		{
			get
			{
				return Text;
			}
			set
			{
				Text = value;
			}
		}

		public EnExceptionMessageBoxSymbol Symbol
		{
			get
			{
				return symbol;
			}
			set
			{
				symbol = value;
			}
		}

		public EnExceptionMessageBoxButtons Buttons
		{
			get
			{
				return buttons;
			}
			set
			{
				buttons = value;
			}
		}

		public EnExceptionMessageBoxOptions Options
		{
			get
			{
				return options;
			}
			set
			{
				options = value;
			}
		}

		public EnExceptionMessageBoxDialogResult CustomDialogResult => m_customDR;

		public EnExceptionMessageBoxDefaultButton DefaultButton
		{
			get
			{
				return defButton;
			}
			set
			{
				defButton = value;
			}
		}

		public int MessageLevelCount
		{
			get
			{
				return m_messageLimitCount;
			}
			set
			{
				m_messageLimitCount = value;
			}
		}

		public bool ShowHelpButton
		{
			get
			{
				return m_showHelpButton;
			}
			set
			{
				m_showHelpButton = value;
			}
		}

		public Bitmap CustomSymbol
		{
			get
			{
				return m_customSymbol;
			}
			set
			{
				m_customSymbol = value;
			}
		}

		public Icon FormIcon
		{
			get
			{
				return m_formIcon;
			}
			set
			{
				m_formIcon = value;
			}
		}

		public bool ShowCheckBox
		{
			get
			{
				return m_showCheckbox;
			}
			set
			{
				m_showCheckbox = value;
			}
		}

		public bool IsCheckBoxChecked
		{
			get
			{
				return chkDontShow.Checked;
			}
			set
			{
				chkDontShow.Checked = value;
			}
		}

		public string CheckBoxText
		{
			get
			{
				return chkDontShow.Text;
			}
			set
			{
				chkDontShow.Text = value;
			}
		}

		public event CopyToClipboardEventHandler CopyToClipboardInternalEvent;

		public ExceptionMessageBoxDlg()
		{
			InitializeComponent();
			tbBtnHelp.DropDown = m_dropdown;
			toolStrip1.Renderer = new PrivateRenderer();
			Icon = m_formIcon;
		}

		private void AddAdditionalInfoMessage(int messageCount, string strText, Exception ex)
		{
			int count = pnlAdditional.Controls.Count;
			pnlAdditional.SuspendLayout();
			try
			{
				Label label = new()
				{
					Name = "picIndentArrow" + count.ToString(CultureInfo.CurrentCulture),
					TabIndex = count++,
					TabStop = false,
					Visible = true,
					ImageList = imgIcons
				};
				if ((options & EnExceptionMessageBoxOptions.RtlReading) != 0)
				{
					label.ImageIndex = 1;
				}
				else
				{
					label.ImageIndex = 0;
				}

				label.Size = new Size(16, 16);
				label.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				label.Margin = new Padding(0);
				label.AutoSize = false;
				label.AccessibleRole = AccessibleRole.Graphic;
				label.Click += HideBorderLines;
				pnlAdditional.Controls.Add(label, messageCount, messageCount);
				LinkLabel linkLabel = new()
				{
					Name = "txtMessage" + count.ToString(CultureInfo.CurrentCulture),
					AutoSize = true,
					TabIndex = count,
					TabStop = true,
					Text = strText,
					LinkArea = new LinkArea(0, 0),
					AccessibleName = strText,
					Visible = true,
					Margin = new Padding(0, 0, 0, 8),
					MaximumSize = new Size(pnlAdditional.GetPreferredSize(Size.Empty).Width - (messageCount + 1) * 20, 0)
				};
				linkLabel.Click += HideBorderLines;
				linkLabel.UseMnemonic = false;
				linkLabel.Anchor = AnchorStyles.Top | AnchorStyles.Left;
				linkLabel.Tag = ex;
				pnlAdditional.Controls.Add(linkLabel, messageCount + 1, messageCount);
				pnlAdditional.SetColumnSpan(linkLabel, pnlAdditional.ColumnStyles.Count - (messageCount + 1));
			}
			finally
			{
				pnlAdditional.ResumeLayout();
			}
		}

		public void Show(Control owner)
		{
			if (owner == null)
			{
				StartPosition = FormStartPosition.CenterScreen;
				ShowInTaskbar = true;
			}
			else
			{
				StartPosition = FormStartPosition.CenterParent;
				Parent = owner;
				CenterToParent();
			}

			Show();
		}

		public void SetButtonText(string[] value)
		{
			m_buttonTextArray = value;
			if (m_buttonTextArray[0] == null || m_buttonTextArray[0].Length == 0)
			{
				m_buttonCount = 0;
			}
			else if (m_buttonTextArray[1] == null || m_buttonTextArray[1].Length == 0)
			{
				m_buttonCount = 1;
			}
			else if (m_buttonTextArray[2] == null || m_buttonTextArray[2].Length == 0)
			{
				m_buttonCount = 2;
			}
			else if (m_buttonTextArray[3] == null || m_buttonTextArray[3].Length == 0)
			{
				m_buttonCount = 3;
			}
			else if (m_buttonTextArray[4] == null || m_buttonTextArray[4].Length == 0)
			{
				m_buttonCount = 4;
			}
			else
			{
				m_buttonCount = 5;
			}
		}

		public void PrepareToShow()
		{
			if (exMessage == null)
			{
				Exception ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			if (m_buttonTextArray == null || m_buttonTextArray.Length < 5)
			{
				ApplicationException ex = new(ExceptionsResources.CantComplete);
				Diag.Dug(ex);
				throw ex;
			}

			if (isButtonPressed)
			{
				ApplicationException ex = new(ExceptionsResources.CantReuseObject);
				Diag.Dug(ex);
				throw ex;
			}

			if (Caption == null || Caption.Length == 0)
			{
				Caption = exMessage.Source;
			}

			SuspendLayout();
			toolStrip1.Visible = m_showHelpButton;
			int height = Screen.FromControl(pnlMessage).WorkingArea.Height * 3 / 4;
			pnlMessage.MaximumSize = new Size(pnlMessage.MaximumSize.Width, height);
			InitializeCheckbox();
			InitializeMessage();
			InitializeSymbol();
			InitializeButtons();
			if (!HasTechnicalDetails())
			{
				tbBtnAdvanced.Visible = false;
				tbBtnAdvanced.Enabled = false;
			}

			if (m_showHelpButton)
			{
				if (m_helpUrlCount == 0)
				{
					tbBtnHelp.Visible = false;
					tbBtnHelpSingle.Visible = false;
					tbBtnHelp.Enabled = false;
					m_dropdown.Items.Clear();
				}
				else if (m_dropdown.Items.Count == 1 && m_helpUrlCount == 1)
				{
					m_dropdown.Items.Clear();
					tbBtnHelp.Visible = false;
					tbBtnHelpSingle.Visible = true;
				}
			}

			if ((options & EnExceptionMessageBoxOptions.RtlReading) != 0)
			{
				RightToLeft = RightToLeft.Yes;
				RightToLeftLayout = true;
				chkDontShow.Padding = new Padding(chkDontShow.Padding.Right, chkDontShow.Padding.Top, chkDontShow.Padding.Left, chkDontShow.Padding.Bottom);
			}

			ResumeLayout();
		}

		private void InitializeButtons()
		{
			Button[] array = new Button[5] { button1, button2, button3, button4, button5 };
			switch (buttons)
			{
				case EnExceptionMessageBoxButtons.OK:
					m_buttonTextArray[0] = ExceptionsResources.OKButton;
					button1.DialogResult = DialogResult.OK;
					AcceptButton = button1;
					CancelButton = button1;
					m_buttonCount = 1;
					break;
				case EnExceptionMessageBoxButtons.OKCancel:
					m_buttonTextArray[0] = ExceptionsResources.OKButton;
					m_buttonTextArray[1] = ExceptionsResources.CancelButton;
					button1.DialogResult = DialogResult.OK;
					button2.DialogResult = DialogResult.Cancel;
					AcceptButton = button1;
					CancelButton = button2;
					m_buttonCount = 2;
					break;
				case EnExceptionMessageBoxButtons.YesNo:
					m_buttonTextArray[0] = ExceptionsResources.YesButton;
					m_buttonTextArray[1] = ExceptionsResources.NoButton;
					button1.DialogResult = DialogResult.Yes;
					button2.DialogResult = DialogResult.No;
					m_buttonCount = 2;
					ControlBox = false;
					break;
				case EnExceptionMessageBoxButtons.YesNoCancel:
					m_buttonTextArray[0] = ExceptionsResources.YesButton;
					m_buttonTextArray[1] = ExceptionsResources.NoButton;
					m_buttonTextArray[2] = ExceptionsResources.CancelButton;
					button1.DialogResult = DialogResult.Yes;
					button2.DialogResult = DialogResult.No;
					button3.DialogResult = DialogResult.Cancel;
					m_buttonCount = 3;
					CancelButton = button3;
					break;
				case EnExceptionMessageBoxButtons.AbortRetryIgnore:
					m_buttonTextArray[0] = ExceptionsResources.AbortButton;
					m_buttonTextArray[1] = ExceptionsResources.RetryButton;
					m_buttonTextArray[2] = ExceptionsResources.IgnoreButton;
					button1.DialogResult = DialogResult.Abort;
					button2.DialogResult = DialogResult.Retry;
					button3.DialogResult = DialogResult.Ignore;
					m_buttonCount = 3;
					ControlBox = false;
					break;
				case EnExceptionMessageBoxButtons.RetryCancel:
					m_buttonTextArray[0] = ExceptionsResources.RetryButton;
					m_buttonTextArray[1] = ExceptionsResources.CancelButton;
					button1.DialogResult = DialogResult.Retry;
					button2.DialogResult = DialogResult.Cancel;
					CancelButton = button2;
					m_buttonCount = 2;
					break;
				case EnExceptionMessageBoxButtons.Custom:
					ControlBox = false;
					break;
			}

			int width = pnlButtons.GetPreferredSize(Size.Empty).Width;
			for (int i = 0; i < m_buttonCount; i++)
			{
				Button obj = array[i];
				obj.Text = m_buttonTextArray[i];
				obj.Visible = true;
			}

			AdjustDialogWidth(pnlButtons.GetPreferredSize(Size.Empty).Width - width, isAdjustingForButtons: true);
			if ((int)defButton >= m_buttonCount)
			{
				InvalidEnumArgumentException ex = new("DefaultButton", (int)defButton, typeof(EnExceptionMessageBoxDefaultButton));
				Diag.Dug(ex);
				throw ex;
			}

			AcceptButton = array[(int)defButton];
		}

		private void InitializeSymbol()
		{
			if (m_customSymbol != null)
			{
				int num = m_customSymbol.Width + 2 - pnlIcon.Width;
				pnlIcon.Width += num;
				AdjustDialogWidth(num, isAdjustingForButtons: false);
				pnlIcon.Height = m_customSymbol.Height + 2;
				pnlIcon.MinimumSize = new Size(0, m_customSymbol.Height + 2);
				return;
			}

			switch (symbol)
			{
				case EnExceptionMessageBoxSymbol.None:
					pnlIcon.Visible = false;
					break;
				case EnExceptionMessageBoxSymbol.Warning:
				case EnExceptionMessageBoxSymbol.Exclamation:
					m_iconSymbol = SystemIcons.Warning;
					break;
				case EnExceptionMessageBoxSymbol.Information:
				case EnExceptionMessageBoxSymbol.Asterisk:
					m_iconSymbol = SystemIcons.Information;
					break;
				case EnExceptionMessageBoxSymbol.Error:
				case EnExceptionMessageBoxSymbol.Hand:
					m_iconSymbol = SystemIcons.Error;
					break;
				case EnExceptionMessageBoxSymbol.Question:
					m_iconSymbol = SystemIcons.Question;
					break;
			}
		}

		private void AdjustDialogWidth(int offset, bool isAdjustingForButtons)
		{
			if (offset <= 0)
			{
				return;
			}

			lblTopMessage.MaximumSize = new Size(lblTopMessage.MaximumSize.Width + offset, lblTopMessage.MaximumSize.Height);
			lblTopMessage.MinimumSize = lblTopMessage.MaximumSize;
			pnlAdditional.MaximumSize = new Size(pnlAdditional.MaximumSize.Width + offset, pnlAdditional.MaximumSize.Height);
			foreach (Label control in pnlAdditional.Controls)
			{
				if (control.ImageIndex < 0)
				{
					control.MaximumSize = new Size(control.MaximumSize.Width + offset, control.MaximumSize.Height);
				}
			}

			if (!isAdjustingForButtons)
			{
				pnlButtons.MinimumSize = new Size(pnlButtons.MinimumSize.Width + offset, pnlButtons.MinimumSize.Height);
			}
		}

		private void InitializeCheckbox()
		{
			if (!m_showCheckbox)
			{
				chkDontShow.Visible = false;
				chkDontShow.Enabled = false;
			}
			else if (chkDontShow.Text.Length == 0)
			{
				chkDontShow.Text = ExceptionsResources.DefaultCheckboxText;
			}
		}

		private void ShowBorderLines(int index)
		{
			int num = 1;
			_ = pnlMessage;
			if (index == 0)
			{
				lblTopMessage.BackColor = ControlPaint.Light(SystemColors.ControlDark, 0.5f);
			}
			else
			{
				lblTopMessage.BackColor = SystemColors.Control;
			}

			if (pnlAdditional == null)
			{
				return;
			}

			foreach (Label control in pnlAdditional.Controls)
			{
				if (control.ImageIndex < 0)
				{
					if (num == index)
					{
						control.BackColor = ControlPaint.Light(SystemColors.ControlDark, 0.5f);
					}
					else
					{
						control.BackColor = SystemColors.Control;
					}

					num++;
				}
			}
		}

		private void InitializeMessage()
		{
			int num = 0;
			int num2 = 0;
			m_dropdown.Items.Clear();
			for (Exception innerException = exMessage.InnerException; innerException != null; innerException = innerException.InnerException)
			{
				num2++;
			}

			if (m_messageLimitCount > 0 && num2 > m_messageLimitCount - 1)
			{
				num2 = m_messageLimitCount - 1;
			}

			if (num2 > 0)
			{
				for (int i = 0; i < num2; i++)
				{
					pnlAdditional.ColumnStyles.Insert(0, new ColumnStyle(SizeType.Absolute, 20f));
					pnlAdditional.RowStyles.Add(new RowStyle(SizeType.AutoSize));
				}

				pnlAdditional.ColumnCount = num2 + 1;
				pnlAdditional.RowCount = num2;
			}

			Label label = lblAdditionalInfo;
			bool visible = pnlAdditional.Visible = num2 > 0;
			label.Visible = visible;
			Exception innerException2 = exMessage;
			while (innerException2 != null && (m_messageLimitCount < 0 || num < m_messageLimitCount))
			{
				StringBuilder stringBuilder = new StringBuilder();
				string text = innerException2.Message != null && innerException2.Message.Length != 0 ? innerException2.Message : ExceptionsResources.CantComplete;
				if (m_showHelpButton)
				{
					string text2 = BuildHelpURL(innerException2);
					bool num3 = text2.Length > 0;
					m_helpUrlArray.Add(text2);
					text2 = text.Length <= 50 ? text : string.Format(CultureInfo.CurrentCulture, ExceptionsResources.AddEllipsis, text[..50]);
					ToolStripItem toolStripItem;
					if (num3)
					{
						toolStripItem = m_dropdown.Items.Add(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.HelpMenuText, text2), null, ItemHelp_Click);
						m_helpUrlCount++;
					}
					else
					{
						toolStripItem = m_dropdown.Items.Add(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.NoHelpMenuText, text2), null, ItemHelp_Click);
						toolStripItem.Enabled = false;
					}

					toolStripItem.Tag = num;
					toolStripItem.MouseEnter += OnHelpButtonMouseEnter;
					toolStripItem.MouseLeave += OnHelpButtonMouseLeave;
				}

				stringBuilder.Remove(0, stringBuilder.Length);
				stringBuilder.Append(text);
				int num4 = 0;
				if (innerException2 is FbException ex)
				{
					num4 = ex.ErrorCode;
				}

				if (innerException2.Source != null && innerException2.Source.Length > 0 && (num != 0 || Caption != innerException2.Source))
				{
					stringBuilder.Append(' ');
					string arg = !(innerException2.GetType() == typeof(FbException)) && !(innerException2.GetType() == typeof(FbException)) ? innerException2.Source : ExceptionsResources.SqlServerSource;
					if (num4 > 0)
					{
						stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ErrorSourceNumber, arg, num4));
					}
					else
					{
						stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ErrorSource, arg));
					}
				}
				else if (num4 > 0)
				{
					stringBuilder.Append(' ');
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ErrorNumber, num4));
				}

				if (num == 0)
				{
					lblTopMessage.Text = stringBuilder.ToString();
					lblTopMessage.LinkArea = new LinkArea(0, 0);
					lblTopMessage.Tag = innerException2;
				}
				else
				{
					AddAdditionalInfoMessage(num - 1, stringBuilder.ToString(), innerException2);
				}

				innerException2 = innerException2.InnerException;
				num++;
			}

			if (Location.Y + GetPreferredSize(Size.Empty).Height > Screen.PrimaryScreen.WorkingArea.Bottom)
			{
				Location = new Point(Location.X, Screen.PrimaryScreen.WorkingArea.Bottom - Size.Height - 10);
			}
		}

		private bool HasTechnicalDetails()
		{
			for (Exception innerException = exMessage; innerException != null; innerException = innerException.InnerException)
			{
				if (innerException.StackTrace != null && innerException.StackTrace.Length > 0 || innerException.GetType() == typeof(FbException) || innerException.GetType() == typeof(FbException))
				{
					return true;
				}

				foreach (DictionaryEntry datum in innerException.Data)
				{
					if (string.Compare((string)datum.Key, 0, C_AdvancedKey, 0, C_AdvancedKey.Length, ignoreCase: false, CultureInfo.CurrentCulture) == 0)
					{
						return true;
					}
				}
			}

			return false;
		}

		private string BuildTechnicalDetails(Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (ex.GetType() == typeof(FbException) || ex.GetType() == typeof(FbException))
			{
				FbException ex2 = (FbException)ex;
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append("---------------");
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(ExceptionsResources.SqlServerInfo);
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(Environment.NewLine);
				
				if (ex2.GetServer() != null && ex2.GetServer().Length > 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlServerName, ex2.GetServer()));
					stringBuilder.Append(Environment.NewLine);
				}
				
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlError, ex2.GetNumber().ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlSeverity, ex2.GetClass().ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlState, ex2.SQLSTATE.ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);

				if (ex2.GetProcedure() != null && ex2.GetProcedure().Length > 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlProcedure, ex2.GetProcedure()));
					stringBuilder.Append(Environment.NewLine);
				}

				if (ex2.GetLineNumber() != 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlLineNumber, ex2.GetLineNumber().ToString(CultureInfo.CurrentCulture)));
					stringBuilder.Append(Environment.NewLine);
				}
			}

			if (ex.StackTrace != null && ex.StackTrace.Length > 0)
			{
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append("---------------");
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(ExceptionsResources.CodeLocation);
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(ex.StackTrace);
				stringBuilder.Append(Environment.NewLine);
			}

			return stringBuilder.ToString();
		}

		private void NewMessageBoxForm_Load(object sender, EventArgs e)
		{
			if (Parent == null)
			{
				StartPosition = FormStartPosition.CenterScreen;
			}
			else
			{
				StartPosition = FormStartPosition.CenterParent;
				CenterToParent();
			}

			if (m_doBeep)
			{
				Beep();
			}

			switch (defButton)
			{
				case EnExceptionMessageBoxDefaultButton.Button1:
					button1.Focus();
					ActiveControl = button1;
					break;
				case EnExceptionMessageBoxDefaultButton.Button2:
					button2.Focus();
					ActiveControl = button2;
					break;
				case EnExceptionMessageBoxDefaultButton.Button3:
					button3.Focus();
					ActiveControl = button3;
					break;
				case EnExceptionMessageBoxDefaultButton.Button4:
					button4.Focus();
					ActiveControl = button4;
					break;
				case EnExceptionMessageBoxDefaultButton.Button5:
					button5.Focus();
					ActiveControl = button5;
					break;
			}
		}

		public string BuildHelpURL(Exception ex)
		{
			int num = 0;
			string text = null;
			StringBuilder stringBuilder = new StringBuilder("?");
			if (ex == null)
			{
				return string.Empty;
			}

			if (ex.HelpLink != null && ex.HelpLink.Length > 0)
			{
				if (string.Compare(ex.HelpLink, 0, C_HttpPrefixKey, 0, C_HttpPrefixKey.Length, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
				{
					return ex.HelpLink;
				}

				return string.Empty;
			}

			MethodInfo method = ex.GetType().GetMethod("get_Data");
			if (method == null || method.ReturnType != typeof(IDictionary))
			{
				/*
				if (ex is FbException)
				{
					return string.Format(CultureInfo.CurrentCulture, "https://www.microsoft.com/products/ee/transform.aspx?ProdName=Microsoft%20SQL%20Server&ProdVer={0}.00.0000.00&EvtSrc=MSSQLServer&EvtID={1}", AssemblyVersionInfo.HighestSqlMajorVersionString, Uri.EscapeUriString(((FbException)ex).Number.ToString(CultureInfo.CurrentCulture)));
				}
				*/
				return string.Empty;
			}

			try
			{
				if (ex.Data == null || ex.Data.Count == 0)
				{
					return string.Empty;
				}

				foreach (DictionaryEntry datum in ex.Data)
				{
					if (datum.Key == null || datum.Key.GetType() != typeof(string) || datum.Value == null || datum.Value.GetType() != typeof(string))
					{
						continue;
					}

					if (string.Compare((string)datum.Key, C_HelpUrlBaseKey, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
					{
						text = datum.Value.ToString();
						if (string.Compare(text, 0, C_HttpPrefixKey, 0, C_HttpPrefixKey.Length, ignoreCase: true, CultureInfo.CurrentCulture) != 0)
						{
							text = string.Empty;
						}
					}
					else if (string.Compare((string)datum.Key, 0, C_PrefixKey, 0, C_PrefixKey.Length, ignoreCase: true, CultureInfo.CurrentCulture) == 0)
					{
						if (num++ > 0)
						{
							stringBuilder.Append('&');
						}

						stringBuilder.Append(Uri.EscapeUriString(((string)datum.Key)[C_PrefixKey.Length..]));
						if (datum.Value != null && datum.Value.ToString().Length > 0)
						{
							stringBuilder.Append('=');
							stringBuilder.Append(Uri.EscapeUriString(datum.Value.ToString()));
						}
					}
				}

				if (text == null && num == 0)
				{
					return string.Empty;
				}

				if (num == 0)
				{
					return text;
				}

				return text + stringBuilder.ToString();
			}
			catch (Exception)
			{
			}

			return string.Empty;
		}

		public string BuildMessageText(bool isForEmail, bool isInternal)
		{
			if (exMessage == null)
			{
				return string.Empty;
			}

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("------------------------------");
			stringBuilder.Append(Environment.NewLine);
			string value = stringBuilder.ToString();
			bool flag = exMessage.InnerException != null;
			int num = 1;
			StringBuilder stringBuilder2 = new StringBuilder();
			if (isInternal)
			{
				stringBuilder2.Append(ExceptionsResources.MessageTitle);
				stringBuilder2.Append(Caption);
			}

			for (Exception innerException = exMessage; innerException != null; innerException = innerException.InnerException)
			{
				if (isInternal || num > 1)
				{
					stringBuilder2.Append(value);
				}

				if (flag && num == 2)
				{
					stringBuilder2.Append(ExceptionsResources.AdditionalInfo);
					stringBuilder2.Append(Environment.NewLine);
				}

				if (isInternal || num > 1)
				{
					stringBuilder2.Append(Environment.NewLine);
				}

				if (innerException.Message == null || innerException.Message.Length == 0)
				{
					stringBuilder2.Append(ExceptionsResources.CantComplete);
				}
				else
				{
					stringBuilder2.Append(innerException.Message);
				}

				if (innerException.Source != null && innerException.Source.Length > 0 && (num != 1 || Caption != innerException.Source))
				{
					stringBuilder2.Append(' ');
					if (innerException.GetType() == typeof(FbException) || innerException.GetType() == typeof(FbException))
					{
						stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ErrorSourceNumber, ExceptionsResources.SqlServerSource, ((FbException)innerException).GetNumber()));
					}
					else
					{
						stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ErrorSource, innerException.Source));
					}
				}

				stringBuilder2.Append(Environment.NewLine);
				string text = BuildHelpURL(innerException);
				if (text.Length > 0)
				{
					stringBuilder2.Append(Environment.NewLine);
					stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ClipboardOrEmailHelpLink, text));
					stringBuilder2.Append(Environment.NewLine);
				}

				num++;
			}

			if (isInternal)
			{
				stringBuilder2.Append(value);
				stringBuilder2.Append(ExceptionsResources.Buttons);
				stringBuilder2.Append(Environment.NewLine);
				for (int i = 0; i < m_buttonCount; i++)
				{
					stringBuilder2.Append(Environment.NewLine);
					stringBuilder2.Append(m_buttonTextArray[i]);
				}

				stringBuilder2.Append(value);
			}

			return stringBuilder2.ToString();
		}

		public string BuildAdvancedInfo(Exception ex, EnAdvancedInfoType type)
		{
			StringBuilder stringBuilder = new StringBuilder();
			if (ex == null)
			{
				ex = new ArgumentNullException("ex");
				Diag.Dug(ex);
				throw ex;
			}

			if (type == EnAdvancedInfoType.All)
			{
				stringBuilder.Append("===================================");
				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(Environment.NewLine);
				}
			}

			if (ex.Message != null && ex.Message.Length != 0 && (type == EnAdvancedInfoType.All || type == EnAdvancedInfoType.Message))
			{
				stringBuilder.Append(ex.Message);
				if (ex.Source != null && ex.Source.Length > 0)
				{
					stringBuilder.AppendFormat(" ({0})", ex.Source);
				}

				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(Environment.NewLine);
				}
			}

			string text = BuildHelpURL(ex);
			if (text.Length > 0)
			{
				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append("------------------------------");
					stringBuilder.Append(Environment.NewLine);
				}

				if (type == EnAdvancedInfoType.All || type == EnAdvancedInfoType.HelpLink)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ClipboardOrEmailHelpLink, text));
				}

				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(Environment.NewLine);
				}
			}

			string text2 = BuildAdvancedInfoProperties(ex);
			if (text2 != null && text2.Length > 0)
			{
				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append("------------------------------");
					stringBuilder.Append(Environment.NewLine);
				}

				if (type == EnAdvancedInfoType.All || type == EnAdvancedInfoType.Data)
				{
					stringBuilder.Append(text2);
				}

				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(Environment.NewLine);
				}
			}

			if (ex.StackTrace != null && ex.StackTrace.Length > 0)
			{
				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append("------------------------------");
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(ExceptionsResources.CodeLocation);
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(Environment.NewLine);
				}

				if (type == EnAdvancedInfoType.All || type == EnAdvancedInfoType.StackTrace)
				{
					stringBuilder.Append(ex.StackTrace);
				}

				if (type == EnAdvancedInfoType.All)
				{
					stringBuilder.Append(Environment.NewLine);
					stringBuilder.Append(Environment.NewLine);
				}
			}

			return stringBuilder.ToString();
		}

		private string BuildAdvancedInfoProperties(Exception ex)
		{
			StringBuilder stringBuilder = new StringBuilder();
			bool flag = false;
			if (ex.GetType() == typeof(FbException) || ex.GetType() == typeof(FbException))
			{
				FbException ex2 = (FbException)ex;
				if (ex2.GetServer() != null && ex2.GetServer().Length > 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlServerName, ex2.GetServer()));
					stringBuilder.Append(Environment.NewLine);
				}

				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlError, ex2.GetNumber().ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlSeverity, ex2.GetClass().ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlState, ex2.GetState().ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);
				if (ex2.GetProcedure() != null && ex2.GetProcedure().Length > 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlProcedure, ex2.GetProcedure()));
					stringBuilder.Append(Environment.NewLine);
				}

				if (ex2.GetLineNumber() != 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.SqlLineNumber, ex2.GetLineNumber().ToString(CultureInfo.CurrentCulture)));
					stringBuilder.Append(Environment.NewLine);
				}

				flag = true;
			}

			foreach (DictionaryEntry datum in ex.Data)
			{
				if (datum.Key != null && !(datum.Key.GetType() != typeof(string)) && datum.Value != null && string.Compare((string)datum.Key, 0, C_AdvancedKey, 0, C_AdvancedKey.Length, ignoreCase: false, CultureInfo.CurrentCulture) == 0 && datum.Value != null && datum.Value.ToString().Length > 0)
				{
					if (flag)
					{
						stringBuilder.Append(Environment.NewLine);
						flag = false;
					}

					stringBuilder.AppendFormat("{0} = {1}", ((string)datum.Key)[C_AdvancedKey.Length..], datum.Value.ToString());
					stringBuilder.Append(Environment.NewLine);
				}
			}

			return stringBuilder.ToString();
		}

		public string BuildAdvancedInfo()
		{
			if (exMessage == null)
			{
				return string.Empty;
			}

			StringBuilder stringBuilder = new StringBuilder();
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("------------------------------");
			stringBuilder.Append(Environment.NewLine);
			string value = stringBuilder.ToString();
			bool flag = exMessage.InnerException != null;
			int num = 1;
			StringBuilder stringBuilder2 = new StringBuilder();
			stringBuilder2.Append(ExceptionsResources.MessageTitle);
			stringBuilder2.Append(Caption);
			for (Exception innerException = exMessage; innerException != null; innerException = innerException.InnerException)
			{
				stringBuilder2.Append(value);
				if (flag && num == 2)
				{
					stringBuilder2.Append(ExceptionsResources.AdditionalInfo);
					stringBuilder2.Append(Environment.NewLine);
				}

				stringBuilder2.Append(Environment.NewLine);
				if (innerException.Message == null || innerException.Message.Length == 0)
				{
					stringBuilder2.Append(ExceptionsResources.CantComplete);
				}
				else
				{
					stringBuilder2.Append(innerException.Message);
				}

				if (innerException.Source != null && innerException.Source.Length > 0 && (num != 1 || Caption != innerException.Source))
				{
					stringBuilder2.AppendFormat(" ({0})", innerException.Source);
				}

				stringBuilder2.Append(Environment.NewLine);
				string text = BuildHelpURL(innerException);
				if (text.Length > 0)
				{
					stringBuilder2.Append(Environment.NewLine);
					stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.ClipboardOrEmailHelpLink, text));
					stringBuilder2.Append(Environment.NewLine);
				}

				stringBuilder2.Append(BuildTechnicalDetails(innerException));
				num++;
			}

			return stringBuilder2.ToString();
		}

		private void CopyToClipboard()
		{
			string text = BuildMessageText(isForEmail: false, isInternal: true);
			if (CopyToClipboardInternalEvent != null)
			{
				CopyToClipboardEventArgs args = new (text);
				CopyToClipboardInternalEvent(this, args);

				if (args.EventHandled)
					return;
			}

			try
			{
				Clipboard.SetDataObject(text, copy: true);
			}
			catch (Exception)
			{
				try
				{
					Clipboard.SetDataObject(text, copy: true);
				}
				catch (Exception exError)
				{
					ShowError(ExceptionsResources.CopyToClipboardError, exError);
				}
			}
		}

		private void Btn_Click(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
			if (buttons == EnExceptionMessageBoxButtons.Custom)
			{
				if (sender == button1)
				{
					m_customDR = EnExceptionMessageBoxDialogResult.Button1;
				}
				else if (sender == button2)
				{
					m_customDR = EnExceptionMessageBoxDialogResult.Button2;
				}
				else if (sender == button3)
				{
					m_customDR = EnExceptionMessageBoxDialogResult.Button3;
				}
				else if (sender == button4)
				{
					m_customDR = EnExceptionMessageBoxDialogResult.Button4;
				}
				else
				{
					m_customDR = EnExceptionMessageBoxDialogResult.Button5;
				}
			}
			else
			{
				m_customDR = EnExceptionMessageBoxDialogResult.None;
			}

			isButtonPressed = true;
			Close();
		}

		private void GetHelp(int index)
		{
			if (m_helpUrlCount == 1)
			{
				index = 0;
				while (index < m_helpUrlArray.Count && ((string)m_helpUrlArray[index]).Length <= 0)
				{
					index++;
				}
			}

			if (index >= m_helpUrlArray.Count || ((string)m_helpUrlArray[index]).Length == 0)
			{
				return;
			}

			string text = (string)m_helpUrlArray[index];
			try
			{
				DialogResult dialogResult;
				using (PrivacyConfirmationDlg privacyConfirmation = new PrivacyConfirmationDlg(Text, text))
				{
					if (Parent == null)
					{
						privacyConfirmation.ShowInTaskbar = true;
						privacyConfirmation.StartPosition = FormStartPosition.CenterScreen;
						dialogResult = privacyConfirmation.ShowDialog(this);
					}
					else
					{
						privacyConfirmation.StartPosition = FormStartPosition.CenterParent;
						dialogResult = privacyConfirmation.ShowDialog(Parent);
					}
				}

				if (dialogResult == DialogResult.No)
				{
					return;
				}
			}
			catch (Exception)
			{
				ShowError(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.CantStartHelpLink, text), null);
				return;
			}

			try
			{
				Process process = new Process();
				process.StartInfo.FileName = text;
				process.Start();
			}
			catch (Exception exError)
			{
				ShowError(string.Format(CultureInfo.CurrentCulture, ExceptionsResources.CantStartHelpLink, text), exError);
			}
		}

		private void ItemCopy_Click(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
			CopyToClipboard();
		}

		public void ShowError(string str, Exception exError)
		{
			ExceptionMessageBoxCtl exceptionMessageBox = new(new Exception(str, exError)
			{
				Source = Text
			})
			{
				Options = options
			};
			exceptionMessageBox.Show(Parent);
		}

		private void NewMessageBoxForm_Closing(object sender, CancelEventArgs e)
		{
			switch (buttons)
			{
				case EnExceptionMessageBoxButtons.OK:
					DialogResult = DialogResult.OK;
					break;
				case EnExceptionMessageBoxButtons.YesNo:
				case EnExceptionMessageBoxButtons.AbortRetryIgnore:
				case EnExceptionMessageBoxButtons.Custom:
					if (!isButtonPressed)
					{
						e.Cancel = true;
						MessageBeep(BeepType.Hand);
					}

					break;
				case EnExceptionMessageBoxButtons.OKCancel:
				case EnExceptionMessageBoxButtons.YesNoCancel:
				case EnExceptionMessageBoxButtons.RetryCancel:
					break;
			}
		}

		private void ShowDetails()
		{
			try
			{
				using AdvancedInformationDlg advancedInformation = new AdvancedInformationDlg();
				advancedInformation.MessageBoxForm = this;
				if ((options & EnExceptionMessageBoxOptions.RtlReading) != 0)
				{
					advancedInformation.RightToLeft = RightToLeft.Yes;
					advancedInformation.RightToLeftLayout = true;
				}

				if (Parent == null)
				{
					advancedInformation.ShowInTaskbar = true;
					advancedInformation.StartPosition = FormStartPosition.CenterScreen;
					advancedInformation.ShowDialog(this);
				}
				else
				{
					advancedInformation.StartPosition = FormStartPosition.CenterParent;
					advancedInformation.ShowDialog(Parent);
				}
			}
			catch (Exception exError)
			{
				ShowError(ExceptionsResources.CantShowTechnicalDetailsError, exError);
			}
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		public static extern bool MessageBeep(BeepType type);

		private void Beep()
		{
			switch (symbol)
			{
				case EnExceptionMessageBoxSymbol.None:
				case EnExceptionMessageBoxSymbol.Warning:
					MessageBeep(BeepType.Asterisk);
					break;
				case EnExceptionMessageBoxSymbol.Information:
					MessageBeep(BeepType.Exclamation);
					break;
				case EnExceptionMessageBoxSymbol.Error:
					MessageBeep(BeepType.Hand);
					break;
			}
		}

		private void ItemShowDetails_Click(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
			ShowDetails();
		}

		[SecurityPermission(SecurityAction.LinkDemand, UnmanagedCode = true)]
		protected override void WndProc(ref Message m)
		{
			if (m.Msg == C_WM_GETDLGID)
			{
				m.Result = (IntPtr)10007;
			}
			else
			{
				base.WndProc(ref m);
			}
		}

		private void NewMessageBoxForm_KeyDown(object sender, KeyEventArgs e)
		{
			if (e.Modifiers == Keys.Control && (e.KeyData & Keys.C) == Keys.C || (e.KeyData & Keys.Insert) == Keys.Insert)
			{
				CopyToClipboard();
				e.Handled = true;
			}
		}

		private void PnlIcon_Paint(object sender, PaintEventArgs e)
		{
			if (m_customSymbol != null)
			{
				e.Graphics.DrawImageUnscaled(m_customSymbol, new Point(0, 0));
			}
			else if (m_iconSymbol != null)
			{
				e.Graphics.DrawIcon(m_iconSymbol, 0, 0);
			}
		}

		private void ItemHelp_Click(object sender, EventArgs e)
		{
			ToolStripItem toolStripItem = sender as ToolStripItem;
			ShowBorderLines(-1);
			GetHelp((int)toolStripItem.Tag & 0xFFF);
		}

		private void NewMessageBoxForm_Click(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
		}

		private void HideBorderLines(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
		}

		private void NewMessageBoxForm_HelpRequested(object sender, HelpEventArgs hlpevent)
		{
			if (m_showHelpButton)
			{
				if (m_dropdown.Items.Count == 0 && m_helpUrlCount == 1)
				{
					GetHelp(0);
				}

				hlpevent.Handled = true;
			}
		}

		private void OnHelpButtonMouseEnter(object sender, EventArgs e)
		{
			ShowBorderLines((int)((ToolStripItem)sender).Tag);
		}

		private void OnHelpButtonMouseLeave(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
		}

		private void TbBtnHelp_Click(object sender, EventArgs e)
		{
			ShowBorderLines(-1);
			if (m_dropdown.Items.Count == 0 && m_helpUrlCount == 1)
			{
				GetHelp(0);
			}
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && components != null)
			{
				components.Dispose();
			}

			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			components = new Container();
			ComponentResourceManager resources = new ComponentResourceManager(typeof(BlackbirdSql.Common.Exceptions.ExceptionMessageBoxDlg));
			pnlForm = new TableLayoutPanel();
			pnlIcon = new Panel();
			pnlMessage = new TableLayoutPanel();
			lblTopMessage = new LinkLabel();
			lblAdditionalInfo = new Label();
			pnlAdditional = new TableLayoutPanel();
			grpSeparator = new GroupBox();
			pnlButtons = new TableLayoutPanel();
			toolStrip1 = new ToolStrip();
			tbBtnHelp = new ToolStripDropDownButton();
			tbBtnHelpSingle = new ToolStripButton();
			tbBtnCopy = new ToolStripButton();
			tbBtnAdvanced = new ToolStripButton();
			button1 = new Button();
			button2 = new Button();
			button3 = new Button();
			button4 = new Button();
			button5 = new Button();
			imgIcons = new ImageList(components);
			chkDontShow = new BlackbirdSql.Common.Controls.WrappingCheckBox();
			pnlForm.SuspendLayout();
			pnlMessage.SuspendLayout();
			pnlButtons.SuspendLayout();
			toolStrip1.SuspendLayout();
			SuspendLayout();
			resources.ApplyResources(pnlForm, "pnlForm");
			pnlForm.Controls.Add(pnlIcon, 0, 0);
			pnlForm.Controls.Add(pnlMessage, 1, 0);
			pnlForm.Controls.Add(chkDontShow, 1, 1);
			pnlForm.Controls.Add(grpSeparator, 0, 2);
			pnlForm.Controls.Add(pnlButtons, 0, 3);
			pnlForm.Name = "pnlForm";
			pnlForm.Click += new EventHandler(HideBorderLines);
			resources.ApplyResources(pnlIcon, "pnlIcon");
			pnlIcon.Name = "pnlIcon";
			pnlIcon.Click += new EventHandler(HideBorderLines);
			pnlIcon.Paint += new PaintEventHandler(PnlIcon_Paint);
			resources.ApplyResources(pnlMessage, "pnlMessage");
			pnlMessage.Controls.Add(lblTopMessage, 0, 0);
			pnlMessage.Controls.Add(lblAdditionalInfo, 0, 1);
			pnlMessage.Controls.Add(pnlAdditional, 0, 2);
			pnlMessage.Name = "pnlMessage";
			pnlMessage.Click += new EventHandler(HideBorderLines);
			resources.ApplyResources(lblTopMessage, "lblTopMessage");
			lblTopMessage.Name = "lblTopMessage";
			lblTopMessage.Click += new EventHandler(HideBorderLines);
			resources.ApplyResources(lblAdditionalInfo, "lblAdditionalInfo");
			lblAdditionalInfo.Name = "lblAdditionalInfo";
			lblAdditionalInfo.Click += new EventHandler(HideBorderLines);
			resources.ApplyResources(pnlAdditional, "pnlAdditional");
			pnlAdditional.Name = "pnlAdditional";
			pnlAdditional.Click += new EventHandler(HideBorderLines);
			resources.ApplyResources(grpSeparator, "grpSeparator");
			pnlForm.SetColumnSpan(grpSeparator, 2);
			grpSeparator.Name = "grpSeparator";
			grpSeparator.TabStop = false;
			resources.ApplyResources(pnlButtons, "pnlButtons");
			pnlForm.SetColumnSpan(pnlButtons, 2);
			pnlButtons.Controls.Add(toolStrip1, 0, 0);
			pnlButtons.Controls.Add(button1, 1, 0);
			pnlButtons.Controls.Add(button2, 2, 0);
			pnlButtons.Controls.Add(button3, 3, 0);
			pnlButtons.Controls.Add(button4, 4, 0);
			pnlButtons.Controls.Add(button5, 5, 0);
			pnlButtons.Name = "pnlButtons";
			pnlButtons.Click += new EventHandler(HideBorderLines);
			toolStrip1.AllowMerge = false;
			toolStrip1.CanOverflow = false;
			resources.ApplyResources(toolStrip1, "toolStrip1");
			toolStrip1.GripStyle = ToolStripGripStyle.Hidden;
			toolStrip1.Items.AddRange(new ToolStripItem[4] { tbBtnHelp, tbBtnHelpSingle, tbBtnCopy, tbBtnAdvanced });
			toolStrip1.LayoutStyle = ToolStripLayoutStyle.HorizontalStackWithOverflow;
			toolStrip1.Name = "toolStrip1";
			toolStrip1.RenderMode = ToolStripRenderMode.System;
			toolStrip1.TabStop = true;
			tbBtnHelp.DisplayStyle = ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(tbBtnHelp, "tbBtnHelp");
			tbBtnHelp.Name = "tbBtnHelp";
			tbBtnHelp.Click += new EventHandler(TbBtnHelp_Click);
			tbBtnHelpSingle.DisplayStyle = ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(tbBtnHelpSingle, "tbBtnHelpSingle");
			tbBtnHelpSingle.Margin = new Padding(1, 1, 0, 2);
			tbBtnHelpSingle.Name = "tbBtnHelpSingle";
			tbBtnHelpSingle.Click += new EventHandler(TbBtnHelp_Click);
			tbBtnCopy.DisplayStyle = ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(tbBtnCopy, "tbBtnCopy");
			tbBtnCopy.Margin = new Padding(1, 1, 0, 2);
			tbBtnCopy.Name = "tbBtnCopy";
			tbBtnCopy.Click += new EventHandler(ItemCopy_Click);
			tbBtnAdvanced.DisplayStyle = ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(tbBtnAdvanced, "tbBtnAdvanced");
			tbBtnAdvanced.Margin = new Padding(1, 1, 0, 2);
			tbBtnAdvanced.Name = "tbBtnAdvanced";
			tbBtnAdvanced.Click += new EventHandler(ItemShowDetails_Click);
			resources.ApplyResources(button1, "button1");
			button1.Name = "button1";
			button1.Click += new EventHandler(Btn_Click);
			resources.ApplyResources(button2, "button2");
			button2.Name = "button2";
			button2.Click += new EventHandler(Btn_Click);
			resources.ApplyResources(button3, "button3");
			button3.Name = "button3";
			button3.Click += new EventHandler(Btn_Click);
			resources.ApplyResources(button4, "button4");
			button4.Name = "button4";
			button4.Tag = "z";
			button4.Click += new EventHandler(Btn_Click);
			resources.ApplyResources(button5, "button5");
			button5.Name = "button5";
			button5.Click += new EventHandler(Btn_Click);
			imgIcons.ImageStream = (ImageListStreamer)resources.GetObject("imgIcons.ImageStream");
			imgIcons.TransparentColor = Color.Transparent;
			imgIcons.Images.SetKeyName(0, "indentarrow.bmp");
			imgIcons.Images.SetKeyName(1, "indentarrow_right.bmp");
			resources.ApplyResources(chkDontShow, "chkDontShow");
			chkDontShow.Name = "chkDontShow";
			chkDontShow.Click += new EventHandler(HideBorderLines);
			AccessibleRole = AccessibleRole.Alert;
			resources.ApplyResources(this, "$this");
			AutoScaleMode = AutoScaleMode.Font;
			Controls.Add(pnlForm);
			DoubleBuffered = true;
			FormBorderStyle = FormBorderStyle.FixedDialog;
			KeyPreview = true;
			MaximizeBox = false;
			MinimizeBox = false;
			Name = "ExceptionMessageBoxForm";
			ShowInTaskbar = false;
			Load += new EventHandler(NewMessageBoxForm_Load);
			Click += new EventHandler(NewMessageBoxForm_Click);
			HelpRequested += new HelpEventHandler(NewMessageBoxForm_HelpRequested);
			KeyDown += new KeyEventHandler(NewMessageBoxForm_KeyDown);
			pnlForm.ResumeLayout(false);
			pnlForm.PerformLayout();
			pnlMessage.ResumeLayout(false);
			pnlMessage.PerformLayout();
			pnlButtons.ResumeLayout(false);
			pnlButtons.PerformLayout();
			toolStrip1.ResumeLayout(false);
			toolStrip1.PerformLayout();
			ResumeLayout(false);
			PerformLayout();
		}
	}
}
