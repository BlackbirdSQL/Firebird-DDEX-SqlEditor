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


public class SqlEditorToggleSqlCmdModeCommand : AbstractSqlEditorCommand
{
	public SqlEditorToggleSqlCmdModeCommand()
	{
	}

	public SqlEditorToggleSqlCmdModeCommand(ISqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null)
		{
			if (!IsEditorExecutingOrDebugging())
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}

			if (qryMgrForEditor.IsWithOleSQLScripting)
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
			}
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();
		if (qryMgrForEditor != null)
		{
			qryMgrForEditor.IsWithOleSQLScripting = !qryMgrForEditor.IsWithOleSQLScripting;
		}

		return VSConstants.S_OK;
	}
}
