#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;
using System.Diagnostics.CodeAnalysis;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio;
using Microsoft.VisualStudio.Shell.Interop;
using Microsoft.VisualStudio.TextManager.Interop;



namespace BlackbirdSql.Common.Model;

[SuppressMessage("Style", "IDE0290:Use primary constructor", Justification = "Readability")]


// =========================================================================================================
//										AuxilliaryDocData Class
//
/// <summary>
/// This class handles all additional information and events.
/// AuxilliaryDocData is linked to a document's <see cref="IVsTextLines"/> through 
/// </summary>
// =========================================================================================================
public sealed class AuxilliaryDocData
{

	public AuxilliaryDocData(string documentMoniker, string explorerMoniker, object docData)
	{
		_OriginalDocumentMoniker = documentMoniker;
		_ExplorerMoniker = explorerMoniker;
		DocData = docData;
	}



	private readonly string _OriginalDocumentMoniker;
	private string _ExplorerMoniker;




	public object DocData { get; set; }



	private uint _DocCookie;

	public uint DocCookie
	{
		get { return _DocCookie; }
		set { _DocCookie = value; }
	}
	public string ExplorerMoniker
	{
		get { return _ExplorerMoniker; }
		set { _ExplorerMoniker = value; }
	}

	public string OriginalDocumentMoniker => _OriginalDocumentMoniker;


	public class SqlExecutionModeChangedEventArgs(EnSqlOutputMode executionMode) : EventArgs
	{
		public EnSqlOutputMode SqlOutputMode { get; private set; } = executionMode;
	}

	public class LiveSettingsChangedEventArgs(IBEditorTransientSettings liveSettings) : EventArgs
	{
		public IBEditorTransientSettings LiveSettings { get; private set; } = liveSettings;
	}

	private QueryManager _QryMgr;

	public const string C_TName = "AuxilliaryDocData";

	private IBSqlEditorStrategy _Strategy;

	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private bool? _IntellisenseEnabled;

	private string _DatabaseAtQueryExecutionStart;


