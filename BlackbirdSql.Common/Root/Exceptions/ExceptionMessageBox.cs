#region Assembly Microsoft.SqlServer.Data.Tools.ExceptionMessageBox, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// C:\Program Files\Microsoft Visual Studio\2022\Enterprise\Common7\IDE\Extensions\Microsoft\SQLDB\Microsoft.SqlServer.Data.Tools.ExceptionMessageBox.dll
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Collections;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Events;

using Microsoft.Win32;
using BlackbirdSql.Common.Enums;

namespace BlackbirdSql.Common.Exceptions
{
	[ComVisible(false)]
	public class ExceptionMessageBox
	{
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

		private EnExceptionMessageBoxButtons m_buttons;

		private EnExceptionMessageBoxSymbol m_symbol = EnExceptionMessageBoxSymbol.Warning;

		private EnExceptionMessageBoxDefaultButton m_defaultButton;

		private EnExceptionMessageBoxOptions m_options;

		private EnExceptionMessageBoxDialogResult m_customDialogResult;

		// private const EnExceptionMessageBoxOptions InvalidOptionsMask = ~(EnExceptionMessageBoxOptions.RightAlign | EnExceptionMessageBoxOptions.RtlReading);

		public Exception Message
		{
			get
			{
				return m_message;
			}
			set
			{
				m_message = value;
			}
		}

		public string Caption
		{
			get
			{
				return m_caption;
			}
			set
			{
				m_caption = value;
			}
		}

		public string Text
		{
			get
			{
				return m_text;
			}
			set
			{
				m_text = value;
			}
		}

		public string HelpLink
		{
			get
			{
				return m_helpLink;
			}
			set
			{
				m_helpLink = value;
			}
		}

		public IDictionary Data => m_exData.Data;

		public Exception InnerException
		{
			get
			{
				return m_innerException;
			}
			set
			{
				m_innerException = value;
			}
		}

		public EnExceptionMessageBoxButtons Buttons
		{
			get
			{
				return m_buttons;
			}
			set
			{
				m_buttons = value;
			}
		}

