#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Common.Interfaces;
using Microsoft.VisualStudio;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.Interfaces;




// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration
namespace BlackbirdSql.Common.Ctl.Commands;


public class SqlEditorParseQueryCommand : SqlEditorExecuteQueryCommand
{
	public SqlEditorParseQueryCommand()
	{
		// Diag.Trace();
	}

	public SqlEditorParseQueryCommand(ISqlEditorWindowPane editor)
		: base(editor)
	{
		// Diag.Trace();
	}

	protected override int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		if (ShouldRunCommand())
		{
			Editor.ParseQuery();
		}

		return VSConstants.S_OK;
	}
}
