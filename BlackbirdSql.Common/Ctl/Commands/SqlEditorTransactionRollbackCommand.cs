#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorTransactionRollbackCommand : AbstractSqlEditorCommand
{
	public SqlEditorTransactionRollbackCommand()
	{
	}

	public SqlEditorTransactionRollbackCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked && StoredQryMgr != null && StoredQryMgr.IsConnected
			&& StoredQryMgr.ConnectionStrategy != null && StoredQryMgr.ConnectionStrategy.HasTransactions)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxDocData?.RollbackTransactions();

		return VSConstants.S_OK;

	}
}
