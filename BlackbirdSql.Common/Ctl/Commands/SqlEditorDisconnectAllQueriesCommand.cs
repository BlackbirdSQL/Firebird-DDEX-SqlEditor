#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorDisconnectAllQueriesCommand : AbstractSqlEditorCommand
{
	public SqlEditorDisconnectAllQueriesCommand()
	{
	}

	public SqlEditorDisconnectAllQueriesCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (GetQueryExecutorForEditor() != null)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		foreach (AuxiliaryDocData value in ((IBEditorPackage)Controller.Instance.DdexPackage).DocDataEditors.Values)
		{
			QueryExecutor queryExecutor = value.QueryExecutor;
			if (queryExecutor != null && queryExecutor.IsConnected)
			{
				if (queryExecutor.IsExecuting)
				{
					AbstractEditorEventsManager.ShouldStopClose(value, GetType());
				}
				else
				{
					queryExecutor.ConnectionStrategy.ResetConnection();
				}
			}
		}

		return VSConstants.S_OK;
	}
}
