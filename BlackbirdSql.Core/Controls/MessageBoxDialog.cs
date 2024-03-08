// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.ExceptionMessageBoxForm

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
using BlackbirdSql.Core.Controls.Enums;
using BlackbirdSql.Core.Controls.Widgets;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Properties;
using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Core.Controls;

public sealed class MessageBoxDialog : Form
{
	public enum EnBeepType
	{
		Standard = -1,
		Default = 0,
		Hand = 0x10,
		Question = 0x20,
		Exclamation = 48,
		Asterisk = 0x40
	}


	private class WrappingCheckBox : CheckBox
	{
		private Size cachedSizeOfOneLineOfText = Size.Empty;

		private readonly Hashtable preferredSizeHash = new Hashtable(3);

		public WrappingCheckBox()
		{
			AutoSize = true;
		}

		protected override void OnTextChanged(EventArgs e)
		{
			base.OnTextChanged(e);
			Cachetextsize();
		}

		protected override void OnFontChanged(EventArgs e)
		{
			base.OnFontChanged(e);
			Cachetextsize();
		}

		private void Cachetextsize()
		{
			preferredSizeHash.Clear();
			if (string.IsNullOrEmpty(Text))
			{
				cachedSizeOfOneLineOfText = Size.Empty;
			}
			else
			{
				cachedSizeOfOneLineOfText = TextRenderer.MeasureText(Text, Font, new Size(int.MaxValue, int.MaxValue), TextFormatFlags.WordBreak);
			}
		}

		public override Size GetPreferredSize(Size proposedSize)
		{
			Size size = base.GetPreferredSize(proposedSize);
			if (size.Width > proposedSize.Width && (!string.IsNullOrEmpty(Text) && !proposedSize.Width.Equals(int.MaxValue) || !proposedSize.Height.Equals(int.MaxValue)))
			{
				Size size2 = size - cachedSizeOfOneLineOfText;
				Size size3 = proposedSize - size2 - new Size(3, 0);
				if (!preferredSizeHash.ContainsKey(size3))
				{
					size = size2 + TextRenderer.MeasureText(Text, Font, size3, TextFormatFlags.WordBreak);
					preferredSizeHash[size3] = size;
				}
				else
				{
					size = (Size)preferredSizeHash[size3];
				}
			}

			return size;
		}
	}



	private Exception _ExMessage = null;

	private EnMessageBoxDefaultButton defButton;

	private EnMessageBoxSymbol symbol = EnMessageBoxSymbol.Warning;

	private EnMessageBoxButtons buttons;

	private EnMessageBoxOptions options;

	// private readonly ArrayList lnkArray = new ArrayList();

	private bool m_showHelpButton = true;

	private int m_messageLimitCount = -1;

	private Icon m_formIcon;

	private Icon m_iconSymbol;

	private string[] m_buttonTextArray;

	private int m_buttonCount;

	private readonly ArrayList m_helpUrlArray = new ArrayList(5);

	private int m_helpUrlCount;

	private EnMessageBoxDialogResult m_customDR;

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

