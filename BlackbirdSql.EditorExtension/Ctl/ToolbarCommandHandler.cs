// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToolbarCommandHandler<T>

using System;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.EditorExtension.Ctl;


public sealed class ToolbarCommandHandler<T> : IBToolbarCommandHandler where T : AbstractCommand, new()
{

	public ToolbarCommandHandler(Guid clsidCmdSet, uint cmdId)
	{
		_Clsid = new GuidId(clsidCmdSet, cmdId);
	}



	private readonly GuidId _Clsid;

	public GuidId Clsid => _Clsid;

	public int HandleQueryStatus(AbstractTabbedEditorWindowPane windowPane, ref OLECMD prgCmd, IntPtr pCmdText)
	{
		if (windowPane is TabbedEditorWindowPane tabbedEditorWindowPane)
		{
			return new T
			{
				WindowPane = tabbedEditorWindowPane
			}.QueryStatus(ref prgCmd, pCmdText);
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}

	public int HandleExec(AbstractTabbedEditorWindowPane windowPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (windowPane is TabbedEditorWindowPane tabbedEditorWindowPane)
		{
			return new T
			{
				WindowPane = tabbedEditorWindowPane
			}.Exec(nCmdexecopt, pvaIn, pvaOut);
		}

		return (int)Constants.MSOCMDERR_E_NOTSUPPORTED;
	}
}
