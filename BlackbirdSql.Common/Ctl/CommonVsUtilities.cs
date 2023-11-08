// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.CommonVsUtilities
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Ctl;

[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD010:Invoke single-threaded types on Main thread",
	Justification = "UIThread compliance is performed by applicable methods.")]

public static class CommonVsUtilities
{
	private delegate DialogResult SafeShowMessageBox(string title, string text, string helpKeyword, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon);

	public enum EnDocumentsFlag
	{
		DirtyDocuments,
		DirtyOrPrimary,
		DirtyExceptPrimary,
		AllDocuments
	}

	private static Control _marshalingControl;

	public static DialogResult ShowMessageBoxEx(string title, string text, string helpKeyword, MessageBoxButtons buttons, MessageBoxDefaultButton defaultButton, MessageBoxIcon icon)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		_marshalingControl ??= new Control();
		if (_marshalingControl.InvokeRequired)
		{
			return (DialogResult)_marshalingControl.Invoke(new SafeShowMessageBox(ShowMessageBoxEx), title, text, helpKeyword, defaultButton, buttons, icon);
		}
		int pnResult = 1;
		if (Package.GetGlobalService(typeof(SVsUIShell)) is IVsUIShell vsUIShell)
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

	internal static IEnumerable<uint> EnumerateOpenedDocuments(IBDesignerDocumentService designerService, __VSRDTSAVEOPTIONS rdtSaveOptions)
	{
		EnDocumentsFlag enumerateDocumentsFlag = GetDesignerDocumentFlagFromSaveOption(rdtSaveOptions);
		return EnumerateOpenedDocuments(designerService, enumerateDocumentsFlag);
	}

	internal static IEnumerable<uint> EnumerateOpenedDocuments(IBDesignerDocumentService designerService, EnDocumentsFlag flag)
	{
		foreach (uint editableDocument in designerService.GetEditableDocuments())
		{
			if (TryGetDocDataFromCookie(editableDocument, out var docData))
			{
				bool flag2 = IsDirty(docData);
				bool flag3 = editableDocument == designerService.GetPrimaryDocCookie();
				bool flag4 = false;
				switch (flag)
				{
					case EnDocumentsFlag.DirtyDocuments:
						flag4 = flag2;
						break;
					case EnDocumentsFlag.DirtyOrPrimary:
						flag4 = flag2 || flag3;
						break;
					case EnDocumentsFlag.DirtyExceptPrimary:
						flag4 = flag2 && !flag3;
						break;
					case EnDocumentsFlag.AllDocuments:
						flag4 = true;
						break;
				}
				if (flag4)
				{
					yield return editableDocument;
				}
			}
		}
	}


	internal static EnDocumentsFlag GetDesignerDocumentFlagFromSaveOption(__VSRDTSAVEOPTIONS saveOption)
	{
		if ((saveOption & __VSRDTSAVEOPTIONS.RDTSAVEOPT_ForceSave) == 0)
		{
			return EnDocumentsFlag.DirtyDocuments;
		}
		return EnDocumentsFlag.DirtyOrPrimary;
	}


	public static bool IsDirty(object docData)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		if (docData is IVsPersistDocData vsPersistDocData)
		{
			Native.ThrowOnFailure(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}
		return false;
	}

	public static bool TryGetDocDataFromCookie(uint cookie, out object docData)
	{
		if (!ThreadHelper.CheckAccess())
		{
			COMException ex = new("Not on UI thread", VSConstants.RPC_E_WRONG_THREAD);
			Diag.Dug(ex);
			throw ex;
		}

		docData = null;
		bool result = false;
		if (Package.GetGlobalService(typeof(IVsRunningDocumentTable)) is IVsRunningDocumentTable vsRunningDocumentTable)
		{
			IntPtr ppunkDocData = IntPtr.Zero;
			try
			{
				if (Native.Succeeded(vsRunningDocumentTable.GetDocumentInfo(cookie, out var _, out var _, out var _, out var _, out var _, out var _, out ppunkDocData)))
				{
					docData = Marshal.GetObjectForIUnknown(ppunkDocData);
					result = true;
				}
			}
			finally
			{
				if (ppunkDocData != IntPtr.Zero)
				{
					Marshal.Release(ppunkDocData);
				}
			}
		}
		return result;
	}
}
