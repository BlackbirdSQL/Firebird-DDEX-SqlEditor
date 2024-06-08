// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.ExceptionMessageBox

using System;
using System.Collections;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Sys.Controls;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Properties;
using Microsoft.Win32;



namespace BlackbirdSql;

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
		_ExMessage = exception;
	}

	public MessageCtl(string text)
	{
		_Text = text;
	}

	public MessageCtl(Exception exception, string text)
	{
		_Text = text;
		_ExMessage = exception;
	}

	public MessageCtl(string text, string caption)
	{
		_Text = text;
		_Caption = caption;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons)
	{
		_ExMessage = exception;
		_Buttons = buttons;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons)
	{
		_Text = text;
		_Caption = caption;
		_Buttons = buttons;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol)
	{
		_ExMessage = exception;
		_Buttons = buttons;
		_Symbol = symbol;
	}


	public MessageCtl(Exception exception, string text, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol)
	{
		_Text = text;
		_ExMessage = exception;
		_Buttons = buttons;
		_Symbol = symbol;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol)
	{
		_Text = text;
		_Caption = caption;
		_Buttons = buttons;
		_Symbol = symbol;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton)
	{
		_ExMessage = exception;
		_Buttons = buttons;
		_Symbol = symbol;
		_DefaultButton = defaultButton;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton)
	{
		_Text = text;
		_Caption = caption;
		_Buttons = buttons;
		_Symbol = symbol;
		_DefaultButton = defaultButton;
	}

	public MessageCtl(Exception exception, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton, EnMessageBoxOptions options)
	{
		_ExMessage = exception;
		_Buttons = buttons;
		_Symbol = symbol;
		_DefaultButton = defaultButton;
		_Options = options;
	}

	public MessageCtl(string text, string caption, EnMessageBoxButtons buttons, EnMessageBoxSymbol symbol, EnMessageBoxDefaultButton defaultButton, EnMessageBoxOptions options)
	{
		_Text = text;
		_Caption = caption;
		_Buttons = buttons;
		_Symbol = symbol;
		_DefaultButton = defaultButton;
		_Options = options;
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
	private readonly string[] _ButtonTextArray = new string[5];
	private int _ButtonCount;
	private DialogResult _DefaultDialogResult = DialogResult.OK;
	private string _CheckBoxRegistryValue = string.Empty;
	private bool _CheckBoxRegistryMeansDoNotShowDialog = true;
	private RegistryKey _CheckBoxRegistryKey;
	private bool _UseOwnerFont;
	private string _Text = string.Empty;
	private string _HelpLink = string.Empty;
	private readonly Exception _ExContainer = new ApplicationException();
	private Exception _InnerException;
	private bool _Beep = true;
	private bool _ShowToolBar = true;
	private bool _ShowCheckBox;
	private bool _IsCheckBoxChecked;
	private string _Caption = string.Empty;
	private string _CheckBoxText = string.Empty;
	private int _MessageLevelCount = -1;
	private Bitmap _CustomSymbol;
	private Font _MessageFont;
	private Exception _ExMessage;
	private EnMessageBoxButtons _Buttons;
	private EnMessageBoxSymbol _Symbol = EnMessageBoxSymbol.Warning;
	private EnMessageBoxDefaultButton _DefaultButton;
	private EnMessageBoxOptions _Options;
	private EnMessageBoxDialogResult _CustomDialogResult;


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

	public Exception ExMessage
	{
		get { return _ExMessage; }
		set { _ExMessage = value; }
	}

	public string Caption
	{
		get { return _Caption; }
		set { _Caption = value; }
	}

	public string Text
	{
		get { return _Text; }
		set { _Text = value; }
	}

	public string HelpLink
	{
		get { return _HelpLink; }
		set { _HelpLink = value; }
	}

	public IDictionary Data => _ExContainer.Data;

	public Exception InnerException
	{
		get { return _InnerException; }
		set { _InnerException = value; }
	}

	public EnMessageBoxButtons Buttons
	{
		get { return _Buttons; }
		set { _Buttons = value; }
	}

	public EnMessageBoxSymbol Symbol
	{
		get { return _Symbol; }
		set { _Symbol = value; }
	}

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

	public EnMessageBoxOptions Options
	{
		get { return _Options; }
		set { _Options = value; }
	}

	public int MessageLevelDefault
	{
		get { return _MessageLevelCount; }
		set { _MessageLevelCount = value; }
	}

	public bool ShowToolBar
	{
		get { return _ShowToolBar; }
		set { _ShowToolBar = value; }
	}

	public bool UseOwnerFont
	{
		get { return _UseOwnerFont; }
		set { _UseOwnerFont = value; }
	}

	public Font MessageFont
	{
		get { return _MessageFont; }
		set { _MessageFont = value; _UseOwnerFont = false; }
	}

	public bool ShowCheckBox
	{
		get { return _ShowCheckBox; }
		set { _ShowCheckBox = value; }
	}

	public bool IsCheckBoxChecked
	{
		get { return _IsCheckBoxChecked; }
		set { _IsCheckBoxChecked = value; }
	}

	public string CheckBoxText
	{
		get { return _CheckBoxText; }
		set { _CheckBoxText = value; }
	}

	public RegistryKey CheckBoxRegistryKey
	{
		get { return _CheckBoxRegistryKey; }
		set { _CheckBoxRegistryKey = value; }
	}

	public string CheckBoxRegistryValue
	{
		get { return _CheckBoxRegistryValue; }
		set { _CheckBoxRegistryValue = value; }
	}

	public bool CheckBoxRegistryMeansDoNotShowDialog
	{
		get { return _CheckBoxRegistryMeansDoNotShowDialog; }
		set { _CheckBoxRegistryMeansDoNotShowDialog = value; }
	}

	public DialogResult DefaultDialogResult
	{
		get { return _DefaultDialogResult; }
		set { _DefaultDialogResult = value; }
	}

	public EnMessageBoxDialogResult CustomDialogResult => _CustomDialogResult;

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
		get { return _Beep; }
		set { _Beep = value; }
	}

	public event CopyToClipboardEventHandler CopyToClipboardEvent;


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - MessageCtl
	// =========================================================================================================



	public void SetButtonText(string button1Text, string button2Text, string button3Text, string button4Text, string button5Text)
	{
		_ButtonTextArray[0] = button1Text;
		_ButtonTextArray[1] = button2Text;
		_ButtonTextArray[2] = button3Text;
		_ButtonTextArray[3] = button4Text;
		_ButtonTextArray[4] = button5Text;
		if (button1Text == null || button1Text.Length == 0)
		{
			_ButtonCount = 0;
		}
		else if (button2Text == null || button2Text.Length == 0)
		{
			_ButtonCount = 1;
		}
		else if (button3Text == null || button3Text.Length == 0)
		{
			_ButtonCount = 2;
		}
		else if (button4Text == null || button4Text.Length == 0)
		{
			_ButtonCount = 3;
		}
		else if (button5Text == null || button5Text.Length == 0)
		{
			_ButtonCount = 4;
		}
		else
		{
			_ButtonCount = 5;
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
		_ExMessage = new(message)
		{
			Source = source
		};
		return Show(owner);
	}

	public DialogResult Show(IWin32Window owner)
	{
		if (_ExMessage == null && (Text == null || Text.Length == 0))
		{
			ArgumentNullException ex = new("Message");
			Diag.Dug(ex);
			throw ex;
		}

		if (_Buttons < EnMessageBoxButtons.OK || _Buttons > EnMessageBoxButtons.Custom)
		{
			InvalidEnumArgumentException ex = new("Buttons", (int)_Buttons, typeof(EnMessageBoxButtons));
			Diag.Dug(ex);
			throw ex;
		}

		if (_Symbol < EnMessageBoxSymbol.None || _Symbol > EnMessageBoxSymbol.Hand)
		{
			InvalidEnumArgumentException ex = new("Symbol", (int)_Symbol, typeof(EnMessageBoxSymbol));
			Diag.Dug(ex);
			throw ex;
		}

		if (_DefaultButton < EnMessageBoxDefaultButton.Button1 || _DefaultButton > EnMessageBoxDefaultButton.Button5)
		{
			InvalidEnumArgumentException ex = new("DefaultButton", (int)_DefaultButton, typeof(EnMessageBoxDefaultButton));
			Diag.Dug(ex);
			throw ex;
		}

		if (((uint)_Options & 0xFFFFFFFCu) != 0)
		{
			InvalidEnumArgumentException ex = new("Options", (int)_Options, typeof(EnMessageBoxOptions));
			Diag.Dug(ex);
			throw ex;
		}

		if (_Buttons == EnMessageBoxButtons.Custom && _ButtonCount == 0)
		{
			Exception ex = new(ControlsResources.CustomButtonTextError);
			Diag.Dug(ex);
			throw ex;
		}

		if (_MessageLevelCount != -1 && _MessageLevelCount < 1)
		{
			ArgumentOutOfRangeException ex = new("MessageLevelDefault", _MessageLevelCount, ControlsResources.MessageLevelCountError);
			Diag.Dug(ex);
			throw ex;
		}

		bool hasError = _ExMessage != null;
		if (_ExMessage == null)
		{
			_ExMessage = new ApplicationException(_Text, _InnerException)
			{
				HelpLink = _HelpLink
			};
			foreach (DictionaryEntry datum in _ExContainer.Data)
			{
				_ExMessage.Data.Add(datum.Key, datum.Value);
			}
		}

		if (_UseOwnerFont)
		{
			try
			{
				if (owner is Form formCtl)
				{
					MessageFont = formCtl.Font;
				}
				else if (owner is UserControl control)
				{
					MessageFont = control.Font;
				}
				else if (owner is Control fontControl)
				{
					MessageFont = fontControl.Font;
				}
			}
			catch (Exception)
			{
			}
		}

		if (_ShowCheckBox && _CheckBoxRegistryKey != null)
		{
			try
			{
				_IsCheckBoxChecked = (int)_CheckBoxRegistryKey.GetValue(_CheckBoxRegistryValue, 0) != 0;
				if (_CheckBoxRegistryMeansDoNotShowDialog && _IsCheckBoxChecked)
				{
					return _DefaultDialogResult;
				}
			}
			catch (Exception)
			{
			}
		}

		if (_Caption == null || _Caption.Length == 0)
		{
			_Caption = _ExMessage.Source;
		}

		if ((_Caption == null || _Caption.Length == 0) && owner is Form form)
		{
			_Caption = form.Text;
		}

		using AdvancedMessageBox messageBoxForm = new AdvancedMessageBox();
		messageBoxForm.SetButtonText(_ButtonTextArray);
		messageBoxForm.Buttons = _Buttons;
		messageBoxForm.Caption = _Caption;
		messageBoxForm.ExMessage = _ExMessage;
		messageBoxForm.Symbol = _Symbol;
		messageBoxForm.DefaultButton = _DefaultButton;
		messageBoxForm.Options = _Options;
		messageBoxForm.DoBeep = _Beep;
		messageBoxForm.CheckBoxText = _CheckBoxText;
		messageBoxForm.IsCheckBoxChecked = _IsCheckBoxChecked;
		messageBoxForm.ShowCheckBox = _ShowCheckBox;
		messageBoxForm.MaxMessages = _MessageLevelCount;
		messageBoxForm.ShowHelpButton = _ShowToolBar;
		messageBoxForm.CopyToClipboardInternalEvent += OnCopyToClipboardEventInternal;
		if (_CustomSymbol != null)
		{
			messageBoxForm.CustomSymbol = _CustomSymbol;
		}

		if (_MessageFont != null)
		{
			messageBoxForm.Font = _MessageFont;
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
				Trace.TraceError("ExceptionMessageBoxShown@" + _ExMessage.Message);
				Trace.Write(_ExMessage);
			}
			else
			{
				Trace.TraceInformation("ExceptionMessageBoxShown@" + _ExMessage.Message);
			}
		};
		messageBoxForm.FormClosed += delegate
		{
			Trace.TraceInformation("ExceptionMessageBoxClosed@" + _ExMessage.Message);
		};
		*/

		messageBoxForm.Shown += OnMessageBoxShown;


		messageBoxForm.PrepareToShow();
		DialogResult result = messageBoxForm.ShowDialog(owner);
		if (messageBoxForm.ShowCheckBox && _CheckBoxRegistryKey != null)
		{
			_CheckBoxRegistryKey.SetValue(_CheckBoxRegistryValue, messageBoxForm.IsCheckBoxChecked ? 1 : 0);
		}

		_IsCheckBoxChecked = messageBoxForm.IsCheckBoxChecked;
		_CustomDialogResult = messageBoxForm.CustomDialogResult;
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
