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

	public CommandShowEstimatedPlan(IBsTabbedEditorWindowPane editorWindow)
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
		if (ExecutionLocked || !CanDisposeTransaction(Resources.ExExecutionPlanCaption))
			return VSConstants.S_OK;


		if (StoredQryMgr != null && StoredQryMgr.Strategy != null)
			StoredQryMgr.Strategy.DisposeTransaction(true);

		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "OnExec", "calling ISqlEditorWindowPane.OnExec");
		WindowPane.AsyncExecuteQuery(EnSqlExecutionType.PlanOnly);


		return VSConstants.S_OK;
	}
}
