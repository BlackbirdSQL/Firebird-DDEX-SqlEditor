// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorCancelQueryCommand

using System;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;


namespace BlackbirdSql.Shared.Ctl.Commands;

public class CommandCancelQuery : AbstractCommand
{
	public CommandCancelQuery()
	{
	}

	public CommandCancelQuery(IBsTabbedEditorPane editorPane) : base(editorPane)
	{
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!CancellationLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (CancellationLocked)
			return VSConstants.S_OK;

		CachedQryMgr?.Cancel(false);

		return VSConstants.S_OK;
	}
}
