// Microsoft.VisualStudio.Data.Tools.Design.Common, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.Design.Common.CommonVsUtilities
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;
using Microsoft.VisualStudio.Shell;
using Microsoft.VisualStudio.Shell.Interop;



namespace BlackbirdSql.Common.Ctl;

public static class CommonVsUtilities
{

	public enum EnDocumentsFlag
	{
		DirtyDocuments,
		DirtyOrPrimary,
		DirtyExceptPrimary,
		AllDocuments
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
		Diag.ThrowIfNotOnUIThread();

		if (docData is IVsPersistDocData vsPersistDocData)
		{
			Native.ThrowOnFailure(vsPersistDocData.IsDocDataDirty(out var pfDirty));
			return pfDirty != 0;
		}
		return false;
	}

	public static bool TryGetDocDataFromCookie(uint cookie, out object docData)
	{
		Diag.ThrowIfNotOnUIThread();

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
