// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToolbarCommandHandler<T>

using System;
using BlackbirdSql.Shared.Controls;
using BlackbirdSql.Shared.Ctl;
using BlackbirdSql.Shared.Ctl.Commands;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.EditorExtension.Ctl;


public sealed class ToolbarCommandHandler<TCommand> : IBsToolbarCommandHandler where TCommand : AbstractCommand, new()
{

	public ToolbarCommandHandler(Guid clsidCmdSet, uint cmdId)
	{
		_Clsid = new GuidId(clsidCmdSet, cmdId);
	}



	private readonly GuidId _Clsid;

	public GuidId Clsid => _Clsid;

	public int OnQueryStatus(IBsTabbedEditorPane editorPane, ref OLECMD prgCmd, IntPtr pCmdText)
	{
		TCommand cmd = new()
		{
			EditorPane = editorPane
		};

		int result = cmd.QueryStatus(ref prgCmd, pCmdText);

		return result;
	}

	public int OnExec(IBsTabbedEditorPane editorPane, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		TCommand cmd = new()
		{
			EditorPane = editorPane
		};

		int result = cmd.Exec(nCmdexecopt, pvaIn, pvaOut);

		return result;
	}
}
