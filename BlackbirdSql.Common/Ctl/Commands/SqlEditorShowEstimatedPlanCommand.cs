#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
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
			QueryManager qryMgr = auxiliaryDocDataForEditor.QryMgr;
			if (qryMgr != null && !qryMgr.IsExecuting)
			{
				try
				{
					Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "HandleExec", "calling ISqlEditorWindowPane.HandleExec");
					auxiliaryDocDataForEditor.EstimatedExecutionPlanEnabled = true;
					EditorWindow.ExecuteQuery();
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
