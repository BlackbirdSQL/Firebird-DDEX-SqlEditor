#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Controls;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Commands;


public class SqlEditorToggleResultsPaneCommand : AbstractSqlEditorCommand
{
	public SqlEditorToggleResultsPaneCommand()
	{
	}

	public SqlEditorToggleResultsPaneCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)(OLECMDF.OLECMDF_SUPPORTED | OLECMDF.OLECMDF_ENABLED);
		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (Editor != null)
		{
			if (((ITabbedEditorService)Editor).ActiveTab is SqlEditorCodeTab)
			{
				bool flag = !Editor.IsSplitterVisible;
				Editor.IsSplitterVisible = flag;
				if (flag)
				{
					Editor.SplittersVisible = flag;
				}
			}
			else
			{
				Editor.IsSplitterVisible = false;
				Editor.ActivateCodeTab();
			}
		}

		return VSConstants.S_OK;
	}
}
