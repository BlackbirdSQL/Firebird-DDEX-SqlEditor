#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Windows.Forms;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorQuerySettingsCommand(IBSqlEditorWindowPane editorWindow)

	: AbstractSqlEditorCommand(editorWindow)
{
	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (AuxDocData == null)
			return VSConstants.S_OK;


		IBEditorPackage editorPackage = (IBEditorPackage)ApcManager.PackageInstance;

		editorPackage.ShowExecutionSettingsDialogFrame(StoredAuxDocData, FormStartPosition.CenterParent);

		return VSConstants.S_OK;
	}
}
