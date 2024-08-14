// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToggleResultsPaneCommand

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Shared.Controls.Tabs;
using BlackbirdSql.Shared.Interfaces;



namespace BlackbirdSql.Shared.Ctl.Commands;


public class CommandToggleResultsPane : AbstractCommand
{
	public CommandToggleResultsPane()
	{
	}

	public CommandToggleResultsPane(IBsTabbedEditorPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (!ExecutionLocked)
			prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;

		return VSConstants.S_OK;
	}

	protected override int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (!ExecutionLocked && EditorPane != null)
		{
			if (EditorPane.ActiveTab is EditorCodeTab)
			{
				bool splitterHidden = !EditorPane.IsSplitterVisible;
				EditorPane.IsSplitterVisible = splitterHidden;

				if (splitterHidden)
					EditorPane.SplittersVisible = splitterHidden;
			}
			else
			{
				EditorPane.IsSplitterVisible = false;
				EditorPane.ActivateCodeTab();
			}
		}

		return VSConstants.S_OK;
	}
}
