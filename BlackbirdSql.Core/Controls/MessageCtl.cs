// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.ExceptionMessageBox

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Core.Controls.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Events;
using BlackbirdSql.Core.Properties;
using Microsoft.Win32;



namespace BlackbirdSql.Core.Controls;

[ComVisible(false)]


// =========================================================================================================
//											 MessageCtl Class
//
/// <summary>
/// Displays extension wide messages and detailed exception messages. 
/// </summary>
// =========================================================================================================
public class MessageCtl
{

	// ---------------------------------
	#region Constructors - MessageBoxCtl
	// ---------------------------------


	public MessageCtl()
	{
	}

	public MessageCtl(Exception exception)
	{
		m_message = exception;
	}

	public MessageCtl(string text)
	{
		m_text = text;
	}

	public MessageCtl(Exception exception, string text)
	{
		m_text = text;
		m_message = exception;
	}

	public MessageCtl(string text, string caption)
	{
		m_text = text;
		m_caption = caption;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons)
	{
		m_message = exception;
		m_buttons = buttons;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons)
	{
		m_text = text;
		m_caption = caption;
		m_buttons = buttons;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol)
	{
		m_message = exception;
		m_buttons = buttons;
		m_symbol = symbol;
	}


	public MessageCtl(Exception exception, string text, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol)
	{
		m_text = text;
		m_message = exception;
		m_buttons = buttons;
		m_symbol = symbol;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol)
	{
		m_text = text;
		m_caption = caption;
		m_buttons = buttons;
		m_symbol = symbol;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton)
	{
		m_message = exception;
		m_buttons = buttons;
		m_symbol = symbol;
		m_defaultButton = defaultButton;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton)
	{
		m_text = text;
		m_caption = caption;
		m_buttons = buttons;
		m_symbol = symbol;
		m_defaultButton = defaultButton;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton, EnMessageBoxOptions options)
	{
		m_message = exception;
		m_buttons = buttons;
		m_symbol = symbol;
		m_defaultButton = defaultButton;
		m_options = options;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton, EnMessageBoxOptions options)
	{
		m_text = text;
		m_caption = caption;
		m_buttons = buttons;
		m_symbol = symbol;
		m_defaultButton = defaultButton;
		m_options = options;
	}


	#endregion Constructors





	// =========================================================================================================
	#region Constants - MessageCtl
	// =========================================================================================================


	// private const EnMessageBoxOptions InvalidOptionsMask = ~(EnMessageBoxOptions.RightAlign | EnMessageBoxOptions.RtlReading);


	#endregion Constants





	// =========================================================================================================
	#region Fields - MessageCtl
	// =========================================================================================================


	private static string _ApplicationTitle;
	private readonly string[] m_buttonTextArray = new string[5];
	private int m_buttonCount;
	private DialogResult m_defaultDialogResult = DialogResult.OK;
	private string m_checkboxRegistryValue = string.Empty;
	private bool m_CheckBoxRegistryMeansDoNotShowDialog = true;
	private RegistryKey m_checkboxRegistryKey;
	private bool m_useOwnerFont;
	private string m_text = string.Empty;
	private string m_helpLink = string.Empty;
	private readonly Exception m_exData = new ApplicationException();
	private Exception m_innerException;
	private bool m_beep = true;
	private bool m_showHelpButton = true;
	private bool m_showCheckBox;
	private bool m_isCheckBoxChecked;
	private string m_caption = string.Empty;
	private string m_checkBoxText = string.Empty;
	private int m_messageLevelCount = -1;
	private Bitmap m_customSymbol;
	private Font m_font;
	private Exception m_message;
	private EnMessageBoxButtons m_buttons;
	private EnMessageBoxSymbol m_symbol = EnMessageBoxSymbol.Warning;
	private EnMessageBoxDefaultButton m_defaultButton;
	private EnMessageBoxOptions m_options;
	private EnMessageBoxDialogResult m_customDialogResult;


	#endregion Fields





	// =========================================================================================================
	#region Property accessors - MessageCtl
	// =========================================================================================================


