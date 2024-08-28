
using System;
using BlackbirdSql.Shared.Interfaces;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandOleCycleToNext : AbstractCommand
{

	public CommandOleCycleToNext()
	{
	}

	public CommandOleCycleToNext(IBsTabbedEditorPane tabbedEditor) : base(tabbedEditor)
	{
	}



	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (ReferenceEquals(TabbedEditor, TabbedEditor.ExtensionInstance.CurrentTabbedEditor))
			prgCmd.cmdf |= (int)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (!ReferenceEquals(TabbedEditor, TabbedEditor.ExtensionInstance.CurrentTabbedEditor))
			return VSConstants.S_OK;

		TabbedEditor.ActivateNextTab();

		return VSConstants.S_OK;
	}
}
