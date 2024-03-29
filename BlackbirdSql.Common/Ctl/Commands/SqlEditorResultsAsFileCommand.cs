﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
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

public class SqlEditorResultsAsFileCommand : AbstractSqlEditorCommand
{
	public SqlEditorResultsAsFileCommand()
	{
	}

	public SqlEditorResultsAsFileCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		AuxiliaryDocData auxDocData = GetAuxiliaryDocDataForEditor();
		if (auxDocData != null)
		{
			if (!IsEditorExecuting())
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}

			if (auxDocData.SqlOutputMode == EnSqlOutputMode.ToFile)
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_LATCHED;
			}
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		AuxiliaryDocData auxDocData = GetAuxiliaryDocDataForEditor();
		if (auxDocData != null)
		{
			auxDocData.SqlOutputMode = EnSqlOutputMode.ToFile;
		}

		return VSConstants.S_OK;
	}
}
