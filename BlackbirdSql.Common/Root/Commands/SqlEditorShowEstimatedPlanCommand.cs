#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Commands;


public class SqlEditorShowEstimatedPlanCommand : AbstractSqlEditorCommand
{
	public SqlEditorShowEstimatedPlanCommand()
	{
	}

	public SqlEditorShowEstimatedPlanCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		if (!IsEditorExecutingOrDebugging())
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor != null)
		{
			QueryExecutor queryExecutor = auxiliaryDocDataForEditor.QueryExecutor;
			if (queryExecutor != null && !queryExecutor.IsExecuting)
			{
				try
				{
					auxiliaryDocDataForEditor.EstimatedExecutionPlanEnabled = true;
					Editor.ExecuteQuery();
				}
				finally
				{
					auxiliaryDocDataForEditor.EstimatedExecutionPlanEnabled = false;
				}
			}
		}

		return VSConstants.S_OK;
	}
}
