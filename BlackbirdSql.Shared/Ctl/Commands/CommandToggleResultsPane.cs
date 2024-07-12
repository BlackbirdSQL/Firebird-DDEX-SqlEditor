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

	public CommandToggleResultsPane(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (WindowPane != null)
		{
			if (((IBTabbedEditorService)WindowPane).ActiveTab is SqlEditorCodeTab)
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
