#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Ctl.QueryExecution;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class SqlEditorDisconnectAllQueriesCommand : AbstractSqlEditorCommand
{
	public SqlEditorDisconnectAllQueriesCommand()
	{
	}

	public SqlEditorDisconnectAllQueriesCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (QryMgr != null)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		foreach (AuxilliaryDocData value in ((IBEditorPackage)ApcManager.PackageInstance).AuxilliaryDocDataTable.Values)
		{
			QueryManager qryMgr = value.QryMgr;

			if (qryMgr != null && qryMgr.IsConnected)
			{
				if (qryMgr.IsExecuting)
				{
					Cmd.ShouldStopCloseDialog(value, GetType());
				}
				else
				{
					qryMgr.ConnectionStrategy.Transaction?.Dispose();
					qryMgr.ConnectionStrategy.Transaction = null;
					qryMgr.ConnectionStrategy.Connection?.Close();
					qryMgr.ConnectionStrategy.ResetConnection();
				}
			}
		}

		return VSConstants.S_OK;
	}
}
