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

	public CommandToggleResultsPane(IBsTabbedEditorWindowPane editorWindow)
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
		if (!ExecutionLocked && WindowPane != null)
		{
			if (((IBsWindowPaneServiceProvider)WindowPane).ActiveTab is SqlEditorCodeTab)
			{
				bool splitterHidden = !WindowPane.IsSplitterVisible;
				WindowPane.IsSplitterVisible = splitterHidden;

				if (splitterHidden)
					WindowPane.SplittersVisible = splitterHidden;
			}
			else
			{
				WindowPane.IsSplitterVisible = false;
				WindowPane.ActivateCodeTab();
			}
		}

		return VSConstants.S_OK;
	}
}