	public IBSqlEditorStrategy Strategy
	{
		get
		{
			lock (_LockLocal)
			{
				return _Strategy;
			}
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
				_Strategy = value;
				QryMgr.ConnectionStrategy = Strategy.CreateConnectionStrategy();
				QryMgr.SqlCmdVariableResolver = Strategy.ResolveSqlCmdVariable;
			}
		}
	}

	public QueryManager QryMgr
	{
		get
		{
			lock (_LockLocal)
			{
				if (_QryMgr == null)
				{
					_QryMgr = new QueryManager(Strategy.CreateConnectionStrategy(), Strategy.ResolveSqlCmdVariable);
					_QryMgr.StatusChangedEvent += QueryManagerStatusChangedEventHandler;
					_QryMgr.ScriptExecutionStartedEvent += OnScriptExecutionStarted;
					_QryMgr.ScriptExecutionCompletedEvent += QueryExecutorScriptExecutionCompletedHandler;
				}

				return _QryMgr;
			}
		}
	}

	public bool HasQryMgr
	{
		get
		{
			lock (_LockLocal)
			{
				return _QryMgr != null;
			}
		}
	}

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

	public bool ActualExecutionPlanEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.LiveSettings.WithExecutionPlan;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.LiveSettings.WithExecutionPlan = value;
			}
		}
	}


	public bool TransactionTrackingEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.LiveSettings.WithTransactionTracking;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.LiveSettings.WithTransactionTracking = value;
			}
		}
	}

	public bool EstimatedExecutionPlanEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.LiveSettings.WithEstimatedExecutionPlan;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.LiveSettings.WithEstimatedExecutionPlan = value;
			}
		}
	}


	public IBEditorTransientSettings LiveSettings => QryMgr.LiveSettings;



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
				RaiseSqlExecutionModeChangedEvent(value);
			}
		}
	}

	/// <summary>
	/// True if underlying document is virtual else false. (Was IsQueryWindow)
	/// </summary>
	public bool IsVirtualWindow { get; set; }


	public bool SuppressSavePrompt => IsVirtualWindow && !PersistentSettings.EditorPromptToSave;


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

	/// <summary>
	/// <see cref="ErrorHandler.ThrowOnFailure"/> token.
	/// </summary>
	private static int ___(int hr) => ErrorHandler.ThrowOnFailure(hr);



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



	private void QueryManagerStatusChangedEventHandler(object sender, QueryManager.StatusChangedEventArgs args)
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
					IDbConnection connection = QryMgr.ConnectionStrategy.Connection;
					if (connection.State != ConnectionState.Open)
					{
						return;
					}

					if (connection is DbConnection dbConnection)
					{
						string serverVersion = dbConnection.ServerVersion;
						Guid riidKey = VS.CLSID_PropSqlVersion;
						vsUserData.SetData(ref riidKey, serverVersion);

						if (QryMgr.ConnectionStrategy is SqlConnectionStrategy connectionStrategy
							&& connectionStrategy.IsDwConnection)
						{
							ActualExecutionPlanEnabled = false;
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

	private bool OnScriptExecutionStarted(object sender, QueryManager.ScriptExecutionStartedEventArgs args)
	{
		IDbConnection connection = QryMgr.ConnectionStrategy.Connection;

		if (connection != null)
			_DatabaseAtQueryExecutionStart = CsbAgent.CreateConnectionUrl(connection);
		else
			_DatabaseAtQueryExecutionStart = null;

		return true;
	}

	private void QueryExecutorScriptExecutionCompletedHandler(object sender, ScriptExecutionCompletedEventArgs args)
	{
		string connectionUrl = null;
		IDbConnection connection = QryMgr.ConnectionStrategy.Connection;

		if (connection != null)
			connectionUrl = CsbAgent.CreateConnectionUrl(connection);

		bool flag = false;
		if (connectionUrl != null && _DatabaseAtQueryExecutionStart == null || connectionUrl == null && _DatabaseAtQueryExecutionStart != null)
		{
			flag = true;
		}
		else if (connectionUrl != null && !connectionUrl.Equals(_DatabaseAtQueryExecutionStart, StringComparison.Ordinal))
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

		_DatabaseAtQueryExecutionStart = null;
	}

	public void CommitTransactions()
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsExecuting || QryMgr.ConnectionStrategy == null
			|| !QryMgr.ConnectionStrategy.HasTransactions)
		{
			return;
		}

		bool result;

		try
		{
			result = QryMgr.ConnectionStrategy.CommitTransactions();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}

		if (!result)
			return;

		if (DocData is not IVsPersistDocData2 persistData)
			return;

		persistData.SetDocDataDirty(0);
	}


	public void RollbackTransactions()
	{
		if (QryMgr == null || !QryMgr.IsConnected || QryMgr.IsExecuting || QryMgr.ConnectionStrategy == null
			|| !QryMgr.ConnectionStrategy.HasTransactions)
		{
			return;
		}

		try
		{
			QryMgr.ConnectionStrategy.RollbackTransactions();
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw;
		}
	}

	public void UpdateLiveSettingsState(IBEditorTransientSettings liveSettings)
	{
		QryMgr.LiveSettingsApplied = false;
		RaiseLiveSettingsChangedEvent(liveSettings);
	}



	public void SetResultsAsTextExecutionMode()
	{
		SqlOutputMode = EnSqlOutputMode.ToText;
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
				_Strategy?.Dispose();
				_QryMgr?.Dispose();
			}

			_QryMgr = null;
		}
	}


	public void RaiseSqlExecutionModeChangedEvent(EnSqlOutputMode newSqlExecMode)
	{
		SqlExecutionModeChangedEvent?.Invoke(this, new(newSqlExecMode));
	}

	public void RaiseLiveSettingsChangedEvent(IBEditorTransientSettings liveSettings)
	{
		LiveSettingsChangedEvent?.Invoke(this, new(liveSettings));
	}
}
