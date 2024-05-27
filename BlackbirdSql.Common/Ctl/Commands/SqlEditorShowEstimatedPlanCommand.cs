#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Sys;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorShowEstimatedPlanCommand : AbstractSqlEditorCommand
{
	public SqlEditorShowEstimatedPlanCommand()
	{
	}

	public SqlEditorShowEstimatedPlanCommand(IBSqlEditorWindowPane editorWindow)
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


		if (AuxDocData.TtsEnabled && QryMgr != null && StoredQryMgr.ConnectionStrategy != null)
			StoredQryMgr.ConnectionStrategy.DisposeTransaction();

		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "HandleExec", "calling ISqlEditorWindowPane.HandleExec");
		EditorWindow.ExecuteQuery(EnSqlExecutionType.PlanOnly);


		return VSConstants.S_OK;
	}
}
