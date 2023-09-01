#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Commands;


public class SqlEditorResultsAsFileCommand : AbstractSqlEditorCommand
{
	public SqlEditorResultsAsFileCommand()
	{
	}

	public SqlEditorResultsAsFileCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();
		if (auxiliaryDocDataForEditor != null)
		{
			if (!IsEditorExecutingOrDebugging())
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}

			if (auxiliaryDocDataForEditor.SqlExecutionMode == EnSqlExecutionMode.ResultsToFile)
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
			auxiliaryDocDataForEditor.SqlExecutionMode = EnSqlExecutionMode.ResultsToFile;
		}

		return VSConstants.S_OK;
	}
}
