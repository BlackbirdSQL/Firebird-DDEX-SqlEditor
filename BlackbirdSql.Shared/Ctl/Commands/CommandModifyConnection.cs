// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorChangeConnectionCommand

using System;
using System.Data;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandModifyConnection : AbstractCommand
{

	public CommandModifyConnection()

	{
	}

	public CommandModifyConnection(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
	}



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked && StoredQryMgr != null && StoredQryMgr.Strategy != null
			&& StoredQryMgr.Strategy.Connection != null)
		{
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked || StoredQryMgr == null || StoredQryMgr.Strategy == null
			&& StoredQryMgr.Strategy.Connection == null)
			return VSConstants.S_OK;

		if (!CanDisposeTransaction(ControlsResources.ErrModifyConnectionCaption))
			return VSConstants.S_OK;

		IDbConnection newConnection = null;

		try
		{
			newConnection = StoredQryMgr.Strategy.ModifyConnection(true);
		}
		finally
		{
			if (newConnection != null)
			{
				StoredQryMgr.IsConnecting = true;
				StoredQryMgr.IsConnecting = false;
			}
			StoredQryMgr?.GetUpdateTransactionsStatus();
		}

		return VSConstants.S_OK;
	}

}
