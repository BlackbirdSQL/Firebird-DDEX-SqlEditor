// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.VSIntegration.SqlEditorCommand

using System;
using System.Windows.Forms;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model;
using BlackbirdSql.Shared.Properties;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.OLE.Interop;



namespace BlackbirdSql.Shared.Ctl.Commands;


// =========================================================================================================
//
//											AbstractCommand Class
//
/// <summary>
/// Base class for Editor Extension commands.
/// </summary>
// =========================================================================================================
public abstract class AbstractCommand
{

	// --------------------------------------------------------
	#region Constructors / Destructors - AbstractCommand
	// --------------------------------------------------------


	public AbstractCommand()
	{
	}



	public AbstractCommand(IBsTabbedEditorPane editorPane)
	{
		_TabbedEditorPane = editorPane;
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AbstractCommand
	// =========================================================================================================


	private AuxilliaryDocData _AuxDocData = null;
	private IBsTabbedEditorPane _TabbedEditorPane;



	#endregion Fields





	// =========================================================================================================
	#region Property accessors - AbstractCommand
	// =========================================================================================================


	protected AuxilliaryDocData CachedAuxDocData
	{
		get
		{
			if (_AuxDocData != null)
				return _AuxDocData;

			if (EditorPane == null)
				return null;

			_AuxDocData = EditorPane.AuxDocData;

			return _AuxDocData;
		}
	}


	protected IBsModelCsb CachedLiveMdlCsb => CachedStrategy?.LiveMdlCsb;

	protected string CachedLiveQualifiedName => CachedLiveMdlCsb?.AdornedQualifiedTitle ?? string.Empty;

	protected IBsModelCsb CachedMdlCsb => CachedStrategy?.MdlCsb;

	protected QueryManager CachedQryMgr => CachedAuxDocData?.QryMgr;

	protected ConnectionStrategy CachedStrategy => CachedQryMgr?.Strategy;


	protected bool CancellationLocked
	{
		get
		{
			return (CachedQryMgr == null || CachedQryMgr.IsCancelling
				|| (!CachedQryMgr.IsExecuting && !CachedQryMgr.IsConnecting));
		}
	}


	public IBsTabbedEditorPane EditorPane
	{
		get { return _TabbedEditorPane; }
		set { _TabbedEditorPane = value; }
	}


	protected bool ExecutionLocked
	{
		get
		{
			return (CachedQryMgr == null || CachedQryMgr.IsLocked);
		}
	}


	protected string[] StoredDatabaseList
	{
		get { return CachedAuxDocData.CommandDatabaseList; }
		set { CachedAuxDocData.CommandDatabaseList = value; }
	}


	protected long StoredRctStamp
	{
		get { return CachedAuxDocData.CommandRctStamp; }
		set { CachedAuxDocData.CommandRctStamp = value; }
	}


	protected string StoredSelectedName
	{
		get
		{
			string value = CachedAuxDocData.CommandSelectedName;

			if (value != null)
			{
				CachedAuxDocData.CommandSelectedName = null;
				return value;
			}

			return CachedLiveQualifiedName;
		}
		set
		{
			CachedAuxDocData.CommandSelectedName = value;
		}
	}


	#endregion Property accessors





	// =========================================================================================================
	#region Methods - AbstractCommand
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	protected static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	public int Exec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut)
	{
		return OnExec(nCmdexecopt, pvaIn, pvaOut);
	}



	public int QueryStatus(ref OLECMD prgCmd, IntPtr pCmdText)
	{
		return OnQueryStatus(ref prgCmd, pCmdText);
	}



	protected bool RequestDeactivateQuery(string msgResource)
	{
		QueryManager qryMgr = CachedQryMgr;

		try
		{
			if (!qryMgr.LiveTransactions)
				return true;
		}
		catch
		{
			return true;
		}

		return CachedAuxDocData.RequestDeactivateQuery(msgResource);
	}



	protected bool RequestDisposeTts(string caption = null)
	{
		QueryManager qryMgr = CachedQryMgr;

		try
		{
			if (!qryMgr.LiveTransactions)
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


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AbstractCommand
	// =========================================================================================================


	protected abstract int OnExec(uint nCmdexecopt, IntPtr pvaIn, IntPtr pvaOut);



	protected abstract int OnQueryStatus(ref OLECMD prgCmd, IntPtr pCmdText);


	#endregion Event Handling

}
