// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorDisconnectAllQueriesCommand

using System;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandDisconnectAllQueries : AbstractCommand
{

	public CommandDisconnectAllQueries()
	{
	}

	public CommandDisconnectAllQueries(IBsTabbedEditorWindowPane windowPane) : base(windowPane)
	{
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (StoredQryMgr != null)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		foreach (AuxilliaryDocData value in ((IBsEditorPackage)ApcManager.PackageInstance).AuxilliaryDocDataTable.Values)
		{
			QueryManager qryMgr = value.QryMgr;

			if (qryMgr != null && qryMgr.IsConnected)
			{
				if (qryMgr.IsExecuting || qryMgr.HasTransactions)
				{
					value.RequestDeactivateQuery();
				}
				else
				{
					qryMgr.Disconnect();
				}
			}
		}

		return VSConstants.S_OK;
	}
}
