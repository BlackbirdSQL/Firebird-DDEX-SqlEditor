
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

	public CommandToggleTTS(IBsTabbedEditorPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		if (CachedAuxDocData.TtsEnabled)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ExecutionLocked || !RequestDisposeTts(Resources.ExDisableTtsCaption))
			return VSConstants.S_OK;

		if (CachedAuxDocData.TtsEnabled)
			CachedStrategy?.DisposeTransaction();

		CachedAuxDocData.TtsEnabled = !CachedAuxDocData.TtsEnabled;

		_ = CachedQryMgr.LiveTransactions;


		return VSConstants.S_OK;
	}
}
