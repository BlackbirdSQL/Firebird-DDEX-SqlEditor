// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorChangeConnectionCommand

using System;
using System.Data;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorChangeConnectionCommand : AbstractSqlEditorCommand
{
	public SqlEditorChangeConnectionCommand()
	{
	}

	public SqlEditorChangeConnectionCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (GetQueryManager() != null)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		QueryManager qryMgr = GetQueryManager();
		IDbConnection newConnection = null;

		if (qryMgr != null)
		{
			try
			{
				newConnection = qryMgr.ConnectionStrategy.ChangeConnection(tryOpenConnection: true);
			}
			finally
			{
				if (newConnection != null)
				{
					qryMgr.IsConnecting = true;
					qryMgr.IsConnecting = false;
				}
			}
		}

		return VSConstants.S_OK;
	}

	private QueryManager GetQueryManager()
	{
		QueryManager qryMgr = GetQueryManagerForEditor();

		return qryMgr != null && (qryMgr.IsExecuting || !qryMgr.IsConnected) ? null : qryMgr;
	}

}