	public Exception ExMessage
	{
		get
		{
			return _ExMessage;
		}
		set
		{
			_ExMessage = value;
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

	public EnMessageBoxSymbol Symbol
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

	public EnMessageBoxButtons Buttons
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

	public EnMessageBoxOptions Options
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

	public EnMessageBoxDialogResult CustomDialogResult => m_customDR;

	public EnMessageBoxDefaultButton DefaultButton
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

	public MessageBoxDialog()
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
			if ((options & EnMessageBoxOptions.RtlReading) != 0)
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
		if (_ExMessage == null)
		{
			Exception ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		if (m_buttonTextArray == null || m_buttonTextArray.Length < 5)
		{
			ApplicationException ex = new(ControlsResources.CantComplete);
			Diag.Dug(ex);
			throw ex;
		}

		if (isButtonPressed)
		{
			ApplicationException ex = new(ControlsResources.CantReuseObject);
			Diag.Dug(ex);
			throw ex;
		}

		if (Caption == null || Caption.Length == 0)
		{
			Caption = _ExMessage.Source;
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

		if ((options & EnMessageBoxOptions.RtlReading) != 0)
		{
			RightToLeft = RightToLeft.Yes;
			RightToLeftLayout = true;
			chkDontShow.Padding = new Padding(chkDontShow.Padding.Right, chkDontShow.Padding.Top, chkDontShow.Padding.Left, chkDontShow.Padding.Bottom);
		}

		ResumeLayout();
	}

	private void InitializeButtons()
	{
		Button[] array = [button1, button2, button3, button4, button5];
		switch (buttons)
		{
			case EnMessageBoxButtons.OK:
				m_buttonTextArray[0] = ControlsResources.OKButton;
				button1.DialogResult = DialogResult.OK;
				AcceptButton = button1;
				CancelButton = button1;
				m_buttonCount = 1;
				break;
			case EnMessageBoxButtons.OKCancel:
				m_buttonTextArray[0] = ControlsResources.OKButton;
				m_buttonTextArray[1] = ControlsResources.CancelButton;
				button1.DialogResult = DialogResult.OK;
				button2.DialogResult = DialogResult.Cancel;
				AcceptButton = button1;
				CancelButton = button2;
				m_buttonCount = 2;
				break;
			case EnMessageBoxButtons.YesNo:
				m_buttonTextArray[0] = ControlsResources.YesButton;
				m_buttonTextArray[1] = ControlsResources.NoButton;
				button1.DialogResult = DialogResult.Yes;
				button2.DialogResult = DialogResult.No;
				m_buttonCount = 2;
				ControlBox = false;
				break;
			case EnMessageBoxButtons.YesNoCancel:
				m_buttonTextArray[0] = ControlsResources.YesButton;
				m_buttonTextArray[1] = ControlsResources.NoButton;
				m_buttonTextArray[2] = ControlsResources.CancelButton;
				button1.DialogResult = DialogResult.Yes;
				button2.DialogResult = DialogResult.No;
				button3.DialogResult = DialogResult.Cancel;
				m_buttonCount = 3;
				CancelButton = button3;
				break;
			case EnMessageBoxButtons.AbortRetryIgnore:
				m_buttonTextArray[0] = ControlsResources.AbortButton;
				m_buttonTextArray[1] = ControlsResources.RetryButton;
				m_buttonTextArray[2] = ControlsResources.IgnoreButton;
				button1.DialogResult = DialogResult.Abort;
				button2.DialogResult = DialogResult.Retry;
				button3.DialogResult = DialogResult.Ignore;
				m_buttonCount = 3;
				ControlBox = false;
				break;
			case EnMessageBoxButtons.RetryCancel:
				m_buttonTextArray[0] = ControlsResources.RetryButton;
				m_buttonTextArray[1] = ControlsResources.CancelButton;
				button1.DialogResult = DialogResult.Retry;
				button2.DialogResult = DialogResult.Cancel;
				CancelButton = button2;
				m_buttonCount = 2;
				break;
			case EnMessageBoxButtons.Custom:
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
			InvalidEnumArgumentException ex = new("DefaultButton", (int)defButton, typeof(EnMessageBoxDefaultButton));
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
			case EnMessageBoxSymbol.None:
				pnlIcon.Visible = false;
				break;
			case EnMessageBoxSymbol.Warning:
			case EnMessageBoxSymbol.Exclamation:
				m_iconSymbol = SystemIcons.Warning;
				break;
			case EnMessageBoxSymbol.Information:
			case EnMessageBoxSymbol.Asterisk:
				m_iconSymbol = SystemIcons.Information;
				break;
			case EnMessageBoxSymbol.Error:
			case EnMessageBoxSymbol.Hand:
				m_iconSymbol = SystemIcons.Error;
				break;
			case EnMessageBoxSymbol.Question:
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
			chkDontShow.Text = ControlsResources.DefaultCheckboxText;
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
		for (Exception innerException = _ExMessage.InnerException; innerException != null; innerException = innerException.InnerException)
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
		Exception innerException2 = _ExMessage;
		while (innerException2 != null && (m_messageLimitCount < 0 || num < m_messageLimitCount))
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = innerException2.Message != null && innerException2.Message.Length != 0 ? innerException2.Message : ControlsResources.CantComplete;
			if (m_showHelpButton)
			{
				string text2 = BuildHelpURL(innerException2);
				bool num3 = text2.Length > 0;
				m_helpUrlArray.Add(text2);
				text2 = text.Length <= 50 ? text : string.Format(CultureInfo.CurrentCulture, ControlsResources.AddEllipsis, text[..50]);
				ToolStripItem toolStripItem;
				if (num3)
				{
					toolStripItem = m_dropdown.Items.Add(string.Format(CultureInfo.CurrentCulture, ControlsResources.HelpMenuText, text2), null, ItemHelp_Click);
					m_helpUrlCount++;
				}
				else
				{
					toolStripItem = m_dropdown.Items.Add(string.Format(CultureInfo.CurrentCulture, ControlsResources.NoHelpMenuText, text2), null, ItemHelp_Click);
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
				string arg = !(innerException2.GetType() == typeof(FbException)) && !(innerException2.GetType() == typeof(FbException)) ? innerException2.Source : ControlsResources.SqlServerSource;
				if (num4 > 0)
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrorSourceNumber, arg, num4));
				}
				else
				{
					stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrorSource, arg));
				}
			}
			else if (num4 > 0)
			{
				stringBuilder.Append(' ');
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrorNumber, num4));
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
		for (Exception innerException = _ExMessage; innerException != null; innerException = innerException.InnerException)
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
			stringBuilder.Append(ControlsResources.SqlServerInfo);
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(Environment.NewLine);

