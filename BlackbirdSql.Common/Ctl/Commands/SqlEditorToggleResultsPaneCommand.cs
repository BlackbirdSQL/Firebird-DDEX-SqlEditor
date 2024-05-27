// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorToggleResultsPaneCommand
using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Controls.Tabs;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorToggleResultsPaneCommand : AbstractSqlEditorCommand
{
	public SqlEditorToggleResultsPaneCommand()
	{
	}

	public SqlEditorToggleResultsPaneCommand(IBSqlEditorWindowPane editorWindow)
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
		if (EditorWindow != null)
		{
			if (((IBTabbedEditorService)EditorWindow).ActiveTab is SqlEditorCodeTab)
			{
				bool splitterHidden = !EditorWindow.IsSplitterVisible;
				EditorWindow.IsSplitterVisible = splitterHidden;

				if (splitterHidden)
					EditorWindow.SplittersVisible = splitterHidden;
			}
			else
			{
				EditorWindow.IsSplitterVisible = false;
				EditorWindow.ActivateCodeTab();
			}
		}

		return VSConstants.S_OK;
	}
}
