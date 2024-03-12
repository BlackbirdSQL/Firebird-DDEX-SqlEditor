#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorExecuteTtsQueryCommand : AbstractSqlEditorCommand
{
	public SqlEditorExecuteTtsQueryCommand()
	{
		// Tracer.Trace();
	}

	public SqlEditorExecuteTtsQueryCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
		// Tracer.Trace();
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
			EditorWindow.ExecuteQuery(true);

		return VSConstants.S_OK;
	}

	protected bool ShouldRunCommand()
	{
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();

		return (qryMgrForEditor != null && !qryMgrForEditor.IsExecuting);
	}
}
