#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Ctl.Enums;
using BlackbirdSql.Core.Model;
using Microsoft.VisualStudio.TextManager.Interop;


namespace BlackbirdSql.Common.Model;

public sealed class AuxiliaryDocData(string documentMoniker, string explorerMoniker, object docData)
{

	private readonly string _OriginalDocumentMoniker = documentMoniker;
	private string _ExplorerMoniker = explorerMoniker;
	public object DocData { get; set; } = docData;



	private uint _DocCookie;

	public uint DocCookie
	{
		get { return  _DocCookie; }
		set { _DocCookie =  value; }
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

	public const string C_TName = "AuxiliaryDocData";

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
					_QryMgr.ScriptExecutionStartedEvent += QueryManagerScriptExecutionStartedHandler;
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
				IVsUserData iVsUserData = VsUserData;
				if (iVsUserData != null)
				{
					Guid riidKey = LibraryData.CLSID_IntelliSenseEnabled;
					Native.ThrowOnFailure(iVsUserData.SetData(ref riidKey, value));
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


	public IVsUserData VsUserData
	{
		get
		{
			lock (_LockLocal)
			{
				IVsUserData vsUserData = DocData as IVsUserData;
				// Tracer.Trace(GetType(), "AuxiliaryDocData.GetIVsUserData", "value of IVsUserData returned is {0}", vsUserData);
				return vsUserData;
			}
		}
	}



	public event EventHandler<SqlExecutionModeChangedEventArgs> SqlExecutionModeChangedEvent;

	public event EventHandler<LiveSettingsChangedEventArgs> LiveSettingsChangedEvent;

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

				IVsUserData iVsUserData = VsUserData;
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
						Guid riidKey = LibraryData.CLSID_PropertySqlVersion;
						iVsUserData.SetData(ref riidKey, serverVersion);
						if (QryMgr.ConnectionStrategy is SqlConnectionStrategy connectionStrategy && connectionStrategy.IsDwConnection)
						{
							ActualExecutionPlanEnabled = false;
							IntellisenseEnabled = false;
						}
					}
				}
				else
				{
					Guid riidKey = LibraryData.CLSID_PropertySqlVersion;
					iVsUserData.SetData(ref riidKey, string.Empty);
				}

				return;
			}
		}
	}

	private bool QueryManagerScriptExecutionStartedHandler(object sender, QueryManager.ScriptExecutionStartedEventArgs args)
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
			IVsUserData iVsUserData = VsUserData;
			if (iVsUserData != null)
			{
				Guid riidKey = LibraryData.CLSID_PropertyDatabaseConnectionChanged;
				Native.ThrowOnFailure(iVsUserData.SetData(ref riidKey, connectionUrl));
			}
		}

		_DatabaseAtQueryExecutionStart = null;
	}

	public void UpdateLiveSettingsState(IBEditorTransientSettings liveSettings)
	{
		QryMgr.LiveSettingsApplied = false;
		RaiseLiveSettingsChangedEvent(liveSettings);
	}


	public DbConnectionStringBuilder GetUserDataCsb()
	{
		DbConnectionStringBuilder csb = null;

		IVsUserData userData = VsUserData;
		if (userData != null)
		{
			Guid clsid = new(LibraryData.SqlEditorConnectionStringGuid);
			try
			{
				userData.GetData(ref clsid, out object objData);

				if (objData != null)
					csb = objData as DbConnectionStringBuilder;
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

		return csb;

	}


	public void SetUserDataCsb(DbConnectionStringBuilder csb)
	{

		IVsUserData userData = VsUserData;
		if (userData != null)
		{
			Guid clsid = new(LibraryData.SqlEditorConnectionStringGuid);
			try
			{
				userData.SetData(ref clsid, csb);
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

	public void SetResultsAsTextExecutionMode()
	{
		SqlOutputMode = EnSqlOutputMode.ToText;
	}

	private void SetOLESqlModeOnDocData(bool on)
	{
		lock (_LockLocal)
		{
			IVsUserData iVsUserData = VsUserData;
			if (iVsUserData != null)
			{
				Guid riidKey = LibraryData.CLSID_PropertyOleSql;
				Native.ThrowOnFailure(iVsUserData.SetData(ref riidKey, on));
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
