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


public class SqlEditorDisconnectCommand : AbstractSqlEditorCommand
{
	public SqlEditorDisconnectCommand()
	{
	}

	public SqlEditorDisconnectCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked && StoredQryMgr.IsConnected)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (QryMgr == null)
			return VSConstants.S_OK;

		if (!CanDisposeTransaction(ControlsResources.ErrDisconnectCaption))
			return VSConstants.S_OK;

		StoredQryMgr.ConnectionStrategy.Transaction?.Dispose();
		StoredQryMgr.ConnectionStrategy.Transaction = null;
		StoredQryMgr.ConnectionStrategy.Connection?.Close();
		StoredQryMgr.ConnectionStrategy.ResetConnection();

		return VSConstants.S_OK;
	}
}
