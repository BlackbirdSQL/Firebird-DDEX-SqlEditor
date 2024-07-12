// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorCommand

using System;
using System.Windows.Forms;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


public abstract class AbstractCommand
{

	public AbstractCommand()
	{
	}

	public AbstractCommand(IBSqlEditorWindowPane windowPane)
	{
		WindowPane = windowPane;
	}



	private readonly bool _IsDwEditorConnection = false;
	private IVsTextView _CodeEditorTextView = null;
	private QueryManager _QryMgr = null;
	private AuxilliaryDocData _AuxDocData = null;


	protected AuxilliaryDocData AuxDocData
	{
		get
		{
			_AuxDocData = null;

			if (WindowPane == null)
			{
				_QryMgr = null;
				return null;
			}


			_CodeEditorTextView ??= ((IBEditorWindowPane)WindowPane).GetCodeEditorTextView();

			if (_CodeEditorTextView != null)
			{
				IVsTextLines textLinesForTextView = GetTextLinesForTextView(_CodeEditorTextView);

				if (textLinesForTextView != null)
					_AuxDocData = ((IBEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(textLinesForTextView);
			}

			if (_AuxDocData == null)
				_QryMgr = null;

			return _AuxDocData;
		}
	}


	protected AuxilliaryDocData StoredAuxDocData => _AuxDocData;


	public IBSqlEditorWindowPane WindowPane { get; set; }


	/// <summary>
	/// Always false.
	/// </summary>
	protected bool IsDwEditorConnection => _IsDwEditorConnection;


	protected bool ExecutionLocked
	{
		get
		{
			return (QryMgr == null || StoredQryMgr.IsExecuting);
		}
	}

	protected bool CancellationLocked
	{
		get
		{
			return (QryMgr == null || StoredQryMgr.IsCancelling || !StoredQryMgr.IsExecuting);
		}
	}

	protected bool StoredIsCancelling
	{
		get
		{
			return (StoredQryMgr != null && StoredQryMgr.IsCancelling);
		}
	}


	protected bool StoredIsExecuting
	{
		get
		{
			return (StoredQryMgr == null || StoredQryMgr.IsExecuting);
		}
	}



	protected QueryManager QryMgr
	{
		get
		{
			_QryMgr = AuxDocData?.QryMgr;
			return _QryMgr;
		}
	}

	protected QueryManager StoredQryMgr => _QryMgr ??= StoredAuxDocData?.QryMgr;




	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);

	protected bool CanDisposeTransaction(string caption = null)
	{
		QueryManager qryMgr = _QryMgr ?? QryMgr;

		try
		{
			if (!qryMgr.GetUpdateTransactionsStatus())
				return true;
		}
		catch
		{
			return true;
		}


		string message = ControlsResources.ErrTransactionsActive;

		MessageCtl.ShowEx(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);

		return false;
	}




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



	public static IVsTextLines GetTextLinesForTextView(IVsTextView textView)
	{
		IVsTextLines ppBuffer = null;

		if (textView != null)
			___(textView.GetBuffer(out ppBuffer));

		return ppBuffer;
	}


}
