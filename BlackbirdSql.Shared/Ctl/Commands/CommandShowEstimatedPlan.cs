#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

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

	public CommandShowEstimatedPlan(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
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

		if (!CanDisposeTransaction(ControlsResources.ErrExecutionPlanCaption))
			return VSConstants.S_OK;


		if (QryMgr != null && StoredQryMgr.Strategy != null)
			StoredQryMgr.Strategy.Transaction = null;

		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "HandleExec", "calling ISqlEditorWindowPane.HandleExec");
		WindowPane.AsyncExecuteQuery(EnSqlExecutionType.PlanOnly);


		return VSConstants.S_OK;
	}
}
