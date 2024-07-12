// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorConnectCommand
using System;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandConnect : AbstractCommand
{
	public CommandConnect()
	{
	}

	public CommandConnect(IBSqlEditorWindowPane windowPane) : base(windowPane)
	{
	}



	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked && StoredQryMgr != null && !StoredQryMgr.IsConnected)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked || StoredQryMgr == null || StoredQryMgr.IsConnected)
			return VSConstants.S_OK;

		try
		{
			// Tracer.Trace(GetType(), "HandleExec()");
			StoredQryMgr.IsConnecting = true;
			StoredQryMgr.Strategy.EnsureConnection(true);
		}
		finally
		{
			StoredQryMgr.IsConnecting = false;
		}

		return VSConstants.S_OK;
	}

}
