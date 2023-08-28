#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Providers;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.QueryExecution;

using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;




namespace BlackbirdSql.Common.Ctl.Commands;


public abstract class AbstractSqlEditorCommand
{
	public ISqlEditorWindowPane Editor { get; set; }

	public int QueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		return HandleQueryStatus(ref prgCmd, pCmdText);
	}

	public int Exec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		return HandleExec(nCmdexecopt, pvaIn, pvaOut);
	}

	protected abstract int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText);

	protected abstract int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);

	public AbstractSqlEditorCommand()
	{
	}

	public AbstractSqlEditorCommand(ISqlEditorWindowPane editor)
	{
		Editor = editor;
	}

	protected AuxiliaryDocData GetAuxiliaryDocDataForEditor()
	{
		AuxiliaryDocData result = null;
		if (Editor != null)
		{
			IVsTextView codeEditorTextView = Editor.GetCodeEditorTextView();
			if (codeEditorTextView != null)
			{
				IVsTextLines textLinesForTextView = GetTextLinesForTextView(codeEditorTextView);
				if (textLinesForTextView != null)
				{
					result = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(textLinesForTextView);
				}
			}
		}

		return result;
	}

	protected QueryExecutor GetQueryExecutorForEditor()
	{
		return GetAuxiliaryDocDataForEditor()?.QueryExecutor;
	}

	public static IVsTextLines GetTextLinesForTextView(IVsTextView textView)
	{
		IVsTextLines ppBuffer = null;
		if (textView != null)
		{
			Native.ThrowOnFailure(textView.GetBuffer(out ppBuffer));
		}

		return ppBuffer;
	}

	protected bool IsEditorExecutingOrDebugging()
	{
		QueryExecutor queryExecutorForEditor = GetQueryExecutorForEditor();
		if (queryExecutorForEditor != null)
		{
			if (!queryExecutorForEditor.IsExecuting)
			{
				return queryExecutorForEditor.IsDebugging;
			}

			return true;
		}

		return false;
	}

	protected bool IsDwEditorConnection()
	{
		AuxiliaryDocData auxillaryDocData = ((IBEditorPackage)Controller.Instance.DdexPackage).GetAuxiliaryDocData(Editor.DocData);
		if (auxillaryDocData != null)
		{
			ISqlEditorStrategy strategy = auxillaryDocData.Strategy;
			switch (strategy.Mode)
			{
				case EnEditorMode.Standard:
					{
						QueryExecutor queryExecutor = auxillaryDocData.QueryExecutor;
						if (queryExecutor != null && queryExecutor.ConnectionStrategy != null)
						{
							return queryExecutor.ConnectionStrategy?.IsDwConnection ?? false;
						}

						break;
					}
				case EnEditorMode.CustomProject:
				case EnEditorMode.CustomOnline:
					if (strategy != null)
					{
						return strategy.IsDw;
					}

					break;
			}
		}

		return false;
	}
}
