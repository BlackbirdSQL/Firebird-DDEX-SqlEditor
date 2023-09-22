
using System;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Security;
using System.Windows;
using System.Windows.Forms;
using System.Windows.Media;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Exceptions;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Model;

using Microsoft;
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
	#region Variables - Cmd
	// =========================================================================================================


	private static string _AppTitle;
	private static Control _MarshalingControl;
	private static ResourceDictionary _SharedResources;

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



	public static ResourceDictionary SharedResources
	{
		get
		{
			return _SharedResources ??= (ResourceDictionary)System.Windows.Application.LoadComponent(
				new Uri("/Properties/SharedResources.xaml", UriKind.Relative));
		}
	}




	#endregion Variables





	// =========================================================================================================
	#region Static Methods - Cmd
	// =========================================================================================================


	// CanonicalizeDirectoryName
	public static string CanonicalizeDirectoryName(string fullPathDirName)
	{
		if (string.IsNullOrEmpty(fullPathDirName))
		{
			ArgumentNullException ex = new("fullPathDirName");
			Diag.Dug(ex);
			throw ex;
		}

		return CanonicalizeFileNameOrDirectoryImpl(fullPathDirName, pathIsDir: true);
	}


	// CanonicalizeFileNameOrDirectoryImpl
	private static string CanonicalizeFileNameOrDirectoryImpl(string path, bool pathIsDir)
	{
		if (path.StartsWith("MSSQL::", StringComparison.OrdinalIgnoreCase) || path.StartsWith("MSSQLCLR::", StringComparison.OrdinalIgnoreCase))
		{
			return path;
		}

		path = Path.GetFullPath(path);
		path = path.ToUpperInvariant();
		if (pathIsDir)
		{
			return EnsureNoBackslash(path);
		}

		return path;
	}



	public static System.Windows.Media.Color CombineColors(System.Windows.Media.Color c1, int a1, System.Windows.Media.Color c2, int a2)
	{
		return System.Windows.Media.Color.FromArgb((byte)((c1.A * a1 + c2.A * a2) / 100), (byte)((c1.R * a1 + c2.R * a2) / 100), (byte)((c1.G * a1 + c2.G * a2) / 100), (byte)((c1.B * a1 + c2.B * a2) / 100));
	}



	public static System.Drawing.Color ConvertColor(System.Windows.Media.Color wpfColor)
	{
		return System.Drawing.Color.FromArgb(255, wpfColor.R, wpfColor.G, wpfColor.B);
	}




	// EnsureNoBackslash
	public static string EnsureNoBackslash(string fullPath)
	{
		string result = fullPath;
		if (!string.IsNullOrEmpty(fullPath) && fullPath.Length > 1 && (fullPath[^1] == '\\' || fullPath[^1] == '/'))
		{
			result = fullPath[..^1];
		}

		return result;
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



	public static IVsProject3 GetMiscellaneousProject(IServiceProvider provider)
	{
		IVsExternalFilesManager obj = provider.GetService(typeof(SVsExternalFilesManager)) as IVsExternalFilesManager;
		try { Assumes.Present(obj); } catch (Exception ex) { Diag.Dug(ex); throw; }

		Native.WrapComCall(obj.GetExternalFilesProject(out IVsProject ppProject), Array.Empty<int>());

		return (IVsProject3)ppProject;
	}



	// IsInAutomationFunction
	public static bool IsInAutomationFunction()
	{
		Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
		IVsExtensibility3 extensibility = GlobalServices.Extensibility;

		if (extensibility == null)
			return true;

		Native.ThrowOnFailure(extensibility.IsInAutomationFunction(out int pfInAutoFunc));

		return pfInAutoFunc != 0;
	}



	// IsSamePath
	public static bool IsSamePath(string file1, string file2)
	{
		if (file1 == null || file1.Length == 0)
		{
			if (file2 != null)
				return file2.Length == 0;

			return true;
		}

		try
		{
			if (!Uri.TryCreate(file1, UriKind.Absolute, out var result)
				|| !Uri.TryCreate(file2, UriKind.Absolute, out var result2))
			{
				return false;
			}

			if (result != null && result.IsFile && result2 != null && result2.IsFile)
			{
				try
				{
					string strA = CanonicalizeDirectoryName(result.LocalPath);
					string strB = CanonicalizeDirectoryName(result2.LocalPath);

					return string.Compare(strA, strB, StringComparison.OrdinalIgnoreCase) == 0;
				}
				catch (PathTooLongException)
				{
					return false;
				}
				catch (ArgumentException)
				{
					return false;
				}
				catch (SecurityException)
				{
					return false;
				}
				catch (NotSupportedException)
				{
					return false;
				}
			}

			return file1 == file2;
		}
		catch (UriFormatException ex5)
		{
			SqlTracer.TraceEvent(TraceEventType.Verbose, EnSqlTraceId.CoreServices,
				string.Format(CultureInfo.CurrentCulture, "IsSamePath exception: {0}", ex5.Message));
		}

		return false;
	}



	public static void OpenAsMiscellaneousFile(IServiceProvider provider, string path, string caption,
		Guid editor, string physicalView, Guid logicalView)
	{
		try
		{
			IVsProject3 miscellaneousProject = GetMiscellaneousProject(provider);
			VSADDRESULT[] array = new VSADDRESULT[1];
			VSADDITEMOPERATION dwAddItemOperation = VSADDITEMOPERATION.VSADDITEMOP_CLONEFILE;

			uint flags = (uint)__VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_DoOpen;

			flags |= (uint)(!(editor == Guid.Empty)
				? __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseEditor
				: __VSSPECIFICEDITORFLAGS.VSSPECIFICEDITOR_UseView);

			Native.WrapComCall(miscellaneousProject.AddItemWithSpecific(grfEditorFlags: flags,
				itemidLoc: uint.MaxValue, dwAddItemOperation: dwAddItemOperation, pszItemName: caption, cFilesToOpen: 1u,
				rgpszFilesToOpen: new string[1] { path }, hwndDlgOwner: IntPtr.Zero, rguidEditorType: ref editor,
				pszPhysicalView: physicalView, rguidLogicalView: ref logicalView, pResult: array), Array.Empty<int>());

			if (array[0] != VSADDRESULT.ADDRESULT_Success)
			{
				throw new ApplicationException(array[0].ToString());
			}
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
	}



	public static void OpenNewMiscellaneousSqlFile(IServiceProvider provider, string initialContent = "")
	{
		IVsProject3 miscellaneousProject = GetMiscellaneousProject(provider);
		miscellaneousProject.GenerateUniqueItemName(VSConstants.VSITEMID_ROOT, MonikerAgent.C_SqlExtension, "SQLQuery", out string pbstrItemName);
		string tempFileName = Path.GetTempFileName();
		if (tempFileName == null)
		{
			ShowMessageBoxEx(string.Empty, ControlsResources.ErrCannotCreateTempFile, MessageBoxButtons.OK, MessageBoxIcon.Hand);
			return;
		}
		StreamWriter streamWriter = null;
		try
		{
			streamWriter = new StreamWriter(tempFileName);
			streamWriter.Write(initialContent);
			streamWriter.Flush();
			streamWriter.Close();
			streamWriter = null;
			OpenAsMiscellaneousFile(provider, tempFileName, pbstrItemName, new Guid(SystemData.DslEditorFactoryGuid),
				string.Empty, VSConstants.LOGVIEWID_Primary);
		}
		finally
		{
			streamWriter?.Close();
			File.Delete(tempFileName);
		}
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
		return ShowMessageBoxEx(title, text, helpKeyword, buttons, MessageBoxDefaultButton.Button1, icon);
	}



	// ShowMessageBoxEx
	public static DialogResult ShowMessageBoxEx(string title, string text, string helpKeyword,
		MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon)
	{
		Microsoft.VisualStudio.Shell.ThreadHelper.ThrowIfNotOnUIThread();
		_MarshalingControl ??= new Control();

		if (_MarshalingControl.InvokeRequired)
		{
			return (DialogResult)_MarshalingControl.Invoke(new SafeShowMessageBox(ShowMessageBoxEx),
				title, text, helpKeyword, buttons, icon);
		}

		int pnResult = 1;
		if (Microsoft.VisualStudio.Shell.Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell vsUIShell)
		{
			Guid rclsidComp = Guid.Empty;
			OLEMSGICON msgicon = OLEMSGICON.OLEMSGICON_INFO;
			switch (icon)
			{
				case MessageBoxIcon.Hand:
					msgicon = OLEMSGICON.OLEMSGICON_CRITICAL;
					break;
				case MessageBoxIcon.Asterisk:
					msgicon = OLEMSGICON.OLEMSGICON_INFO;
					break;
				case MessageBoxIcon.None:
					msgicon = OLEMSGICON.OLEMSGICON_NOICON;
					break;
				case MessageBoxIcon.Question:
					msgicon = OLEMSGICON.OLEMSGICON_QUERY;
					break;
				case MessageBoxIcon.Exclamation:
					msgicon = OLEMSGICON.OLEMSGICON_WARNING;
					break;
			}

			OLEMSGDEFBUTTON msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST;
			switch (defaultButton)
			{
				case MessageBoxDefaultButton.Button2:
					msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_SECOND;
					break;
				case MessageBoxDefaultButton.Button3:
					msgdefbtn = OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_THIRD;
					break;
			}

			Native.WrapComCall(vsUIShell.ShowMessageBox(0u, ref rclsidComp, title, string.IsNullOrEmpty(text) ? null : text, helpKeyword, 0u, (OLEMSGBUTTON)buttons, msgdefbtn, msgicon, 0, out pnResult));
		}

		return (DialogResult)pnResult;
	}


	#endregion Static Methods
}
