// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.DataModel.AuxiliaryDocData

using System;
using System.Data;
using System.Data.Common;
using System.IO;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Model;
using BlackbirdSql.Shared.Ctl.Config;
using BlackbirdSql.Shared.Ctl.QueryExecution;
using BlackbirdSql.Shared.Enums;
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

				if (_QryMgr != null)
				{
					_QryMgr.StatusChangedEvent -= OnQueryManagerStatusChanged;
					_QryMgr.ExecutionStartedEvent -= OnQueryExecutionStarted;
					_QryMgr.ExecutionCompletedEvent -= OnQueryExecutionCompleted;
					_QryMgr.Dispose();
					_QryMgr = null;
				}
			}
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields - AuxilliaryDocData
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	/// <summary>
	/// Records the last moniker created so that we can do a fast equivalency comparison
	/// on the connection and use this static for the DatasetKey if they are equivalent.
	/// This avoids repeatedly creating a new Moniker and going through the
	/// registration process each time.
	/// </summary>
	private Csb _CommandCsa = null;
	private string[] _CommandDatabaseList = null;
	private long _CommandRctStamp = -1;

	private string _ConnectionUrlAtExecutionStart;
	private readonly object _DocData;
	private uint _DocCookie;
	private string _ExplorerMoniker;
	private bool? _IntellisenseEnabled;
	private readonly string _OriginalDocumentMoniker;
	private QueryManager _QryMgr;
	private IBsConnectionStrategyFactory _StrategyFactory;


	#endregion Fields





	// =========================================================================================================
	#region Property Accessors - AuxilliaryDocData
	// =========================================================================================================


	public IBsConnectionStrategyFactory StrategyFactory
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

				if (_QryMgr != null)
					_QryMgr.Strategy = _StrategyFactory.CreateConnectionStrategy();
			}
		}
	}


	public Csb CommandCsa
	{
		get { return _CommandCsa; }
		set { _CommandCsa = value; }
	}


	public string[] CommandDatabaseList
	{
		get { return _CommandDatabaseList; }
		set { _CommandDatabaseList = value; }
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


	public IBsEditorTransientSettings LiveSettings => QryMgr.LiveSettings;

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
					_QryMgr.ExecutionStartedEvent += OnQueryExecutionStarted;
					_QryMgr.ExecutionCompletedEvent += OnQueryExecutionCompleted;
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
			if (QryMgr.IsLocked)
			{
				InvalidOperationException ex = new(Resources.ExSqlExecutionModeChangeFailed);
				Diag.ThrowException(ex);
			}

			if (LiveSettings.EditorResultsOutputMode != value)
			{
				LiveSettings.EditorResultsOutputMode = value;
				SqlExecutionModeChangedEvent?.Invoke(this, new(value));
			}
		}
	}


	public long CommandRctStamp
	{
		get
		{
			return _CommandRctStamp;
		}
		set
		{
			if (_CommandRctStamp == value)
				return;

			_CommandCsa = null;
			_CommandDatabaseList = null;

			_CommandRctStamp = value;
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
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsLocked
			|| !QryMgr.GetUpdateTransactionsStatus(true))
		{
			return;
		}

		bool result = QryMgr.CommitTransactions();

		if (!result)
			return;

		if (DocData is not IVsPersistDocData2 persistData)
			return;

		// Only clear dirty flag on queries not on disk.

		string moniker = RdtManager.GetDocumentMoniker(DocCookie);

		if (moniker == null || !moniker.StartsWith(Path.GetTempPath(), StringComparison.InvariantCultureIgnoreCase))
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
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsLocked
			|| !QryMgr.GetUpdateTransactionsStatus(true))
		{
			return;
		}

		QryMgr.RollbackTransactions();
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



	public void UpdateLiveSettingsState(IBsEditorTransientSettings liveSettings)
	{
		QryMgr.LiveSettingsApplied = false;
		LiveSettingsChangedEvent?.Invoke(this, new(liveSettings));
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - AuxilliaryDocData
	// =========================================================================================================


	private void OnQueryManagerStatusChanged(object sender, QueryStatusChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			if (args.StatusFlag != EnQueryStatusFlags.Connected)
				return;


			IVsUserData vsUserData = VsUserData;
			if (QryMgr.IsConnected)
			{
				IDbConnection connection = QryMgr.Strategy.Connection;

				if (connection.State != ConnectionState.Open)
					return;

				if (connection is DbConnection dbConnection)
				{
					string serverVersion = dbConnection.ServerVersion;
					Guid riidKey = VS.CLSID_PropSqlVersion;
					vsUserData.SetData(ref riidKey, serverVersion);
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



	private void OnQueryExecutionCompleted(object sender, QueryExecutionCompletedEventArgs args)
	{
		string connectionUrl = null;
		IDbConnection connection = QryMgr.Strategy.Connection;

		if (connection != null)
			connectionUrl = Csb.CreateConnectionUrl(connection);

		EnNullEquality nullEquality = Cmd.NullEquality(_ConnectionUrlAtExecutionStart, connectionUrl);

		if (nullEquality == EnNullEquality.NotNulls)
		{
			nullEquality = connectionUrl.Equals(_ConnectionUrlAtExecutionStart, StringComparison.Ordinal)
				? EnNullEquality.Equal : EnNullEquality.UnEqual;
		}

		if (nullEquality == EnNullEquality.UnEqual)
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


	private bool OnQueryExecutionStarted(object sender, QueryExecutionStartedEventArgs args)
	{
		if (args.Connection != null)
			_ConnectionUrlAtExecutionStart = Csb.CreateConnectionUrl(args.Connection);
		else
			_ConnectionUrlAtExecutionStart = null;

		return true;
	}


	#endregion Event Handling




	// =========================================================================================================
	#region Sub-Classes - AuxilliaryDocData
	// =========================================================================================================


	public class LiveSettingsChangedEventArgs(IBsEditorTransientSettings liveSettings) : EventArgs
	{
		public IBsEditorTransientSettings LiveSettings { get; private set; } = liveSettings;
	}



	public class SqlExecutionModeChangedEventArgs(EnSqlOutputMode executionMode) : EventArgs
	{
		public EnSqlOutputMode SqlOutputMode { get; private set; } = executionMode;
	}


	#endregion Sub-Classes
}
