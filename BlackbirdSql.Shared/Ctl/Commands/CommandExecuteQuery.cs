// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorExecuteQueryCommand

using System;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;

namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandExecuteQuery : AbstractCommand
{

	public CommandExecuteQuery()
	{
		// Tracer.Trace();
	}

	public CommandExecuteQuery(IBsTabbedEditorWindowPane windowPane) : base(windowPane)
	{
		// Tracer.Trace();
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}


	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked)
			return VSConstants.S_OK;

		EnSqlExecutionType executionType = StoredAuxDocData.HasActualPlan
			? EnSqlExecutionType.QueryWithPlan : EnSqlExecutionType.QueryOnly;

		// Tracer.Trace(GetType(), "OnExec()", "ExecutionType: {0}.", executionType);

		// ----------------------------------------------------------------------------------- //
		// ******************** Execution Point (0) - CommandExecuteQuery.OnExec() ******************** //
		// ----------------------------------------------------------------------------------- //
		WindowPane.AsyncExecuteQuery(executionType);

		return VSConstants.S_OK;
	}

}
