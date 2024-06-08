#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class SqlEditorToggleTTSCommand : AbstractSqlEditorCommand
{
	public SqlEditorToggleTTSCommand()
	{
	}

	public SqlEditorToggleTTSCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (AuxDocData == null)
			return VSConstants.S_OK;

		if (!StoredIsExecuting)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		if (StoredAuxDocData.TtsEnabled)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (!CanDisposeTransaction(ControlsResources.ErrDisableTtsCaption))
			return VSConstants.S_OK;

		if (AuxDocData.TtsEnabled && QryMgr != null && StoredQryMgr.ConnectionStrategy != null)
			StoredQryMgr.ConnectionStrategy.Transaction = null;

		StoredAuxDocData.TtsEnabled = !StoredAuxDocData.TtsEnabled;

		return VSConstants.S_OK;
	}
}
