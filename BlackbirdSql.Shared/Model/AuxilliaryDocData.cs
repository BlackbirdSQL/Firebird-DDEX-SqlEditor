
using System;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Shared.Model;


// =========================================================================================================
//										AuxilliaryDocData Class
//
/// <summary>
/// This class handles all additional DocData information and events for an active document.
/// AuxilliaryDocData is linked to a document's <see cref="IVsTextLines"/> through DocData.
/// </summary>
// =========================================================================================================
public sealed class AuxilliaryDocData
{

	// ---------------------------------------------------
	#region Constructors / Destructors - AuxilliaryDocData
	// ---------------------------------------------------


	/// <summary>
	/// Default .ctor.
	/// </summary>
	public AuxilliaryDocData(string documentMoniker, string explorerMoniker, object docData)
	{
		_OriginalDocumentMoniker = documentMoniker;
		_ExplorerMoniker = explorerMoniker;
		_DocData = docData;
	}



	public void Dispose()
	{
		Dispose(disposing: true);
	}



	private void Dispose(bool disposing)
	{
		lock (_LockLocal)
		{
			if (disposing)
			{
				_StrategyFactory?.Dispose();
				_QryMgr?.Dispose();
				_QryMgr = null;
			}
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AuxilliaryDocData
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private string _ConnectionUrlAtExecutionStart;
	private readonly object _DocData;
	private uint _DocCookie;
	private string _ExplorerMoniker;
	private bool? _IntellisenseEnabled;
	private readonly string _OriginalDocumentMoniker;
	private QueryManager _QryMgr;
	private IBConnectionStrategy _StrategyFactory;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AuxilliaryDocData
	// =========================================================================================================


	public IBConnectionStrategy StrategyFactory
	{
		get
		{
			lock (_LockLocal)
				return _StrategyFactory;
		}
		set
		{
			if (value == null)
			{
				ArgumentNullException ex = new("value");
				Diag.Dug(ex);
				throw ex;
			}

			lock (_LockLocal)
			{
				_StrategyFactory?.Dispose();
				_StrategyFactory = value;

				QryMgr.Strategy = _StrategyFactory.CreateConnectionStrategy();
			}
		}
	}


	public uint DocCookie
	{
		get { return _DocCookie; }
		set { _DocCookie = value; }
	}


	public object DocData => _DocData;


	public bool ClientStatisticsEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.LiveSettings.WithClientStats;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.LiveSettings.WithClientStats = value;
			}
		}
	}


	public EnSqlExecutionType ExecutionType
	{
		get { return QryMgr.LiveSettings.ExecutionType; }
	}


	public string ExplorerMoniker
	{
		get { return _ExplorerMoniker; }
		set { _ExplorerMoniker = value; }
	}


	public bool HasActualPlan
	{
		get { return QryMgr.LiveSettings.WithActualPlan; }
		set { QryMgr.LiveSettings.WithActualPlan = value; }
	}


	public bool HasExecutionPlan => QryMgr.LiveSettings.HasExecutionPlan;


	public bool? IntellisenseEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				if (!_IntellisenseEnabled.HasValue)
					_IntellisenseEnabled = PersistentSettings.EditorEnableIntellisense;

				return _IntellisenseEnabled;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				IVsUserData vsUserData = VsUserData;
				if (vsUserData != null)
				{
					Guid riidKey = VS.CLSID_PropIntelliSenseEnabled;
					___(vsUserData.SetData(ref riidKey, value));
					_IntellisenseEnabled = value;
				}
			}
		}
	}


	/// <summary>
	/// True if underlying document is virtual else false. (Was IsQueryWindow)
	/// </summary>
	public bool IsVirtualWindow { get; set; }


	public IBEditorTransientSettings LiveSettings => QryMgr.LiveSettings;

	public string OriginalDocumentMoniker => _OriginalDocumentMoniker;


	public QueryManager QryMgr
	{
		get
		{
			lock (_LockLocal)
			{
				if (_QryMgr == null)
				{
					_QryMgr = new QueryManager(this, StrategyFactory.CreateConnectionStrategy());
					_QryMgr.StatusChangedEvent += OnQueryManagerStatusChanged;
					_QryMgr.ScriptExecutionStartedEvent += OnScriptExecutionStarted;
					_QryMgr.ScriptExecutionCompletedEvent += OnScriptExecutionCompleted;
				}

				return _QryMgr;
			}
		}
	}


	public EnSqlOutputMode SqlOutputMode
	{
		get
		{
			return LiveSettings.EditorResultsOutputMode;
		}
		set
		{
			// Tracer.Trace(GetType(), "DisplaySQLResultsControl.SqlOutputMode", "value = {0}", value);
			if (QryMgr.IsExecuting)
			{
				InvalidOperationException ex = new(ControlsResources.SqlExecutionModeChangeFailed);
				Diag.ThrowException(ex);
			}

			if (LiveSettings.EditorResultsOutputMode != value)
			{
				LiveSettings.EditorResultsOutputMode = value;
				SqlExecutionModeChangedEvent?.Invoke(this, new(value));
			}
		}
	}


	public bool SuppressSavePrompt => IsVirtualWindow && !PersistentSettings.EditorPromptToSave;


	public bool TtsEnabled
	{
		get
		{
			lock (_LockLocal)
				return QryMgr.LiveSettings.TtsEnabled;
		}
		set
		{
			lock (_LockLocal)
				QryMgr.LiveSettings.TtsEnabled = value;
		}
	}


	public DbConnectionStringBuilder UserDataCsb
	{
		get
		{
			IVsUserData vsUserData = VsUserData;

			if (vsUserData == null)
			{
				ArgumentNullException ex = new("IVsUserData is null");
				Diag.Dug(ex);
				throw ex;
			}

			DbConnectionStringBuilder csb = null;

			Guid clsid = new(LibraryData.UserDataCsbGuid);

			try
			{
				vsUserData.GetData(ref clsid, out object objData);

				if (objData != null)
					csb = objData as DbConnectionStringBuilder;
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}

			return csb;
		}

		set
		{
			IVsUserData vsUserData = VsUserData;

			if (vsUserData != null)
			{
				Guid clsid = new(LibraryData.UserDataCsbGuid);
				try
				{
					vsUserData.SetData(ref clsid, value);
				}
				catch (Exception ex)
				{
					Diag.Dug(ex);
				}
			}
			else
			{
				ArgumentNullException ex = new("IVsUserData is null");
				Diag.Dug(ex);
				throw ex;
			}
		}
	}


	public IVsUserData VsUserData
	{
		get
		{
			lock (_LockLocal)
			{
				IVsUserData vsUserData = DocData as IVsUserData;
				// Tracer.Trace(GetType(), "AuxilliaryDocData.GetIVsUserData", "value of IVsUserData returned is {0}", vsUserData);
				return vsUserData;
			}
		}
	}




	public event EventHandler<SqlExecutionModeChangedEventArgs> SqlExecutionModeChangedEvent;
	public event EventHandler<LiveSettingsChangedEventArgs> LiveSettingsChangedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - AuxilliaryDocData
	// =========================================================================================================


	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



	public void CommitTransactions()
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsExecuting
			|| !QryMgr.GetUpdateTransactionsStatus())
		{
			return;
		}

		bool result = QryMgr.Strategy.CommitTransactions();

		if (!result)
			return;

		if (DocData is not IVsPersistDocData2 persistData)
			return;

		persistData.SetDocDataDirty(0);
	}



	/// <summary>
	/// For performance. Gets the AuxilliaryDocData directly from DocData.
	/// </summary>
	public static AuxilliaryDocData GetUserDataAuxilliaryDocData(object docData)
	{
		if (docData == null)
			return null;

		if (docData is not IVsUserData vsUserData)
			return null;

		AuxilliaryDocData value = null;
		Guid clsid = new(LibraryData.AuxilliaryDocDataGuid);

		try
		{
			vsUserData.GetData(ref clsid, out object objData);

			if (objData != null)
				value = objData as AuxilliaryDocData;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "GetData failed on DocData for LibraryData.AuxilliaryDocDataGuid.");
		}

		return value;
	}



	public void RollbackTransactions()
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsExecuting
			|| !QryMgr.GetUpdateTransactionsStatus())
		{
			return;
		}

		QryMgr.Strategy.RollbackTransactions();
	}



	private void SetOLESqlModeOnDocData(bool on)
	{
		lock (_LockLocal)
		{
			IVsUserData vsUserData = VsUserData;
			if (vsUserData != null)
			{
				Guid riidKey = VS.CLSID_PropOleSql;
				___(vsUserData.SetData(ref riidKey, on));
			}
		}
	}



	/// <summary>
	/// For performance. Sets a reference to the AuxilliaryDocData in DocData.
	/// </summary>
	public static void SetUserDataAuxilliaryDocData(object docData, AuxilliaryDocData value)
	{
		if (docData == null)
			return;

		if (docData is not IVsUserData vsUserData)
		{
			Type type = docData.GetType();

			ArgumentException ex = new ArgumentException($"DocData is not IVsUserData: {type.FullName}. SetData failed on DocData for LibraryData.AuxilliaryDocDataGuid.",
				nameof(docData));

			Diag.Dug(ex);

			return;
		}

		Guid clsid = new(LibraryData.AuxilliaryDocDataGuid);

		try
		{
			vsUserData.SetData(ref clsid, value);
		}
		catch (Exception ex)
		{
			Diag.Dug(ex, "SetData failed on DocData for LibraryData.AuxilliaryDocDataGuid.");
		}
	}



	public void UpdateLiveSettingsState(IBEditorTransientSettings liveSettings)
	{
		QryMgr.LiveSettingsApplied = false;
		LiveSettingsChangedEvent?.Invoke(this, new(liveSettings));
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AuxilliaryDocData
	// =========================================================================================================


	private void OnQueryManagerStatusChanged(object sender, QueryManager.StatusChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			if (args.Change == QueryManager.EnStatusType.ExecutionOptionsWithOleSqlChanged)
			{
				SetOLESqlModeOnDocData(QryMgr.IsWithOleSQLScripting);
			}
			else
			{
				if (args.Change != QueryManager.EnStatusType.Connected)
				{
					return;
				}

				IVsUserData vsUserData = VsUserData;
				if (QryMgr.IsConnected)
				{
					IDbConnection connection = QryMgr.Strategy.Connection;
					if (connection.State != ConnectionState.Open)
					{
						return;
					}

					if (connection is DbConnection dbConnection)
					{
						string serverVersion = dbConnection.ServerVersion;
						Guid riidKey = VS.CLSID_PropSqlVersion;
						vsUserData.SetData(ref riidKey, serverVersion);

						if (QryMgr.Strategy is ConnectionStrategy connectionStrategy
							&& connectionStrategy.IsDwConnection)
						{
							IntellisenseEnabled = false;
						}
					}
				}
				else
				{
					Guid riidKey = VS.CLSID_PropSqlVersion;
					vsUserData.SetData(ref riidKey, string.Empty);
				}

				return;
			}
		}
	}



	private void OnScriptExecutionCompleted(object sender, ScriptExecutionCompletedEventArgs args)
	{
		string connectionUrl = null;
		IDbConnection connection = QryMgr.Strategy.Connection;

		if (connection != null)
			connectionUrl = Csb.CreateConnectionUrl(connection);

		bool flag = false;
		if (connectionUrl != null && _ConnectionUrlAtExecutionStart == null || connectionUrl == null && _ConnectionUrlAtExecutionStart != null)
		{
			flag = true;
		}
		else if (connectionUrl != null && !connectionUrl.Equals(_ConnectionUrlAtExecutionStart, StringComparison.Ordinal))
		{
			flag = true;
		}

		if (flag)
		{
			IVsUserData vsUserData = VsUserData;
			if (vsUserData != null)
			{
				Guid riidKey = VS.CLSID_PropDatabaseChanged;
				___(vsUserData.SetData(ref riidKey, connectionUrl));
			}
		}

		_ConnectionUrlAtExecutionStart = null;
	}


	private bool OnScriptExecutionStarted(object sender, QueryManager.ScriptExecutionStartedEventArgs args)
	{
		IDbConnection connection = QryMgr.Strategy.Connection;

		if (connection != null)
			_ConnectionUrlAtExecutionStart = Csb.CreateConnectionUrl(connection);
		else
			_ConnectionUrlAtExecutionStart = null;

		return true;
	}


	#endregion Event Handling




	// =========================================================================================================
	#region Sub-Classes - AuxilliaryDocData
	// =========================================================================================================


	public class LiveSettingsChangedEventArgs(IBEditorTransientSettings liveSettings) : EventArgs
	{
		public IBEditorTransientSettings LiveSettings { get; private set; } = liveSettings;
	}



	public class SqlExecutionModeChangedEventArgs(EnSqlOutputMode executionMode) : EventArgs
	{
		public EnSqlOutputMode SqlOutputMode { get; private set; } = executionMode;
	}


	#endregion Sub-Classes
}
