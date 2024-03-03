
using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Exceptions;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;


namespace BlackbirdSql.Common;

// =========================================================================================================
//												Cmd Class
//
/// <summary>
/// Central location for implementation of utility static methods for Controls. 
/// </summary>
// =========================================================================================================

public abstract class Cmd : BlackbirdSql.Core.Cmd
{


	// ---------------------------------------------------------------------------------
	#region Constants - Cmd
	// ---------------------------------------------------------------------------------


	public const char OpenSquareBracket = '[';
	public const char DoubleQuote = '"';


	#endregion Constants





	// =========================================================================================================
	#region Fields - Cmd
	// =========================================================================================================


	private static string _AppTitle;

	private delegate DialogResult SafeShowMessageBox(string title, string text, string helpKeyword, MessageBoxButtons buttons, MessageBoxIcon icon);


	public static string ApplicationTitle
	{
		get
		{
			if (string.IsNullOrWhiteSpace(_AppTitle))
			{
				return ControlsResources.ConnectToServer;
			}

			return _AppTitle;
		}
		set
		{
			_AppTitle = value;
		}
	}



	#endregion Fields





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================




	public static System.Windows.Media.Color CombineColors(System.Windows.Media.Color c1, int a1, System.Windows.Media.Color c2, int a2)
	{
		return System.Windows.Media.Color.FromArgb((byte)((c1.A * a1 + c2.A * a2) / 100), (byte)((c1.R * a1 + c2.R * a2) / 100), (byte)((c1.G * a1 + c2.G * a2) / 100), (byte)((c1.B * a1 + c2.B * a2) / 100));
	}



	public static System.Drawing.Color ConvertColor(System.Windows.Media.Color wpfColor)
	{
		return System.Drawing.Color.FromArgb(255, wpfColor.R, wpfColor.G, wpfColor.B);
	}


	public static UIElement FindVisualParent(Type typeOfParent, UIElement element, Type stopOnType = null)
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (typeOfParent.IsAssignableFrom(uIElement.GetType()))
			{
				return uIElement;
			}
			if (stopOnType != null && stopOnType.IsAssignableFrom(uIElement.GetType()))
			{
				return null;
			}
		}
		return null;
	}

	public static T FindVisualParent<T>(UIElement element, Type stopOnType = null) where T : UIElement
	{
		for (UIElement uIElement = element; uIElement != null; uIElement = VisualTreeHelper.GetParent(uIElement) as UIElement)
		{
			if (uIElement is T result)
			{
				return result;
			}
			if (stopOnType != null && stopOnType.IsAssignableFrom(uIElement.GetType()))
			{
				return null;
			}
		}
		return null;
	}




	// IsInAutomationFunction
	public static bool IsInAutomationFunction()
	{
		Diag.ThrowIfNotOnUIThread();

		IVsExtensibility3 extensibility = VS.GetExtensibility(); 

		if (extensibility == null)
			return true;

		Native.ThrowOnFailure(extensibility.IsInAutomationFunction(out int pfInAutoFunc));

		return pfInAutoFunc != 0;
	}






	public static DialogResult ShowException(Exception e)
	{
		Diag.Dug(e);
		return DialogResult.OK;
	}



	// ShowExceptionInDialog
	public static void ShowExceptionInDialog(string message, Exception e)
	{
		ShowMessageBoxEx(null, string.Format(CultureInfo.CurrentCulture, "{0} {1}", message, e.Message), null, MessageBoxButtons.OK, MessageBoxIcon.Hand);
	}



	public static DialogResult ShowMessage(Exception ex, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner)
	{
		return ShowMessage(ex, caption, buttons, icon, owner, -1);
	}

	public static DialogResult ShowMessage(Exception ex, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner, int exceptionLevel)
	{
		if (caption == null || caption == "")
		{
			caption = ApplicationTitle;
		}

		EnExceptionMessageBoxSymbol symbol = EnExceptionMessageBoxSymbol.Information;
		switch (icon)
		{
			case MessageBoxIcon.None:
				symbol = EnExceptionMessageBoxSymbol.None;
				break;
			case MessageBoxIcon.Hand:
				symbol = EnExceptionMessageBoxSymbol.Error;
				break;
			case MessageBoxIcon.Question:
				symbol = EnExceptionMessageBoxSymbol.Question;
				break;
			case MessageBoxIcon.Exclamation:
				symbol = EnExceptionMessageBoxSymbol.Warning;
				break;
			case MessageBoxIcon.Asterisk:
				symbol = EnExceptionMessageBoxSymbol.Information;
				break;
		}

		EnExceptionMessageBoxButtons buttons2 = EnExceptionMessageBoxButtons.OK;
		switch (buttons)
		{
			case MessageBoxButtons.AbortRetryIgnore:
				buttons2 = EnExceptionMessageBoxButtons.AbortRetryIgnore;
				break;
			case MessageBoxButtons.OKCancel:
				buttons2 = EnExceptionMessageBoxButtons.OKCancel;
				break;
			case MessageBoxButtons.RetryCancel:
				buttons2 = EnExceptionMessageBoxButtons.RetryCancel;
				break;
			case MessageBoxButtons.YesNo:
				buttons2 = EnExceptionMessageBoxButtons.YesNo;
				break;
			case MessageBoxButtons.YesNoCancel:
				buttons2 = EnExceptionMessageBoxButtons.YesNoCancel;
				break;
		}

		ExceptionMessageBoxCtl exceptionMessageBox = new(ex, buttons2, symbol)
		{
			Caption = caption,
			MessageLevelDefault = exceptionLevel
		};

		if (buttons == MessageBoxButtons.YesNo)
			exceptionMessageBox.DefaultButton = EnExceptionMessageBoxDefaultButton.Button2;

		return exceptionMessageBox.Show(owner);
	}

	public static DialogResult ShowMessage(string message, string caption, MessageBoxButtons buttons, MessageBoxIcon icon, IWin32Window owner)
	{
		return ShowMessage(new Exception(message), caption, buttons, icon, owner);
	}



	// ShowMessageBox
	public static DialogResult ShowMessageBox(string title, string text, string helpKeyword)
	{
		return ShowMessageBoxEx(title, text, helpKeyword, MessageBoxButtons.OK, MessageBoxIcon.Asterisk);
	}



	// ShowMessageBoxEx
	public static DialogResult ShowMessageBoxEx(string title, string text,
		MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return ShowMessageBoxEx(title, text, string.Empty, buttons, icon);
	}

	// ShowMessageBoxEx
	public static DialogResult ShowMessageBoxEx(string title, string text,
		string helpKeyword, MessageBoxButtons buttons, MessageBoxIcon icon)
	{
		return VS.ShowMessageBoxEx(title, text, helpKeyword, buttons, MessageBoxDefaultButton.Button1, icon);
	}





	#endregion Static Methods
}
