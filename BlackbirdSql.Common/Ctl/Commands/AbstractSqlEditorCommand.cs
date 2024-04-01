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
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Common.Ctl.Commands;


public abstract class AbstractSqlEditorCommand
{

	public AbstractSqlEditorCommand()
	{
	}

	public AbstractSqlEditorCommand(IBSqlEditorWindowPane editorWindow)
	{
		EditorWindow = editorWindow;
	}



	protected bool _IsDwEditorConnection = false;
	private IVsTextView _CodeEditorTextView = null;

	public IBSqlEditorWindowPane EditorWindow { get; set; }

	protected QueryManager QryMgr => GetAuxilliaryDocData()?.QryMgr;


	public int QueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		return HandleQueryStatus(ref prgCmd, pCmdText);
	}

	public int Exec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		return HandleExec(nCmdexecopt, pvaIn, pvaOut);
	}

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	protected abstract int HandleQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText);

	protected abstract int HandleExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);


	protected AuxilliaryDocData GetAuxilliaryDocData()
	{
		if (EditorWindow == null)
			return null;

		AuxilliaryDocData result = null;

		_CodeEditorTextView ??= ((IBEditorWindowPane)EditorWindow).GetCodeEditorTextView();

		if (_CodeEditorTextView != null)
		{
			IVsTextLines textLinesForTextView = GetTextLinesForTextView(_CodeEditorTextView);

			if (textLinesForTextView != null)
			{
				result = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(textLinesForTextView);
			}
		}

		return result;
	}


	public static IVsTextLines GetTextLinesForTextView(IVsTextView textView)
	{
		IVsTextLines ppBuffer = null;

		if (textView != null)
			___(textView.GetBuffer(out ppBuffer));

		return ppBuffer;
	}

	protected bool IsEditorExecuting()
	{
		QueryManager qryMgr = QryMgr;

		if (qryMgr != null)
			return qryMgr.IsExecuting;

		return false;
	}

	protected bool IsDwEditorConnection()
	{
		// Always false.
		if (!_IsDwEditorConnection)
			return false;

		// Never happens.

		AuxilliaryDocData auxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(EditorWindow.DocData);

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