	public static string ApplicationTitle
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_ApplicationTitle))
			{
				return ControlsResources.ConnectToServer;
			}

			return _ApplicationTitle;
		}
		set
		{
			_ApplicationTitle = value;
		}
	}

	public Exception Message
	{
		get { return m_message; }
		set { m_message = value; }
	}

	public string Caption
	{
		get { return m_caption; }
		set { m_caption = value; }
	}

	public string Text
	{
		get { return m_text; }
		set { m_text = value; }
	}

	public string HelpLink
	{
		get { return m_helpLink; }
		set { m_helpLink = value; }
	}

	public IDictionary Data => m_exData.Data;

	public Exception InnerException
	{
		get { return m_innerException; }
		set { m_innerException = value; }
	}

	public EnMessageBoxButtons Buttons
	{
		get { return m_buttons; }
		set { m_buttons = value; }
	}

	public EnMessageBoxSymbol Symbol
	{
		get { return m_symbol; }
		set { m_symbol = value; }
	}

	public Bitmap CustomSymbol
	{
		get { return m_customSymbol; }
		set { m_customSymbol = value; }
	}

	public EnMessageBoxDefaultButton DefaultButton
	{
		get { return m_defaultButton; }
		set { m_defaultButton = value; }
	}

	public EnMessageBoxOptions Options
	{
		get { return m_options; }
		set { m_options = value; }
	}

	public int MessageLevelDefault
	{
		get { return m_messageLevelCount; }
		set { m_messageLevelCount = value; }
	}

	public bool ShowToolBar
	{
		get { return m_showHelpButton; }
		set { m_showHelpButton = value; }
	}

	public bool UseOwnerFont
	{
		get { return m_useOwnerFont; }
		set { m_useOwnerFont = value; }
	}

	public Font Font
	{
		get { return m_font; }
		set { m_font = value; m_useOwnerFont = false; }
	}

	public bool ShowCheckBox
	{
		get { return m_showCheckBox; }
		set { m_showCheckBox = value; }
	}

	public bool IsCheckBoxChecked
	{
		get { return m_isCheckBoxChecked; }
		set { m_isCheckBoxChecked = value; }
	}

	public string CheckBoxText
	{
		get { return m_checkBoxText; }
		set {  m_checkBoxText = value; }
	}

	public RegistryKey CheckBoxRegistryKey
	{
		get { return m_checkboxRegistryKey; }
		set { m_checkboxRegistryKey = value; }
	}

	public string CheckBoxRegistryValue
	{
		get { return m_checkboxRegistryValue; }
		set { m_checkboxRegistryValue = value; }
	}

	public bool CheckBoxRegistryMeansDoNotShowDialog
	{
		get { return m_CheckBoxRegistryMeansDoNotShowDialog; }
		set { m_CheckBoxRegistryMeansDoNotShowDialog = value; }
	}

	public DialogResult DefaultDialogResult
	{
		get { return m_defaultDialogResult; }
		set { m_defaultDialogResult = value; }
	}

	public EnMessageBoxDialogResult CustomDialogResult => m_customDialogResult;

	public static string OKButtonText => ControlsResources.OKButton;

	public static string CancelButtonText => ControlsResources.CancelButton;

	public static string YesButtonText => ControlsResources.YesButton;

	public static string NoButtonText => ControlsResources.NoButton;

	public static string AbortButtonText => ControlsResources.AbortButton;

	public static string RetryButtonText => ControlsResources.RetryButton;

	public static string FailButtonText => ControlsResources.FailButton;

	public static string IgnoreButtonText => ControlsResources.IgnoreButton;

	public bool Beep
	{
		get { return m_beep; }
		set	{ m_beep = value; }
	}

	public event CopyToClipboardEventHandler CopyToClipboardEvent;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - MessageCtl
	// =========================================================================================================



	public void SetButtonText(string button1Text, string button2Text, string button3Text, string button4Text, string button5Text)
	{
		m_buttonTextArray[0] = button1Text;
		m_buttonTextArray[1] = button2Text;
		m_buttonTextArray[2] = button3Text;
		m_buttonTextArray[3] = button4Text;
		m_buttonTextArray[4] = button5Text;
		if (button1Text == null || button1Text.Length == 0)
		{
			m_buttonCount = 0;
		}
		else if (button2Text == null || button2Text.Length == 0)
		{
			m_buttonCount = 1;
		}
		else if (button3Text == null || button3Text.Length == 0)
		{
			m_buttonCount = 2;
		}
		else if (button4Text == null || button4Text.Length == 0)
		{
			m_buttonCount = 3;
		}
		else if (button5Text == null || button5Text.Length == 0)
		{
			m_buttonCount = 4;
		}
		else
		{
			m_buttonCount = 5;
		}
	}

	public void SetButtonText(string button1Text, string button2Text, string button3Text, string button4Text)
	{
		SetButtonText(button1Text, button2Text, button3Text, button4Text, null);
	}

	public void SetButtonText(string button1Text, string button2Text, string button3Text)
	{
		SetButtonText(button1Text, button2Text, button3Text, null, null);
	}

	public void SetButtonText(string button1Text, string button2Text)
	{
		SetButtonText(button1Text, button2Text, null, null, null);
	}

	public void SetButtonText(string button1Text)
	{
		SetButtonText(button1Text, null, null, null, null);
	}

	delegate int BeforeCloseSolutionDelegate(object pUnkReserved);

	public DialogResult Show(IntPtr hwnd, string message, string source, string sourceAppName, string sourceAppVersion, string sourceModule, string sourceMessageId, string sourceLanguage)
	{
		MessageBoxParent owner = new MessageBoxParent(hwnd);
		Message = new(message)
		{
			Source = source
		};
		return Show(owner);
	}

	public DialogResult Show(IWin32Window owner)
	{
		if (m_message == null && (Text == null || Text.Length == 0))
		{
			ArgumentNullException ex = new("Message");
			Diag.Dug(ex);
			throw ex;
		}

		if (m_buttons < EnMessageBoxButtons.OK || m_buttons > EnMessageBoxButtons.Custom)
		{
			InvalidEnumArgumentException ex = new("Buttons", (int)m_buttons, typeof(EnMessageBoxButtons));
			Diag.Dug(ex);
			throw ex;
		}

		if (m_symbol < EnMessageBoxSymbol.None || m_symbol > EnMessageBoxSymbol.Hand)
		{
			InvalidEnumArgumentException ex = new("Symbol", (int)m_symbol, typeof(EnMessageBoxSymbol));
			Diag.Dug(ex);
			throw ex;
		}

		if (m_defaultButton < EnMessageBoxDefaultButton.Button1 || m_defaultButton > EnMessageBoxDefaultButton.Button5)
		{
			InvalidEnumArgumentException ex = new("DefaultButton", (int)m_defaultButton, typeof(EnMessageBoxDefaultButton));
			Diag.Dug(ex);
			throw ex;
		}

		if (((uint)m_options & 0xFFFFFFFCu) != 0)
		{
			InvalidEnumArgumentException ex = new("Options", (int)m_options, typeof(EnMessageBoxOptions));
			Diag.Dug(ex);
			throw ex;
		}

		if (m_buttons == EnMessageBoxButtons.Custom && m_buttonCount == 0)
		{
			Exception ex = new(ControlsResources.CustomButtonTextError);
			Diag.Dug(ex);
			throw ex;
		}

		if (m_messageLevelCount != -1 && m_messageLevelCount < 1)
		{
			ArgumentOutOfRangeException ex = new("MessageLevelDefault", m_messageLevelCount, ControlsResources.MessageLevelCountError);
			Diag.Dug(ex);
			throw ex;
		}

		bool hasError = m_message != null;
		if (m_message == null)
		{
			m_message = new ApplicationException(m_text, m_innerException)
			{
				HelpLink = m_helpLink
			};
			foreach (DictionaryEntry datum in m_exData.Data)
			{
				m_message.Data.Add(datum.Key, datum.Value);
			}
		}

		if (m_useOwnerFont)
		{
			try
			{
				if (owner is Form formCtl)
				{
					Font = formCtl.Font;
				}
				else if (owner is UserControl control)
				{
					Font = control.Font;
				}
				else if (owner is Control fontControl)
				{
					Font = fontControl.Font;
				}
			}
			catch (Exception)
			{
			}
		}

		if (m_showCheckBox && m_checkboxRegistryKey != null)
		{
			try
			{
				m_isCheckBoxChecked = (int)m_checkboxRegistryKey.GetValue(m_checkboxRegistryValue, 0) != 0;
				if (m_CheckBoxRegistryMeansDoNotShowDialog && m_isCheckBoxChecked)
				{
					return m_defaultDialogResult;
				}
			}
			catch (Exception)
			{
			}
		}

		if (m_caption == null || m_caption.Length == 0)
		{
			m_caption = m_message.Source;
		}

		if ((m_caption == null || m_caption.Length == 0) && owner is Form form)
		{
			m_caption = form.Text;
		}

		using AdvancedMessageBox messageBoxForm = new AdvancedMessageBox();
		messageBoxForm.SetButtonText(m_buttonTextArray);
		messageBoxForm.Buttons = m_buttons;
		messageBoxForm.Caption = m_caption;
		messageBoxForm.ExMessage = m_message;
		messageBoxForm.Symbol = m_symbol;
		messageBoxForm.DefaultButton = m_defaultButton;
		messageBoxForm.Options = m_options;
		messageBoxForm.DoBeep = m_beep;
		messageBoxForm.CheckBoxText = m_checkBoxText;
		messageBoxForm.IsCheckBoxChecked = m_isCheckBoxChecked;
		messageBoxForm.ShowCheckBox = m_showCheckBox;
		messageBoxForm.MaxMessages = m_messageLevelCount;
		messageBoxForm.ShowHelpButton = m_showHelpButton;
		messageBoxForm.CopyToClipboardInternalEvent += OnCopyToClipboardEventInternal;
		if (m_customSymbol != null)
		{
			messageBoxForm.CustomSymbol = m_customSymbol;
		}

		if (m_font != null)
		{
			messageBoxForm.Font = m_font;
		}

		if (owner == null)
		{
			messageBoxForm.StartPosition = FormStartPosition.CenterScreen;
			messageBoxForm.ShowInTaskbar = true;
		}
		else
		{
			messageBoxForm.StartPosition = FormStartPosition.CenterParent;
		}

		/*
		messageBoxForm.Shown += delegate
		{
			if (hasError)
			{
				Trace.TraceError("ExceptionMessageBoxShown@" + m_message.Message);
				Trace.Write(m_message);
			}
			else
			{
				Trace.TraceInformation("ExceptionMessageBoxShown@" + m_message.Message);
			}
		};
		messageBoxForm.FormClosed += delegate
		{
			Trace.TraceInformation("ExceptionMessageBoxClosed@" + m_message.Message);
		};
		*/

		messageBoxForm.Shown += OnMessageBoxShown;


		messageBoxForm.PrepareToShow();
		DialogResult result = messageBoxForm.ShowDialog(owner);
		if (messageBoxForm.ShowCheckBox && m_checkboxRegistryKey != null)
		{
			m_checkboxRegistryKey.SetValue(m_checkboxRegistryValue, messageBoxForm.IsCheckBoxChecked ? 1 : 0);
		}

		m_isCheckBoxChecked = messageBoxForm.IsCheckBoxChecked;
		m_customDialogResult = messageBoxForm.CustomDialogResult;
		return result;
	}

	public delegate void MessageBoxShownDelegate(object sender, EventArgs e);
	private MessageBoxShownDelegate _OnMessageBoxShownEvent;

	public event MessageBoxShownDelegate OnMessageBoxShownEvent
	{
		add { _OnMessageBoxShownEvent += value; }
		remove { _OnMessageBoxShownEvent -= value; }
	}

	private void OnMessageBoxShown(object sender, EventArgs e)
	{
		// Tracer.Trace(GetType(), "OnMessageBoxShown()");
		_OnMessageBoxShownEvent?.Invoke(sender, e);
	}

	private void OnCopyToClipboardEventInternal(object sender, CopyToClipboardEventArgs e)
	{
		CopyToClipboardEvent?.Invoke(this, e);
	}

	public static string GetMessageText(Exception exception)
	{
		using AdvancedMessageBox messageBoxForm = new AdvancedMessageBox();
		messageBoxForm.ExMessage = exception;
		return messageBoxForm.BuildMessageText(isForEmail: false, isInternal: false);
	}


	public static DialogResult ShowEx(string message, string caption)
	{
		return ShowEx(null, message, caption, null, MessageBoxButtons.OK, MessageBoxIcon.Hand, null, -1);
	}


	public static DialogResult ShowEx(string message, string caption, MessageBoxButtons buttons)
	{
		return ShowEx(null, message, caption, null, buttons, MessageBoxIcon.Hand, null, -1);
	}




	// ShowExceptionInDialog
	public static void ShowEx(string message, Exception ex)
	{
		ShowEx(ex, string.Empty, null, null, MessageBoxButtons.OK, MessageBoxIcon.Hand, null, -1);

		// VS.SafeShowMessageBox(null, string.Format(CultureInfo.CurrentCulture, "{0} {1}", message, e.Message), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
	}

	public static DialogResult ShowEx(string text, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return ShowEx(null, text, null, null, buttons, icon, null, -1);
	}


	public static DialogResult ShowEx(string text, string title, MessageBoxButtons buttons,
		MessageBoxIcon icon, MessageBoxShownDelegate messageBoxShownDelegate = null)
	{
		return ShowEx(null, text, title, null, buttons, icon, null, -1, messageBoxShownDelegate);
	}

	public static DialogResult ShowEx(string text, string title, string helpKeyword,
		MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return ShowEx(null, text, title, helpKeyword, buttons, icon, null, -1);
	}

	public static DialogResult ShowEx(Exception ex, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return ShowEx(ex, string.Empty, null, null, buttons, icon, null, -1);
	}


	public static DialogResult ShowEx(Exception ex, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return ShowEx(ex, string.Empty, caption, null, buttons, icon, null, -1);
	}

	public static DialogResult ShowEx(Exception ex, string text, string caption, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return ShowEx(ex, text, caption, null, buttons, icon, null, -1);
	}


	public static DialogResult ShowEx(Exception ex, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner)
	{
		return ShowEx(ex, string.Empty, caption, null, buttons, icon, owner, -1);
	}

	public static DialogResult ShowEx(Exception ex, string text, string caption, string helpLink, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner, int exceptionLevel, MessageBoxShownDelegate messageBoxShownDelegate = null)
	{
		if (caption == null || caption == "")
		{
			caption = ApplicationTitle;
		}

		EnMessageBoxSymbol symbol = EnMessageBoxSymbol.Information;
		switch (icon)
		{
			case MessageBoxIcon.None:
				symbol = EnMessageBoxSymbol.None;
				break;
			case MessageBoxIcon.Hand:
				symbol = EnMessageBoxSymbol.Error;
				break;
			case MessageBoxIcon.Question:
				symbol = EnMessageBoxSymbol.Question;
				break;
			case MessageBoxIcon.Exclamation:
				symbol = EnMessageBoxSymbol.Warning;
				break;
			case MessageBoxIcon.Asterisk:
				symbol = EnMessageBoxSymbol.Information;
				break;
		}

		EnMessageBoxButtons buttons2 = EnMessageBoxButtons.OK;
		switch (buttons)
		{
			case MessageBoxButtons.AbortRetryIgnore:
				buttons2 = EnMessageBoxButtons.AbortRetryIgnore;
				break;
			case MessageBoxButtons.OKCancel:
				buttons2 = EnMessageBoxButtons.OKCancel;
				break;
			case MessageBoxButtons.RetryCancel:
				buttons2 = EnMessageBoxButtons.RetryCancel;
				break;
			case MessageBoxButtons.YesNo:
				buttons2 = EnMessageBoxButtons.YesNo;
				break;
			case MessageBoxButtons.YesNoCancel:
				buttons2 = EnMessageBoxButtons.YesNoCancel;
				break;
		}

		MessageCtl messageBox = new(ex, text, buttons2, symbol)
		{
			Caption = caption,
			MessageLevelDefault = exceptionLevel,
			HelpLink = helpLink
		};

		if (messageBoxShownDelegate != null)
		{
			messageBox.OnMessageBoxShownEvent += messageBoxShownDelegate;

			/*
			if (owner == null)
			{
				IVsUIShell uiShell = Package.GetGlobalService(typeof(IVsUIShell)) as IVsUIShell
					?? throw Diag.ExceptionService(typeof(IVsUIShell));

				try
				{
					___(uiShell.GetDialogOwnerHwnd(out IntPtr phwnd));

					IWin32Window win32Window = new Win32WindowWrapper(phwnd);
					owner = win32Window;
				}
				catch (Exception exw)
				{
					Diag.Dug(exw);
					throw;
				}
			}
			*/
		}


		if (buttons == MessageBoxButtons.YesNo)
			messageBox.DefaultButton = EnMessageBoxDefaultButton.Button1;

		return messageBox.Show(owner);
	}


	#endregion Methods


}
