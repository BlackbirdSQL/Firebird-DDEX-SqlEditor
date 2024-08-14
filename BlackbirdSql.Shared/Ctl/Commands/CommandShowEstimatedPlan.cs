// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorShowEstimatedPlanCommand

using System;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandShowEstimatedPlan : AbstractCommand
{
	public CommandShowEstimatedPlan()
	{
	}

	public CommandShowEstimatedPlan(IBsTabbedEditorPane editorWindow)
		: base(editorWindow)
	{
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
		if (ExecutionLocked || !RequestDeactivateQuery(Resources.MsgQueryAbort_UncommittedTransactions))
			return VSConstants.S_OK;


		CachedStrategy?.DisposeTransaction();

		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "OnExec", "calling ISqlEditorWindowPane.OnExec");
		EditorPane.AsyncExecuteQuery(EnSqlExecutionType.PlanOnly);


		return VSConstants.S_OK;
	}
}
