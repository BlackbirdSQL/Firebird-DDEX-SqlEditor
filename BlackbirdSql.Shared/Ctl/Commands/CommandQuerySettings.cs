// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorQueryOptionsCommand

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandQuerySettings(IBsTabbedEditorWindowPane editorWindow)

	: AbstractCommand(editorWindow)
{
	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked || StoredAuxDocData == null)
			return VSConstants.S_OK;


		IBsEditorPackage editorPackage = (IBsEditorPackage)ApcManager.PackageInstance;

		editorPackage.ShowExecutionSettingsDialogFrame(StoredAuxDocData, FormStartPosition.CenterParent);

		return VSConstants.S_OK;
	}
}
