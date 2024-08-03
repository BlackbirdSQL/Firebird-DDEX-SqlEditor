// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorCommand

using System;
using System.Windows.Forms;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.QueryExecution;
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

	public AbstractCommand(IBsTabbedEditorWindowPane windowPane)
	{
		WindowPane = windowPane;
	}



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


			_CodeEditorTextView ??= ((IBsEditorWindowPane)WindowPane).GetCodeEditorTextView();

			if (_CodeEditorTextView != null)
			{
				IVsTextLines textLinesForTextView = GetTextLinesForTextView(_CodeEditorTextView);

				if (textLinesForTextView != null)
					_AuxDocData = ((IBsEditorPackage)ApcManager.PackageInstance).GetAuxilliaryDocData(textLinesForTextView);
			}

			if (_AuxDocData == null)
				_QryMgr = null;

			return _AuxDocData;
		}
	}



	protected IBsModelCsb StoredMdlCsb => StoredStrategy?.MdlCsb;
	protected IBsModelCsb StoredLiveMdlCsb => StoredStrategy?.LiveMdlCsb;

	protected ConnectionStrategy StoredStrategy => StoredQryMgr?.Strategy;

	public long StoredRctStamp
	{
		get { return StoredAuxDocData.CommandRctStamp; }
		set { StoredAuxDocData.CommandRctStamp = value; }
	}

	public string StoredSelectedName
	{
		get { return StoredAuxDocData.CommandSelectedName; }
		set { StoredAuxDocData.CommandSelectedName = value; }
	}

	public string[] StoredDatabaseList
	{
		get { return StoredAuxDocData.CommandDatabaseList; }
		set { StoredAuxDocData.CommandDatabaseList = value; }
	}


	protected AuxilliaryDocData StoredAuxDocData => _AuxDocData ?? AuxDocData;


	public IBsTabbedEditorWindowPane WindowPane { get; set; }


	protected bool ExecutionLocked
	{
		get
		{
			return (StoredQryMgr == null || StoredQryMgr.IsLocked);
		}
	}

	protected bool CancellationLocked
	{
		get
		{
			return (StoredQryMgr == null || StoredQryMgr.IsCancelling || !StoredQryMgr.IsExecuting);
		}
	}

	protected QueryManager QryMgr
	{
		get
		{
			_QryMgr = StoredAuxDocData?.QryMgr;
			return _QryMgr;
		}
	}

	protected QueryManager StoredQryMgr => _QryMgr ?? QryMgr;


	protected bool HasTransactions => StoredQryMgr?.HasTransactions ?? false;


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	protected bool CanDisposeTransaction(string caption = null)
	{
		QueryManager qryMgr = StoredQryMgr;

		try
		{
			if (!qryMgr.GetUpdatedTransactionsStatus(true))
				return true;
		}
		catch
		{
			return true;
		}


		string message = Resources.ExTransactionsActive;

		MessageCtl.ShowEx(message, caption, MessageBoxButtons.OK, MessageBoxIcon.Stop);

		return false;
	}




	public int QueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		return OnQueryStatus(ref prgCmd, pCmdText);
	}



	public int Exec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		return OnExec(nCmdexecopt, pvaIn, pvaOut);
	}



	protected abstract int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText);

	protected abstract int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);



	public static IVsTextLines GetTextLinesForTextView(IVsTextView textView)
	{
		IVsTextLines ppBuffer = null;

		if (textView != null)
			___(textView.GetBuffer(out ppBuffer));

		return ppBuffer;
	}



	protected bool RequestDeactivateQuery(string msgResource)
	{
		QueryManager qryMgr = StoredQryMgr;

		try
		{
			if (!qryMgr.GetUpdatedTransactionsStatus(true))
				return true;
		}
		catch
		{
			return true;
		}

		return StoredAuxDocData.RequestDeactivateQuery(msgResource);
	}

}