		public EnExceptionMessageBoxSymbol Symbol
		{
			get
			{
				return m_symbol;
			}
			set
			{
				m_symbol = value;
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

		public EnExceptionMessageBoxDefaultButton DefaultButton
		{
			get
			{
				return m_defaultButton;
			}
			set
			{
				m_defaultButton = value;
			}
		}

		public EnExceptionMessageBoxOptions Options
		{
			get
			{
				return m_options;
			}
			set
			{
				m_options = value;
			}
		}

		public int MessageLevelDefault
		{
			get
			{
				return m_messageLevelCount;
			}
			set
			{
				m_messageLevelCount = value;
			}
		}

		public bool ShowToolBar
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

		public bool UseOwnerFont
		{
			get
			{
				return m_useOwnerFont;
			}
			set
			{
				m_useOwnerFont = value;
			}
		}

		public Font Font
		{
			get
			{
				return m_font;
			}
			set
			{
				m_font = value;
				m_useOwnerFont = false;
			}
		}

		public bool ShowCheckBox
		{
			get
			{
				return m_showCheckBox;
			}
			set
			{
				m_showCheckBox = value;
			}
		}

		public bool IsCheckBoxChecked
		{
			get
			{
				return m_isCheckBoxChecked;
			}
			set
			{
				m_isCheckBoxChecked = value;
			}
		}

		public string CheckBoxText
		{
			get
			{
				return m_checkBoxText;
			}
			set
			{
				m_checkBoxText = value;
			}
		}

		public RegistryKey CheckBoxRegistryKey
		{
			get
			{
				return m_checkboxRegistryKey;
			}
			set
			{
				m_checkboxRegistryKey = value;
			}
		}

		public string CheckBoxRegistryValue
		{
			get
			{
				return m_checkboxRegistryValue;
			}
			set
			{
				m_checkboxRegistryValue = value;
			}
		}

		public bool CheckBoxRegistryMeansDoNotShowDialog
		{
			get
			{
				return m_CheckBoxRegistryMeansDoNotShowDialog;
			}
			set
			{
				m_CheckBoxRegistryMeansDoNotShowDialog = value;
			}
		}

		public DialogResult DefaultDialogResult
		{
			get
			{
				return m_defaultDialogResult;
			}
			set
			{
				m_defaultDialogResult = value;
			}
		}

		public EnExceptionMessageBoxDialogResult CustomDialogResult => m_customDialogResult;

		public static string OKButtonText => ExceptionsResources.OKButton;

		public static string CancelButtonText => ExceptionsResources.CancelButton;

		public static string YesButtonText => ExceptionsResources.YesButton;

		public static string NoButtonText => ExceptionsResources.NoButton;

		public static string AbortButtonText => ExceptionsResources.AbortButton;

		public static string RetryButtonText => ExceptionsResources.RetryButton;

		public static string FailButtonText => ExceptionsResources.FailButton;

		public static string IgnoreButtonText => ExceptionsResources.IgnoreButton;

		public bool Beep
		{
			get
			{
				return m_beep;
			}
			set
			{
				m_beep = value;
			}
		}

		public event CopyToClipboardEventHandler OnCopyToClipboard;

		public ExceptionMessageBox()
		{
		}

		public ExceptionMessageBox(Exception exception)
		{
			m_message = exception;
		}

		public ExceptionMessageBox(string text)
		{
			m_text = text;
		}

		public ExceptionMessageBox(string text, string caption)
		{
			m_text = text;
			m_caption = caption;
		}

		public ExceptionMessageBox(Exception exception, EnExceptionMessageBoxButtons buttons)
		{
			m_message = exception;
			m_buttons = buttons;
		}

		public ExceptionMessageBox(string text, string caption, EnExceptionMessageBoxButtons buttons)
		{
			m_text = text;
			m_caption = caption;
			m_buttons = buttons;
		}

		public ExceptionMessageBox(Exception exception, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol)
		{
			m_message = exception;
			m_buttons = buttons;
			m_symbol = symbol;
		}

		public ExceptionMessageBox(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol)
		{
			m_text = text;
			m_caption = caption;
			m_buttons = buttons;
			m_symbol = symbol;
		}

		public ExceptionMessageBox(Exception exception, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, EnExceptionMessageBoxDefaultButton defaultButton)
		{
			m_message = exception;
			m_buttons = buttons;
			m_symbol = symbol;
			m_defaultButton = defaultButton;
		}

		public ExceptionMessageBox(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, EnExceptionMessageBoxDefaultButton defaultButton)
		{
			m_text = text;
			m_caption = caption;
			m_buttons = buttons;
			m_symbol = symbol;
			m_defaultButton = defaultButton;
		}

		public ExceptionMessageBox(Exception exception, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, EnExceptionMessageBoxDefaultButton defaultButton, EnExceptionMessageBoxOptions options)
		{
			m_message = exception;
			m_buttons = buttons;
			m_symbol = symbol;
			m_defaultButton = defaultButton;
			m_options = options;
		}

		public ExceptionMessageBox(string text, string caption, EnExceptionMessageBoxButtons buttons, EnExceptionMessageBoxSymbol symbol, EnExceptionMessageBoxDefaultButton defaultButton, EnExceptionMessageBoxOptions options)
		{
			m_text = text;
			m_caption = caption;
			m_buttons = buttons;
			m_symbol = symbol;
			m_defaultButton = defaultButton;
			m_options = options;
		}

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

		public DialogResult Show(IntPtr hwnd, string message, string source, string sourceAppName, string sourceAppVersion, string sourceModule, string sourceMessageId, string sourceLanguage)
		{
			ExceptionMessageBoxParent owner = new ExceptionMessageBoxParent(hwnd);
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

			if (m_buttons < EnExceptionMessageBoxButtons.OK || m_buttons > EnExceptionMessageBoxButtons.Custom)
			{
				InvalidEnumArgumentException ex = new("Buttons", (int)m_buttons, typeof(EnExceptionMessageBoxButtons));
				Diag.Dug(ex);
				throw ex;
			}

			if (m_symbol < EnExceptionMessageBoxSymbol.None || m_symbol > EnExceptionMessageBoxSymbol.Hand)
			{
				InvalidEnumArgumentException ex = new("Symbol", (int)m_symbol, typeof(EnExceptionMessageBoxSymbol));
				Diag.Dug(ex);
				throw ex;
			}

			if (m_defaultButton < EnExceptionMessageBoxDefaultButton.Button1 || m_defaultButton > EnExceptionMessageBoxDefaultButton.Button5)
			{
				InvalidEnumArgumentException ex = new("DefaultButton", (int)m_defaultButton, typeof(EnExceptionMessageBoxDefaultButton));
				Diag.Dug(ex);
				throw ex;
			}

			if (((uint)m_options & 0xFFFFFFFCu) != 0)
			{
				InvalidEnumArgumentException ex = new("Options", (int)m_options, typeof(EnExceptionMessageBoxOptions));
				Diag.Dug(ex);
				throw ex;
			}

			if (m_buttons == EnExceptionMessageBoxButtons.Custom && m_buttonCount == 0)
			{
				Exception ex = new(ExceptionsResources.CustomButtonTextError);
				Diag.Dug(ex);
				throw ex;
			}

			if (m_messageLevelCount != -1 && m_messageLevelCount < 1)
			{
				ArgumentOutOfRangeException ex = new("MessageLevelDefault", m_messageLevelCount, ExceptionsResources.MessageLevelCountError);
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

			using ExceptionMessageBoxForm exceptionMessageBoxForm = new ExceptionMessageBoxForm();
			exceptionMessageBoxForm.SetButtonText(m_buttonTextArray);
			exceptionMessageBoxForm.Buttons = m_buttons;
			exceptionMessageBoxForm.Caption = m_caption;
			exceptionMessageBoxForm.Message = m_message;
			exceptionMessageBoxForm.Symbol = m_symbol;
			exceptionMessageBoxForm.DefaultButton = m_defaultButton;
			exceptionMessageBoxForm.Options = m_options;
			exceptionMessageBoxForm.DoBeep = m_beep;
			exceptionMessageBoxForm.CheckBoxText = m_checkBoxText;
			exceptionMessageBoxForm.IsCheckBoxChecked = m_isCheckBoxChecked;
			exceptionMessageBoxForm.ShowCheckBox = m_showCheckBox;
			exceptionMessageBoxForm.MessageLevelCount = m_messageLevelCount;
			exceptionMessageBoxForm.ShowHelpButton = m_showHelpButton;
			exceptionMessageBoxForm.OnCopyToClipboardInternal += OnCopyToClipboardEventInternal;
			if (m_customSymbol != null)
			{
				exceptionMessageBoxForm.CustomSymbol = m_customSymbol;
			}

			if (m_font != null)
			{
				exceptionMessageBoxForm.Font = m_font;
			}

			if (owner == null)
			{
				exceptionMessageBoxForm.StartPosition = FormStartPosition.CenterScreen;
				exceptionMessageBoxForm.ShowInTaskbar = true;
			}
			else
			{
				exceptionMessageBoxForm.StartPosition = FormStartPosition.CenterParent;
			}

			exceptionMessageBoxForm.Shown += delegate
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
			exceptionMessageBoxForm.FormClosed += delegate
			{
				Trace.TraceInformation("ExceptionMessageBoxClosed@" + m_message.Message);
			};
			exceptionMessageBoxForm.PrepareToShow();
			DialogResult result = exceptionMessageBoxForm.ShowDialog(owner);
			if (exceptionMessageBoxForm.ShowCheckBox && m_checkboxRegistryKey != null)
			{
				m_checkboxRegistryKey.SetValue(m_checkboxRegistryValue, exceptionMessageBoxForm.IsCheckBoxChecked ? 1 : 0);
			}

			m_isCheckBoxChecked = exceptionMessageBoxForm.IsCheckBoxChecked;
			m_customDialogResult = exceptionMessageBoxForm.CustomDialogResult;
			return result;
		}

		private void OnCopyToClipboardEventInternal(object sender, CopyToClipboardEventArgs e)
		{
			OnCopyToClipboard?.Invoke(this, e);
		}

		public static string GetMessageText(Exception exception)
		{
			using ExceptionMessageBoxForm exceptionMessageBoxForm = new ExceptionMessageBoxForm();
			exceptionMessageBoxForm.Message = exception;
			return exceptionMessageBoxForm.BuildMessageText(isForEmail: false, isInternal: false);
		}
	}
}
