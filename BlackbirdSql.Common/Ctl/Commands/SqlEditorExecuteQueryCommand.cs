#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Ctl.Interfaces;

namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorExecuteQueryCommand : AbstractSqlEditorCommand
{
	public SqlEditorExecuteQueryCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorExecuteQueryCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
		// Diag.Trace();
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		if (ShouldRunCommand())
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ShouldRunCommand())
		{
			EditorWindow.ExecuteQuery();
		}

		return VSConstants.S_OK;
	}

	protected bool ShouldRunCommand()
	{
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null && !qryMgrForEditor.IsExecuting && (!qryMgrForEditor.IsDebugging || qryMgrForEditor.IsDebugging && qryMgrForEditor.IsAllowedToExecuteWhileDebugging))
		{
			return true;
		}

		return false;
	}
}
