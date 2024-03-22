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


// =========================================================================================================
//										AdvancedMessageBox Class 
//
/// <summary>
/// Universal MessageBox for all BlackbirdSql messages.
/// </summary>
// =========================================================================================================
public partial class AdvancedMessageBox : Form
{

	// ----------------------------------------------------
	#region Constructors / Destructors - AdvancedMessageBox
	// ----------------------------------------------------


	/// <summary>
	/// AdvancedMessageBox .ctor
	/// </summary>
	public AdvancedMessageBox()
	{
		InitializeComponent();
		tbBtnHelp.DropDown = _Dropdown;
		toolStrip1.Renderer = new PrivateRenderer();
		Icon = _FormIcon;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Internal Classes - AdvancedMessageBox
	// =========================================================================================================


	public enum EnBeepType
	{
		Standard = -1,
		Default = 0,
		Hand = 0x10,
		Question = 0x20,
		Exclamation = 48,
		Asterisk = 0x40
	}


	#endregion Internal Classes





	// =========================================================================================================
	#region Constants - AdvancedMessageBox
	// =========================================================================================================


	private const string C_HelpUrlBaseKey = "HelpLink.BaseHelpUrl";
	private const string C_PrefixKey = "HelpLink.";
	private const string C_AdvancedKey = "AdvancedInformation.";
	private const string C_HttpPrefixKey = "HTTP";
	// private const int C_MaxAdditionalPanelHeight = 150;
	// private const int C_WM_USER = 1024;
	private const int C_WM_GETDLGID = 7690;


	#endregion Constants





	// =========================================================================================================
	#region Fields - AdvancedMessageBox
	// =========================================================================================================


	private int _ButtonCount;
	private EnMessageBoxButtons _Buttons;
	private string[] _ButtonTextArray;
	private Bitmap _CustomSymbol;
	private EnMessageBoxDialogResult _CustomDialogResult;
	private EnMessageBoxDefaultButton _DefaultButton;
	private bool _DoBeep = true;
	private readonly ToolStripDropDown _Dropdown = new ToolStripDropDown();
	private Exception _ExMessage = null;
	private Icon _FormIcon;
	private readonly ArrayList _HelpUrlArray = new ArrayList(5);
	private int _HelpUrlCount;
	private Icon _IconSymbol;
	private bool _IsButtonPressed;
	// private readonly ArrayList _LnkArray = new ArrayList();
	private int _MaxMessages = -1;
	private EnMessageBoxOptions _Options;
	private bool _ShowCheckbox;
	private bool _ShowHelpButton = true;
	private EnMessageBoxSymbol _Symbol = EnMessageBoxSymbol.Warning;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AdvancedMessageBox
	// =========================================================================================================

	public EnMessageBoxButtons Buttons
	{
		get { return _Buttons; }
		set { _Buttons = value; }
	}

	public string Caption
	{
		get { return Text; }
		set { Text = value; }
	}

	public string CheckBoxText
	{
		get { return chkDontShow.Text; }
		set { chkDontShow.Text = value; }
	}

	public EnMessageBoxDialogResult CustomDialogResult => _CustomDialogResult;

	public Bitmap CustomSymbol
	{
		get { return _CustomSymbol; }
		set { _CustomSymbol = value; }
	}

	public EnMessageBoxDefaultButton DefaultButton
	{
		get { return _DefaultButton; }
		set { _DefaultButton = value; }
	}

	public bool DoBeep
	{
		get { return _DoBeep; }
		set { _DoBeep = value; }
	}

	public Exception ExMessage
	{
		get { return _ExMessage; }
		set { _ExMessage = value; }
	}

	public Icon FormIcon
	{
		get { return _FormIcon; }
		set { _FormIcon = value; }
	}

	public bool IsCheckBoxChecked
	{
		get { return chkDontShow.Checked; }
		set { chkDontShow.Checked = value; }
	}

	public int MaxMessages
	{
		get { return _MaxMessages; }
		set { _MaxMessages = value; }
	}

	public EnMessageBoxOptions Options
	{
		get { return _Options; }
		set { _Options = value; }
	}

	public bool ShowCheckBox
	{
		get { return _ShowCheckbox; }
		set { _ShowCheckbox = value; }
	}

	public bool ShowHelpButton
	{
		get { return _ShowHelpButton; }
		set { _ShowHelpButton = value; }
	}

	public EnMessageBoxSymbol Symbol
	{
		get { return _Symbol; }
		set { _Symbol = value; }
	}



	public event CopyToClipboardEventHandler CopyToClipboardInternalEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AdvancedMessageBox
	// =========================================================================================================


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
			if ((_Options & EnMessageBoxOptions.RtlReading) != 0)
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



	private void Beep()
	{
		switch (_Symbol)
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
			for (int i = 0; i < _ButtonCount; i++)
			{
				stringBuilder2.Append(Environment.NewLine);
				stringBuilder2.Append(_ButtonTextArray[i]);
			}

			stringBuilder2.Append(value);
		}

		return stringBuilder2.ToString();
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



	private void GetHelp(int index)
	{
		if (_HelpUrlCount == 1)
		{
			index = 0;
			while (index < _HelpUrlArray.Count && ((string)_HelpUrlArray[index]).Length <= 0)
			{
				index++;
			}
		}

		if (index >= _HelpUrlArray.Count || ((string)_HelpUrlArray[index]).Length == 0)
		{
			return;
		}

		string text = (string)_HelpUrlArray[index];
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


	private void HideBorderLines(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
	}



	private void InitializeButtons()
	{
		Button[] array = [button1, button2, button3, button4, button5];
		switch (_Buttons)
		{
			case EnMessageBoxButtons.OK:
				_ButtonTextArray[0] = ControlsResources.OKButton;
				button1.DialogResult = DialogResult.OK;
				AcceptButton = button1;
				CancelButton = button1;
				_ButtonCount = 1;
				break;
			case EnMessageBoxButtons.OKCancel:
				_ButtonTextArray[0] = ControlsResources.OKButton;
				_ButtonTextArray[1] = ControlsResources.CancelButton;
				button1.DialogResult = DialogResult.OK;
				button2.DialogResult = DialogResult.Cancel;
				AcceptButton = button1;
				CancelButton = button2;
				_ButtonCount = 2;
				break;
			case EnMessageBoxButtons.YesNo:
				_ButtonTextArray[0] = ControlsResources.YesButton;
				_ButtonTextArray[1] = ControlsResources.NoButton;
				button1.DialogResult = DialogResult.Yes;
				button2.DialogResult = DialogResult.No;
				_ButtonCount = 2;
				ControlBox = false;
				break;
			case EnMessageBoxButtons.YesNoCancel:
				_ButtonTextArray[0] = ControlsResources.YesButton;
				_ButtonTextArray[1] = ControlsResources.NoButton;
				_ButtonTextArray[2] = ControlsResources.CancelButton;
				button1.DialogResult = DialogResult.Yes;
				button2.DialogResult = DialogResult.No;
				button3.DialogResult = DialogResult.Cancel;
				_ButtonCount = 3;
				CancelButton = button3;
				break;
			case EnMessageBoxButtons.AbortRetryIgnore:
				_ButtonTextArray[0] = ControlsResources.AbortButton;
				_ButtonTextArray[1] = ControlsResources.RetryButton;
				_ButtonTextArray[2] = ControlsResources.IgnoreButton;
				button1.DialogResult = DialogResult.Abort;
				button2.DialogResult = DialogResult.Retry;
				button3.DialogResult = DialogResult.Ignore;
				_ButtonCount = 3;
				ControlBox = false;
				break;
			case EnMessageBoxButtons.RetryCancel:
				_ButtonTextArray[0] = ControlsResources.RetryButton;
				_ButtonTextArray[1] = ControlsResources.CancelButton;
				button1.DialogResult = DialogResult.Retry;
				button2.DialogResult = DialogResult.Cancel;
				CancelButton = button2;
				_ButtonCount = 2;
				break;
			case EnMessageBoxButtons.Custom:
				ControlBox = false;
				break;
		}

		int width = pnlButtons.GetPreferredSize(Size.Empty).Width;
		for (int i = 0; i < _ButtonCount; i++)
		{
			Button obj = array[i];
			obj.Text = _ButtonTextArray[i];
			obj.Visible = true;
		}

		AdjustDialogWidth(pnlButtons.GetPreferredSize(Size.Empty).Width - width, isAdjustingForButtons: true);
		if ((int)_DefaultButton >= _ButtonCount)
		{
			InvalidEnumArgumentException ex = new("DefaultButton", (int)_DefaultButton, typeof(EnMessageBoxDefaultButton));
			Diag.Dug(ex);
			throw ex;
		}

		AcceptButton = array[(int)_DefaultButton];
	}



	private void InitializeCheckbox()
	{
		if (!_ShowCheckbox)
		{
			chkDontShow.Visible = false;
			chkDontShow.Enabled = false;
		}
		else if (chkDontShow.Text.Length == 0)
		{
			chkDontShow.Text = ControlsResources.DefaultCheckboxText;
		}
	}



	private void InitializeMessage()
	{
		int num = 0;
		int num2 = 0;
		_Dropdown.Items.Clear();
		for (Exception innerException = _ExMessage.InnerException; innerException != null; innerException = innerException.InnerException)
		{
			num2++;
		}

		if (_MaxMessages > 0 && num2 > _MaxMessages - 1)
		{
			num2 = _MaxMessages - 1;
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
		while (innerException2 != null && (_MaxMessages < 0 || num < _MaxMessages))
		{
			StringBuilder stringBuilder = new StringBuilder();
			string text = innerException2.Message != null && innerException2.Message.Length != 0 ? innerException2.Message : ControlsResources.CantComplete;
			if (_ShowHelpButton)
			{
				string text2 = BuildHelpURL(innerException2);
				bool num3 = text2.Length > 0;
				_HelpUrlArray.Add(text2);
				text2 = text.Length <= 50 ? text : string.Format(CultureInfo.CurrentCulture, ControlsResources.AddEllipsis, text[..50]);
				ToolStripItem toolStripItem;
				if (num3)
				{
					toolStripItem = _Dropdown.Items.Add(string.Format(CultureInfo.CurrentCulture, ControlsResources.HelpMenuText, text2), null, ItemHelp_Click);
					_HelpUrlCount++;
				}
				else
				{
					toolStripItem = _Dropdown.Items.Add(string.Format(CultureInfo.CurrentCulture, ControlsResources.NoHelpMenuText, text2), null, ItemHelp_Click);
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



	private void InitializeSymbol()
	{
		if (_CustomSymbol != null)
		{
			int num = _CustomSymbol.Width + 2 - pnlIcon.Width;
			pnlIcon.Width += num;
			AdjustDialogWidth(num, isAdjustingForButtons: false);
			pnlIcon.Height = _CustomSymbol.Height + 2;
			pnlIcon.MinimumSize = new Size(0, _CustomSymbol.Height + 2);
			return;
		}

		switch (_Symbol)
		{
			case EnMessageBoxSymbol.None:
				pnlIcon.Visible = false;
				break;
			case EnMessageBoxSymbol.Warning:
			case EnMessageBoxSymbol.Exclamation:
				_IconSymbol = SystemIcons.Warning;
				break;
			case EnMessageBoxSymbol.Information:
			case EnMessageBoxSymbol.Asterisk:
				_IconSymbol = SystemIcons.Information;
				break;
			case EnMessageBoxSymbol.Error:
			case EnMessageBoxSymbol.Hand:
				_IconSymbol = SystemIcons.Error;
				break;
			case EnMessageBoxSymbol.Question:
				_IconSymbol = SystemIcons.Question;
				break;
		}
	}



	[DllImport("user32.dll", CharSet = CharSet.Auto)]
	public static extern bool MessageBeep(EnBeepType type);



	public void PrepareToShow()
	{
		if (_ExMessage == null)
		{
			Exception ex = new();
			Diag.Dug(ex);
			throw ex;
		}

		if (_ButtonTextArray == null || _ButtonTextArray.Length < 5)
		{
			ApplicationException ex = new(ControlsResources.CantComplete);
			Diag.Dug(ex);
			throw ex;
		}

		if (_IsButtonPressed)
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
		toolStrip1.Visible = _ShowHelpButton;
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

		if (_ShowHelpButton)
		{
			if (_HelpUrlCount == 0)
			{
				tbBtnHelp.Visible = false;
				tbBtnHelpSingle.Visible = false;
				tbBtnHelp.Enabled = false;
				_Dropdown.Items.Clear();
			}
			else if (_Dropdown.Items.Count == 1 && _HelpUrlCount == 1)
			{
				_Dropdown.Items.Clear();
				tbBtnHelp.Visible = false;
				tbBtnHelpSingle.Visible = true;
			}
		}

		if ((_Options & EnMessageBoxOptions.RtlReading) != 0)
		{
			RightToLeft = RightToLeft.Yes;
			RightToLeftLayout = true;
			chkDontShow.Padding = new Padding(chkDontShow.Padding.Right, chkDontShow.Padding.Top, chkDontShow.Padding.Left, chkDontShow.Padding.Bottom);
		}

		ResumeLayout();
	}



	public void SetButtonText(string[] value)
	{
		_ButtonTextArray = value;
		if (_ButtonTextArray[0] == null || _ButtonTextArray[0].Length == 0)
		{
			_ButtonCount = 0;
		}
		else if (_ButtonTextArray[1] == null || _ButtonTextArray[1].Length == 0)
		{
			_ButtonCount = 1;
		}
		else if (_ButtonTextArray[2] == null || _ButtonTextArray[2].Length == 0)
		{
			_ButtonCount = 2;
		}
		else if (_ButtonTextArray[3] == null || _ButtonTextArray[3].Length == 0)
		{
			_ButtonCount = 3;
		}
		else if (_ButtonTextArray[4] == null || _ButtonTextArray[4].Length == 0)
		{
			_ButtonCount = 4;
		}
		else
		{
			_ButtonCount = 5;
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



	private void ShowDetails()
	{
		try
		{
			using AdvancedInformationDialog advancedInformation = new AdvancedInformationDialog();
			advancedInformation.MessageBoxForm = this;
			if ((_Options & EnMessageBoxOptions.RtlReading) != 0)
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



	public void ShowError(string str, Exception exError)
	{
		MessageCtl exceptionMessageBox = new(new Exception(str, exError)
		{
			Source = Text
		})
		{
			Options = _Options
		};
		exceptionMessageBox.Show(Parent);
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AdvancedMessageBox
	// =========================================================================================================


	private void Btn_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
		if (_Buttons == EnMessageBoxButtons.Custom)
		{
			if (sender == button1)
			{
				_CustomDialogResult = EnMessageBoxDialogResult.Button1;
			}
			else if (sender == button2)
			{
				_CustomDialogResult = EnMessageBoxDialogResult.Button2;
			}
			else if (sender == button3)
			{
				_CustomDialogResult = EnMessageBoxDialogResult.Button3;
			}
			else if (sender == button4)
			{
				_CustomDialogResult = EnMessageBoxDialogResult.Button4;
			}
			else
			{
				_CustomDialogResult = EnMessageBoxDialogResult.Button5;
			}
		}
		else
		{
			_CustomDialogResult = EnMessageBoxDialogResult.None;
		}

		_IsButtonPressed = true;
		Close();
	}



	private void ItemCopy_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
		CopyToClipboard();
	}



	private void ItemHelp_Click(object sender, EventArgs e)
	{
		ToolStripItem toolStripItem = sender as ToolStripItem;
		ShowBorderLines(-1);
		GetHelp((int)toolStripItem.Tag & 0xFFF);
	}



	private void ItemShowDetails_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
		ShowDetails();
	}



	private void AdvancedMessageBox_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
	}



	private void AdvancedMessageBox_HelpRequested(object sender, HelpEventArgs hlpevent)
	{
		if (_ShowHelpButton)
		{
			if (_Dropdown.Items.Count == 0 && _HelpUrlCount == 1)
			{
				GetHelp(0);
			}

			hlpevent.Handled = true;
		}
	}



	private void AdvancedMessageBox_FormClosing(object sender, FormClosingEventArgs e)
	{
		switch (_Buttons)
		{
			case EnMessageBoxButtons.OK:
				DialogResult = DialogResult.OK;
				break;
			case EnMessageBoxButtons.YesNo:
			case EnMessageBoxButtons.AbortRetryIgnore:
			case EnMessageBoxButtons.Custom:
				if (!_IsButtonPressed)
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



	private void AdvancedMessageBox_KeyDown(object sender, KeyEventArgs e)
	{
		if (e.Modifiers == Keys.Control && (e.KeyData & Keys.C) == Keys.C || (e.KeyData & Keys.Insert) == Keys.Insert)
		{
			CopyToClipboard();
			e.Handled = true;
		}
	}



	private void AdvancedMessageBox_Load(object sender, EventArgs e)
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

		if (_DoBeep)
		{
			Beep();
		}

		switch (_DefaultButton)
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



	private void OnHelpButtonMouseEnter(object sender, EventArgs e)
	{
		ShowBorderLines((int)((ToolStripItem)sender).Tag);
	}



	private void OnHelpButtonMouseLeave(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
	}



	private void PnlIcon_Paint(object sender, PaintEventArgs e)
	{
		if (_CustomSymbol != null)
		{
			e.Graphics.DrawImageUnscaled(_CustomSymbol, new Point(0, 0));
		}
		else if (_IconSymbol != null)
		{
			e.Graphics.DrawIcon(_IconSymbol, 0, 0);
		}
	}



	private void TbBtnHelp_Click(object sender, EventArgs e)
	{
		ShowBorderLines(-1);
		if (_Dropdown.Items.Count == 0 && _HelpUrlCount == 1)
		{
			GetHelp(0);
		}
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


	#endregion Event Handling

}