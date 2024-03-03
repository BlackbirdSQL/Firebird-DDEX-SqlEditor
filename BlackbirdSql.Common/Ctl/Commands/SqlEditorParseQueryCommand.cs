#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using Microsoft.VisualStudio;
using BlackbirdSql.Common.Model;
using Microsoft.VisualStudio.OLE.Interop;
using BlackbirdSql.Common.Controls.Interfaces;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration
namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorParseQueryCommand : SqlEditorExecuteQueryCommand
{
	public SqlEditorParseQueryCommand()
	{
		// Tracer.Trace();
	}

	public SqlEditorParseQueryCommand(IBSqlEditorWindowPane editorWindow)
		: base(editorWindow)
	{
		// Tracer.Trace();
	}

	protected override int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		AuxiliaryDocData auxiliaryDocDataForEditor = GetAuxiliaryDocDataForEditor();

		prgCmd.cmdf = (uint)OLECMDF.OLECMDF_SUPPORTED;
		if (auxiliaryDocDataForEditor != null)
		{
			if (!IsEditorExecuting())
			{
				// Disabled for now
				// prgCmd.cmdf |= (uint)OLECMDF.OLECMDF_ENABLED;
			}
		}

		return VSConstants.S_OK;
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ShouldRunCommand())
		{
			EditorWindow.ParseQuery();
		}

		return VSConstants.S_OK;
	}
}
