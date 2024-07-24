
using System;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandToggleTTS : AbstractCommand
{
	public CommandToggleTTS()
	{
	}

	public CommandToggleTTS(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		if (StoredAuxDocData.TtsEnabled)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked || !CanDisposeTransaction(Resources.ExDisableTtsCaption))
			return VSConstants.S_OK;

		if (StoredAuxDocData.TtsEnabled && StoredQryMgr != null && StoredQryMgr.Strategy != null)
			StoredQryMgr.Strategy.DisposeTransaction(true);

		StoredAuxDocData.TtsEnabled = !StoredAuxDocData.TtsEnabled;

		StoredQryMgr.GetUpdateTransactionsStatus(true);


		return VSConstants.S_OK;
	}
}
