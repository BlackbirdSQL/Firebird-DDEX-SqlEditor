#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Commands;


public class SqlEditorDisconnectCommand : AbstractSqlEditorCommand
{
	public SqlEditorDisconnectCommand()
	{
	}

	public SqlEditorDisconnectCommand(ISqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null && qryMgrForEditor.IsConnected && !qryMgrForEditor.IsExecuting)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null)
		{
			qryMgrForEditor.ConnectionStrategy.Connection?.Close();
			qryMgrForEditor.ConnectionStrategy.ResetConnection();
		}

		return VSConstants.S_OK;
	}
}
