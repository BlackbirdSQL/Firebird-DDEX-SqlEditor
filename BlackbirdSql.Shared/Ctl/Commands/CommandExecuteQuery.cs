#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

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

	public CommandExecuteQuery(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
		// Tracer.Trace();
	}



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}


	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked)
			return VSConstants.S_OK;

		EnSqlExecutionType executionType = StoredAuxDocData.HasActualPlan
			? EnSqlExecutionType.QueryWithPlan : EnSqlExecutionType.QueryOnly;

		// Tracer.Trace(GetType(), "HandleExec()", "ExecutionType: {0}.", executionType);

		// ----------------------------------------------------------------------------------- //
		// ******************** Execution Point (0) - CommandExecuteQuery.HandleExec() ******************** //
		// ----------------------------------------------------------------------------------- //
		WindowPane.AsyncExecuteQuery(executionType);

		return VSConstants.S_OK;
	}

}
