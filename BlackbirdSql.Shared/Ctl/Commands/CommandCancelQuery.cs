#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

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

	public CommandCancelQuery(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
	}



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!CancellationLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (CancellationLocked)
			return VSConstants.S_OK;

		StoredQryMgr?.Cancel(synchronous: false);

		return VSConstants.S_OK;
	}
}
