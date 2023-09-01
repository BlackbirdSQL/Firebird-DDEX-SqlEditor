// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorChangeConnectionCommand

using System;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Commands;


public class SqlEditorChangeConnectionCommand : AbstractSqlEditorCommand
{
	public SqlEditorChangeConnectionCommand()
	{
	}

	public SqlEditorChangeConnectionCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		QueryExecutor queryExecutorForEditor = GetQueryExecutorForEditor();
		if (queryExecutorForEditor != null && queryExecutorForEditor.IsConnected && !queryExecutorForEditor.IsExecuting)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		QueryExecutor queryExecutorForEditor = GetQueryExecutorForEditor();
		if (queryExecutorForEditor != null)
		{
			try
			{
				queryExecutorForEditor.IsConnecting = true;
				queryExecutorForEditor.ConnectionStrategy.ChangeConnection(tryOpenConnection: true);
			}
			finally
			{
				queryExecutorForEditor.IsConnecting = false;
			}
		}

		return VSConstants.S_OK;
	}
}
