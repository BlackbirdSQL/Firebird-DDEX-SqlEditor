#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Common.Controls.Interfaces;


namespace BlackbirdSql.Common.Ctl.Commands;

public class SqlEditorResultsAsGridCommand : AbstractSqlEditorCommand
{
	public SqlEditorResultsAsGridCommand()
	{
	}

	public SqlEditorResultsAsGridCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor != null)
		{
			if (!IsEditorExecuting())
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}

			if (auxiliaryDocDataForEditor.SqlOutputMode == EnSqlOutputMode.ToGrid)
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
			}
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor != null)
		{
			auxiliaryDocDataForEditor.SqlOutputMode = EnSqlOutputMode.ToGrid;
		}

		return VSConstants.S_OK;
	}
}