			if (ex2.GetServer() != null && ex2.GetServer().Length > 0)
			{
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlServerName, ex2.GetServer()));
				stringBuilder.Append(Environment.NewLine);
			}

			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlError, ex2.GetErrorCode().ToString(CultureInfo.CurrentCulture)));
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlSeverity, ex2.GetClass().ToString(CultureInfo.CurrentCulture)));
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlState, ex2.SQLSTATE.ToString(CultureInfo.CurrentCulture)));
			stringBuilder.Append(Environment.NewLine);

			if (ex2.GetProcedure() != null && ex2.GetProcedure().Length > 0)
			{
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlProcedure, ex2.GetProcedure()));
				stringBuilder.Append(Environment.NewLine);
			}

			if (ex2.GetLineNumber() != 0)
			{
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlLineNumber, ex2.GetLineNumber().ToString(CultureInfo.CurrentCulture)));
				stringBuilder.Append(Environment.NewLine);
			}
		}

		if (ex.StackTrace != null && ex.StackTrace.Length > 0)
		{
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append("---------------");
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(ControlsResources.CodeLocation);
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
			case EnMessageBoxDefaultButton.Button1:
				button1.Focus();
				ActiveControl = button1;
				break;
			case EnMessageBoxDefaultButton.Button2:
				button2.Focus();
				ActiveControl = button2;
				break;
			case EnMessageBoxDefaultButton.Button3:
				button3.Focus();
				ActiveControl = button3;
				break;
			case EnMessageBoxDefaultButton.Button4:
				button4.Focus();
				ActiveControl = button4;
				break;
			case EnMessageBoxDefaultButton.Button5:
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
		if (_ExMessage == null)
		{
			return string.Empty;
		}

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append("------------------------------");
		stringBuilder.Append(Environment.NewLine);
		string value = stringBuilder.ToString();
		bool flag = _ExMessage.InnerException != null;
		int num = 1;
		StringBuilder stringBuilder2 = new StringBuilder();
		if (isInternal)
		{
			stringBuilder2.Append(ControlsResources.MessageTitle);
			stringBuilder2.Append(Caption);
		}

		for (Exception innerException = _ExMessage; innerException != null; innerException = innerException.InnerException)
		{
			if (isInternal || num > 1)
			{
				stringBuilder2.Append(value);
			}

			if (flag && num == 2)
			{
				stringBuilder2.Append(ControlsResources.AdditionalInfo);
				stringBuilder2.Append(Environment.NewLine);
			}

			if (isInternal || num > 1)
			{
				stringBuilder2.Append(Environment.NewLine);
			}

			if (innerException.Message == null || innerException.Message.Length == 0)
			{
				stringBuilder2.Append(ControlsResources.CantComplete);
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
					stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrorSourceNumber, ControlsResources.SqlServerSource, ((FbException)innerException).GetErrorCode()));
				}
				else
				{
					stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ErrorSource, innerException.Source));
				}
			}

			stringBuilder2.Append(Environment.NewLine);
			string text = BuildHelpURL(innerException);
			if (text.Length > 0)
			{
				stringBuilder2.Append(Environment.NewLine);
				stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ClipboardOrEmailHelpLink, text));
				stringBuilder2.Append(Environment.NewLine);
			}

			num++;
		}

		if (isInternal)
		{
			stringBuilder2.Append(value);
			stringBuilder2.Append(ControlsResources.Buttons);
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
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ClipboardOrEmailHelpLink, text));
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
				stringBuilder.Append(ControlsResources.CodeLocation);
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
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlServerName, ex2.GetServer()));
				stringBuilder.Append(Environment.NewLine);
			}

			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlError, ex2.GetErrorCode().ToString(CultureInfo.CurrentCulture)));
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlSeverity, ex2.GetClass().ToString(CultureInfo.CurrentCulture)));
			stringBuilder.Append(Environment.NewLine);
			stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlState, ex2.GetState().ToString(CultureInfo.CurrentCulture)));
			stringBuilder.Append(Environment.NewLine);
			if (ex2.GetProcedure() != null && ex2.GetProcedure().Length > 0)
			{
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlProcedure, ex2.GetProcedure()));
				stringBuilder.Append(Environment.NewLine);
			}

			if (ex2.GetLineNumber() != 0)
			{
				stringBuilder.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.SqlLineNumber, ex2.GetLineNumber().ToString(CultureInfo.CurrentCulture)));
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
		if (_ExMessage == null)
		{
			return string.Empty;
		}

		StringBuilder stringBuilder = new StringBuilder();
		stringBuilder.Append(Environment.NewLine);
		stringBuilder.Append("------------------------------");
		stringBuilder.Append(Environment.NewLine);
		string value = stringBuilder.ToString();
		bool flag = _ExMessage.InnerException != null;
		int num = 1;
		StringBuilder stringBuilder2 = new StringBuilder();
		stringBuilder2.Append(ControlsResources.MessageTitle);
		stringBuilder2.Append(Caption);
		for (Exception innerException = _ExMessage; innerException != null; innerException = innerException.InnerException)
		{
			stringBuilder2.Append(value);
			if (flag && num == 2)
			{
				stringBuilder2.Append(ControlsResources.AdditionalInfo);
				stringBuilder2.Append(Environment.NewLine);
			}

			stringBuilder2.Append(Environment.NewLine);
			if (innerException.Message == null || innerException.Message.Length == 0)
			{
				stringBuilder2.Append(ControlsResources.CantComplete);
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
				stringBuilder2.Append(string.Format(CultureInfo.CurrentCulture, ControlsResources.ClipboardOrEmailHelpLink, text));
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
			CopyToClipboardEventArgs args = new(text);
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
				ShowError(ControlsResources.CopyToClipboardError, exError);
			}
		}
	}

	private void Btn_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
		if (buttons == EnMessageBoxButtons.Custom)
		{
			if (sender == button1)
			{
				m_customDR = EnMessageBoxDialogResult.Button1;
			}
			else if (sender == button2)
			{
				m_customDR = EnMessageBoxDialogResult.Button2;
			}
			else if (sender == button3)
			{
				m_customDR = EnMessageBoxDialogResult.Button3;
			}
			else if (sender == button4)
			{
				m_customDR = EnMessageBoxDialogResult.Button4;
			}
			else
			{
				m_customDR = EnMessageBoxDialogResult.Button5;
			}
		}
		else
		{
			m_customDR = EnMessageBoxDialogResult.None;
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
			using (PrivacyConfirmationDialog privacyConfirmation = new PrivacyConfirmationDialog(Text, text))
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
			ShowError(string.Format(CultureInfo.CurrentCulture, ControlsResources.CantStartHelpLink, text), null);
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
			ShowError(string.Format(CultureInfo.CurrentCulture, ControlsResources.CantStartHelpLink, text), exError);
		}
	}

	private void ItemCopy_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
		CopyToClipboard();
	}

	public void ShowError(string str, Exception exError)
	{
		MessageCtl exceptionMessageBox = new(new Exception(str, exError)
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
			case EnMessageBoxButtons.OK:
				DialogResult = DialogResult.OK;
				break;
			case EnMessageBoxButtons.YesNo:
			case EnMessageBoxButtons.AbortRetryIgnore:
			case EnMessageBoxButtons.Custom:
				if (!isButtonPressed)
				{
					e.Cancel = true;
					MessageBeep(EnBeepType.Hand);
				}

				break;
			case EnMessageBoxButtons.OKCancel:
			case EnMessageBoxButtons.YesNoCancel:
			case EnMessageBoxButtons.RetryCancel:
				break;
		}
	}

	private void ShowDetails()
	{
		try
		{
			using AdvancedInformationDialog advancedInformation = new AdvancedInformationDialog();
			advancedInformation.MessageBoxForm = this;
			if ((options & EnMessageBoxOptions.RtlReading) != 0)
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
			ShowError(ControlsResources.CantShowTechnicalDetailsError, exError);
		}
	}

	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool MessageBeep(EnBeepType type);

	private void Beep()
	{
		switch (symbol)
		{
			case EnMessageBoxSymbol.None:
			case EnMessageBoxSymbol.Warning:
				MessageBeep(EnBeepType.Asterisk);
				break;
			case EnMessageBoxSymbol.Information:
				MessageBeep(EnBeepType.Exclamation);
				break;
			case EnMessageBoxSymbol.Error:
				MessageBeep(EnBeepType.Hand);
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
			this.components = new System.ComponentModel.Container();
			System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(MessageBoxDialog));
			this.pnlForm = new System.Windows.Forms.TableLayoutPanel();
			this.pnlIcon = new System.Windows.Forms.Panel();
			this.pnlMessage = new System.Windows.Forms.TableLayoutPanel();
			this.lblTopMessage = new System.Windows.Forms.LinkLabel();
			this.lblAdditionalInfo = new System.Windows.Forms.Label();
			this.pnlAdditional = new System.Windows.Forms.TableLayoutPanel();
			this.chkDontShow = new BlackbirdSql.Core.Controls.MessageBoxDialog.WrappingCheckBox();
			this.grpSeparator = new System.Windows.Forms.GroupBox();
			this.pnlButtons = new System.Windows.Forms.TableLayoutPanel();
			this.toolStrip1 = new System.Windows.Forms.ToolStrip();
			this.tbBtnHelp = new System.Windows.Forms.ToolStripDropDownButton();
			this.tbBtnHelpSingle = new System.Windows.Forms.ToolStripButton();
			this.tbBtnCopy = new System.Windows.Forms.ToolStripButton();
			this.tbBtnAdvanced = new System.Windows.Forms.ToolStripButton();
			this.button1 = new System.Windows.Forms.Button();
			this.button2 = new System.Windows.Forms.Button();
			this.button3 = new System.Windows.Forms.Button();
			this.button4 = new System.Windows.Forms.Button();
			this.button5 = new System.Windows.Forms.Button();
			this.imgIcons = new System.Windows.Forms.ImageList(this.components);
			this.pnlForm.SuspendLayout();
			this.pnlMessage.SuspendLayout();
			this.pnlButtons.SuspendLayout();
			this.toolStrip1.SuspendLayout();
			this.SuspendLayout();
			// 
			// pnlForm
			// 
			resources.ApplyResources(this.pnlForm, "pnlForm");
			this.pnlForm.Controls.Add(this.pnlIcon, 0, 0);
			this.pnlForm.Controls.Add(this.pnlMessage, 1, 0);
			this.pnlForm.Controls.Add(this.chkDontShow, 1, 1);
			this.pnlForm.Controls.Add(this.grpSeparator, 0, 2);
			this.pnlForm.Controls.Add(this.pnlButtons, 0, 3);
			this.pnlForm.Name = "pnlForm";
			this.pnlForm.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// pnlIcon
			// 
			resources.ApplyResources(this.pnlIcon, "pnlIcon");
			this.pnlIcon.Name = "pnlIcon";
			this.pnlIcon.Click += new System.EventHandler(this.HideBorderLines);
			this.pnlIcon.Paint += new System.Windows.Forms.PaintEventHandler(this.PnlIcon_Paint);
			// 
			// pnlMessage
			// 
			resources.ApplyResources(this.pnlMessage, "pnlMessage");
			this.pnlMessage.Controls.Add(this.lblTopMessage, 0, 0);
			this.pnlMessage.Controls.Add(this.lblAdditionalInfo, 0, 1);
			this.pnlMessage.Controls.Add(this.pnlAdditional, 0, 2);
			this.pnlMessage.Name = "pnlMessage";
			this.pnlMessage.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// lblTopMessage
			// 
			resources.ApplyResources(this.lblTopMessage, "lblTopMessage");
			this.lblTopMessage.Name = "lblTopMessage";
			this.lblTopMessage.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// lblAdditionalInfo
			// 
			resources.ApplyResources(this.lblAdditionalInfo, "lblAdditionalInfo");
			this.lblAdditionalInfo.Name = "lblAdditionalInfo";
			this.lblAdditionalInfo.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// pnlAdditional
			// 
			resources.ApplyResources(this.pnlAdditional, "pnlAdditional");
			this.pnlAdditional.Name = "pnlAdditional";
			this.pnlAdditional.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// chkDontShow
			// 
			resources.ApplyResources(this.chkDontShow, "chkDontShow");
			this.chkDontShow.Name = "chkDontShow";
			this.chkDontShow.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// grpSeparator
			// 
			resources.ApplyResources(this.grpSeparator, "grpSeparator");
			this.pnlForm.SetColumnSpan(this.grpSeparator, 2);
			this.grpSeparator.Name = "grpSeparator";
			this.grpSeparator.TabStop = false;
			// 
			// pnlButtons
			// 
			resources.ApplyResources(this.pnlButtons, "pnlButtons");
			this.pnlForm.SetColumnSpan(this.pnlButtons, 2);
			this.pnlButtons.Controls.Add(this.toolStrip1, 0, 0);
			this.pnlButtons.Controls.Add(this.button1, 1, 0);
			this.pnlButtons.Controls.Add(this.button2, 2, 0);
			this.pnlButtons.Controls.Add(this.button3, 3, 0);
			this.pnlButtons.Controls.Add(this.button4, 4, 0);
			this.pnlButtons.Controls.Add(this.button5, 5, 0);
			this.pnlButtons.Name = "pnlButtons";
			this.pnlButtons.Click += new System.EventHandler(this.HideBorderLines);
			// 
			// toolStrip1
			// 
			this.toolStrip1.AllowMerge = false;
			this.toolStrip1.CanOverflow = false;
			resources.ApplyResources(this.toolStrip1, "toolStrip1");
			this.toolStrip1.GripStyle = System.Windows.Forms.ToolStripGripStyle.Hidden;
			this.toolStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.tbBtnHelp,
            this.tbBtnHelpSingle,
            this.tbBtnCopy,
            this.tbBtnAdvanced});
			this.toolStrip1.LayoutStyle = System.Windows.Forms.ToolStripLayoutStyle.HorizontalStackWithOverflow;
			this.toolStrip1.Name = "toolStrip1";
			this.toolStrip1.RenderMode = System.Windows.Forms.ToolStripRenderMode.System;
			this.toolStrip1.TabStop = true;
			// 
			// tbBtnHelp
			// 
			this.tbBtnHelp.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnHelp, "tbBtnHelp");
			this.tbBtnHelp.Name = "tbBtnHelp";
			this.tbBtnHelp.Click += new System.EventHandler(this.TbBtnHelp_Click);
			// 
			// tbBtnHelpSingle
			// 
			this.tbBtnHelpSingle.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnHelpSingle, "tbBtnHelpSingle");
			this.tbBtnHelpSingle.Margin = new System.Windows.Forms.Padding(1, 1, 0, 2);
			this.tbBtnHelpSingle.Name = "tbBtnHelpSingle";
			this.tbBtnHelpSingle.Click += new System.EventHandler(this.TbBtnHelp_Click);
			// 
			// tbBtnCopy
			// 
			this.tbBtnCopy.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnCopy, "tbBtnCopy");
			this.tbBtnCopy.Margin = new System.Windows.Forms.Padding(1, 1, 0, 2);
			this.tbBtnCopy.Name = "tbBtnCopy";
			this.tbBtnCopy.Click += new System.EventHandler(this.ItemCopy_Click);
			// 
			// tbBtnAdvanced
			// 
			this.tbBtnAdvanced.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Image;
			resources.ApplyResources(this.tbBtnAdvanced, "tbBtnAdvanced");
			this.tbBtnAdvanced.Margin = new System.Windows.Forms.Padding(1, 1, 0, 2);
			this.tbBtnAdvanced.Name = "tbBtnAdvanced";
			this.tbBtnAdvanced.Click += new System.EventHandler(this.ItemShowDetails_Click);
			// 
			// button1
			// 
			resources.ApplyResources(this.button1, "button1");
			this.button1.Name = "button1";
			this.button1.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button2
			// 
			resources.ApplyResources(this.button2, "button2");
			this.button2.Name = "button2";
			this.button2.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button3
			// 
			resources.ApplyResources(this.button3, "button3");
			this.button3.Name = "button3";
			this.button3.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button4
			// 
			resources.ApplyResources(this.button4, "button4");
			this.button4.Name = "button4";
			this.button4.Tag = "z";
			this.button4.Click += new System.EventHandler(this.Btn_Click);
			// 
			// button5
			// 
			resources.ApplyResources(this.button5, "button5");
			this.button5.Name = "button5";
			this.button5.Click += new System.EventHandler(this.Btn_Click);
			// 
			// imgIcons
			// 
			this.imgIcons.ImageStream = ((System.Windows.Forms.ImageListStreamer)(resources.GetObject("imgIcons.ImageStream")));
			this.imgIcons.TransparentColor = System.Drawing.Color.Transparent;
			this.imgIcons.Images.SetKeyName(0, "indentarrow.bmp");
			this.imgIcons.Images.SetKeyName(1, "indentarrow_right.bmp");
			// 
			// MessageBoxDialog
			// 
			this.AccessibleRole = System.Windows.Forms.AccessibleRole.Alert;
			resources.ApplyResources(this, "$this");
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.pnlForm);
			this.DoubleBuffered = true;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
			this.KeyPreview = true;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Name = "MessageBoxDialog";
			this.ShowInTaskbar = false;
			this.Load += new System.EventHandler(this.NewMessageBoxForm_Load);
			this.Click += new System.EventHandler(this.NewMessageBoxForm_Click);
			this.HelpRequested += new System.Windows.Forms.HelpEventHandler(this.NewMessageBoxForm_HelpRequested);
			this.KeyDown += new System.Windows.Forms.KeyEventHandler(this.NewMessageBoxForm_KeyDown);
			this.pnlForm.ResumeLayout(false);
			this.pnlForm.PerformLayout();
			this.pnlMessage.ResumeLayout(false);
			this.pnlMessage.PerformLayout();
			this.pnlButtons.ResumeLayout(false);
			this.pnlButtons.PerformLayout();
			this.toolStrip1.ResumeLayout(false);
			this.toolStrip1.PerformLayout();
			this.ResumeLayout(false);
			this.PerformLayout();

	}
}
