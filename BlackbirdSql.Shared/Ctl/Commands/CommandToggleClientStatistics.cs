// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToggleClientStatisticsCommand

using System;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandToggleClientStatistics : AbstractCommand
{
	public CommandToggleClientStatistics()
	{
	}

	public CommandToggleClientStatistics(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		if (StoredAuxDocData.ClientStatisticsEnabled)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (StoredAuxDocData != null)
			StoredAuxDocData.ClientStatisticsEnabled = !StoredAuxDocData.ClientStatisticsEnabled;

		return VSConstants.S_OK;
	}
}
