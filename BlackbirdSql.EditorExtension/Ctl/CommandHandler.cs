// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToolbarCommandHandler<T>

using System;
using System.ComponentModel.Design;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.EditorExtension.Ctl;


public sealed class CommandHandler<TCommand> : IBsCommandHandler where TCommand : IBsCommand, new()
{

	public CommandHandler(Guid clsidCmdSet, int cmdId)
	{
		_CmdId = new (clsidCmdSet, cmdId);
	}



	private readonly CommandID _CmdId;

	public CommandID CmdId => _CmdId;

	public int OnQueryStatus(IBsTabbedEditorPane tabbedEditor, ref OLECMD prgCmd, IntPtr pCmdText)
	{
		TCommand cmd = new()
		{
			TabbedEditor = tabbedEditor
		};

		int result = cmd.QueryStatus(ref prgCmd, pCmdText);

		return result;
	}

	public int OnExec(IBsTabbedEditorPane tabbedEditor, uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		TCommand cmd = new()
		{
			TabbedEditor = tabbedEditor
		};

		int result = cmd.Exec(nCmdexecopt, pvaIn, pvaOut);

		return result;
	}
}
