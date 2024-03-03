// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorCommand
using System;
using BlackbirdSql.Common.Controls.Interfaces;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls.Interfaces;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Ctl.Commands;

public abstract class AbstractSqlEditorCommand
{
	public IBSqlEditorWindowPane EditorWindow { get; set; }

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

	public AbstractSqlEditorCommand(IBSqlEditorWindowPane editorWindow)
	{
		EditorWindow = editorWindow;
	}

	protected AuxiliaryDocData GetAuxiliaryDocDataForEditor()
	{
		AuxiliaryDocData result = null;
		if (EditorWindow != null)
		{
			IVsTextView codeEditorTextView = ((IBEditorWindowPane)EditorWindow).GetCodeEditorTextView();
			if (codeEditorTextView != null)
			{
				IVsTextLines textLinesForTextView = GetTextLinesForTextView(codeEditorTextView);
				if (textLinesForTextView != null)
				{
					result = ((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(textLinesForTextView);
				}
			}
		}

		return result;
	}

	protected QueryManager GetQueryManagerForEditor()
	{
		return GetAuxiliaryDocDataForEditor()?.QryMgr;
	}

	public static IVsTextLines GetTextLinesForTextView(IVsTextView textView)
	{
		IVsTextLines ppBuffer = null;
		if (textView != null)
		{
			Core.Native.ThrowOnFailure(textView.GetBuffer(out ppBuffer));
		}

		return ppBuffer;
	}

	protected bool IsEditorExecuting()
	{
		QueryManager qryMgrForEditor = GetQueryManagerForEditor();

		if (qryMgrForEditor != null)
			return qryMgrForEditor.IsExecuting;

		return false;
	}

	protected bool IsDwEditorConnection()
	{
		AuxiliaryDocData auxDocData = ((IBEditorPackage)Controller.DdexPackage).GetAuxiliaryDocData(EditorWindow.DocData);
		if (auxDocData != null)
		{
			IBSqlEditorStrategy strategy = auxDocData.Strategy;

			// Alway EnEditorMode.Standard atm
			switch (strategy.Mode)
			{
				case EnEditorMode.Standard:
					QueryManager qryMgr = auxDocData.QryMgr;
					if (qryMgr != null && qryMgr.ConnectionStrategy != null)
					{
						return qryMgr.ConnectionStrategy?.IsDwConnection ?? false;
					}

					break;
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
