﻿#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;




namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorToggleClientStatisticsCommand : AbstractSqlEditorCommand
{
	public SqlEditorToggleClientStatisticsCommand()
	{
	}

	public SqlEditorToggleClientStatisticsCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		AuxiliaryDocData auxDocData = GetAuxiliaryDocDataForEditor();
		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;

		if (auxDocData != null)
		{
			if (!IsEditorExecuting())
			{
				prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}

			if (auxDocData.ClientStatisticsEnabled)
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
			auxDocData.ClientStatisticsEnabled = !auxDocData.ClientStatisticsEnabled;
		}

		return VSConstants.S_OK;
	}
}
