#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;

using BlackbirdSql.Core;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Ctl.Enums;
using BlackbirdSql.Core.Ctl.Diagnostics;

namespace BlackbirdSql.Common.Model;


public sealed class AuxiliaryDocData
{
	public class SqlExecutionModeChangedEventArgs : EventArgs
	{
		public EnSqlExecutionMode SqlExecutionMode { get; private set; }

		public SqlExecutionModeChangedEventArgs(EnSqlExecutionMode executionMode)
		{
			SqlExecutionMode = executionMode;
		}
	}

	public class ResultsSettingsChangedEventArgs : EventArgs
	{
		public IBQueryExecutionResultsSettings ResultsSettings { get; private set; }

		public ResultsSettingsChangedEventArgs(IBQueryExecutionResultsSettings resultSettings)
		{
			ResultsSettings = resultSettings;
		}
	}

	private QueryManager _QryMgr;

	public const string C_TName = "AuxiliaryDocData";

	private IBSqlEditorStrategy _Strategy;

	private readonly object _LockLocal = new object();

	private bool? _IntellisenseEnabled;

	private string _DatabaseAtQueryExecutionStart;

	private object DocData { get; set; }

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

	public bool? IntellisenseEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return _IntellisenseEnabled;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				IVsUserData iVsUserData = GetIVsUserData();
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
				return QryMgr.ExecutionOptions.WithClientStats;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.ExecutionOptions.WithClientStats = value;
			}
		}
	}

	public bool ActualExecutionPlanEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.ExecutionOptions.WithExecutionPlan;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.ExecutionOptions.WithExecutionPlan = value;
			}
		}
	}

	public bool EstimatedExecutionPlanEnabled
	{
		get
		{
			lock (_LockLocal)
			{
				return QryMgr.ExecutionOptions.WithEstimatedExecutionPlan;
			}
		}
		set
		{
			lock (_LockLocal)
			{
				QryMgr.ExecutionOptions.WithEstimatedExecutionPlan = value;
			}
		}
	}

	public IBQueryExecutionResultsSettings ResultsSettings
	{
		get
		{
			return QryMgr.QueryExecutionSettings.ExecutionResults;
		}
		set
		{
			Tracer.Trace(GetType(), "AuxiliaryDocData.ResultsSettings", "", null);
			if (QryMgr.IsExecuting)
			{
				InvalidOperationException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			QryMgr.QueryExecutionSettings.ExecutionResults = value;
			RaiseResultSettingsChangedEvent(value);
		}
	}

	public EnSqlExecutionMode SqlExecutionMode
	{
		get
		{
			return ResultsSettings.SqlExecutionMode;
		}
		set
		{
			Tracer.Trace(GetType(), "DisplaySQLResultsControl.SqlExecutionMode", "value = {0}", value);
			if (QryMgr.IsExecuting)
			{
				InvalidOperationException ex = new(ControlsResources.SqlExecutionModeChangeFailed);
				Tracer.LogExThrow(GetType(), ex);
				throw ex;
			}

			if (ResultsSettings.SqlExecutionMode != value)
			{
				ResultsSettings.SqlExecutionMode = value;
				RaiseSqlExecutionModeChangedEvent(value);
			}
		}
	}

	public bool IsQueryWindow { get; set; }

	public event EventHandler<SqlExecutionModeChangedEventArgs> SqlExecutionModeChangedEvent;

	public event EventHandler<ResultsSettingsChangedEventArgs> ResultSettingsChangedEvent;

	public AuxiliaryDocData(object docData)
	{
		DocData = docData;
	}

	private void QueryManagerStatusChangedEventHandler(object sender, QueryManager.StatusChangedEventArgs args)
	{
		lock (_LockLocal)
		{
			if (args.Change == QueryManager.StatusType.ExecutionOptionsWithOleSqlChanged)
			{
				SetOLESqlModeOnDocData(QryMgr.IsWithOleSQLScripting);
			}
			else
			{
				if (args.Change != QueryManager.StatusType.Connected)
				{
					return;
				}

				IVsUserData iVsUserData = GetIVsUserData();
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
		{
			_DatabaseAtQueryExecutionStart = connection.Database;
		}
		else
		{
			_DatabaseAtQueryExecutionStart = null;
		}

		return true;
	}

	private void QueryExecutorScriptExecutionCompletedHandler(object sender, ScriptExecutionCompletedEventArgs args)
	{
		string text = null;
		IDbConnection connection = QryMgr.ConnectionStrategy.Connection;
		if (connection != null)
		{
			text = connection.Database;
		}

		bool flag = false;
		if (text != null && _DatabaseAtQueryExecutionStart == null || text == null && _DatabaseAtQueryExecutionStart != null)
		{
			flag = true;
		}
		else if (text != null && !text.Equals(_DatabaseAtQueryExecutionStart, StringComparison.Ordinal))
		{
			flag = true;
		}

		if (flag)
		{
			IVsUserData iVsUserData = GetIVsUserData();
			if (iVsUserData != null)
			{
				Guid riidKey = LibraryData.CLSID_PropertyDatabaseChanged;
				Native.ThrowOnFailure(iVsUserData.SetData(ref riidKey, text));
			}
		}

		_DatabaseAtQueryExecutionStart = null;
	}

	public void UpdateExecutionSettings()
	{
		ResultsSettings = QryMgr.QueryExecutionSettings.ExecutionResults;
		QryMgr.QueryExecutionSettingsApplied = false;
	}

	public IVsUserData GetIVsUserData()
	{
		lock (_LockLocal)
		{
			IVsUserData vsUserData = DocData as IVsUserData;
			Tracer.Trace(GetType(), "AuxiliaryDocData.GetIVsUserData", "value of IVsUserData returned is {0}", vsUserData);
			return vsUserData;
		}
	}

	public DbConnectionStringBuilder GetUserDataCsb()
	{
		DbConnectionStringBuilder csb = null;

		IVsUserData userData = GetIVsUserData();
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

	public void SetResultsAsTextExecutionMode()
	{
		SqlExecutionMode = EnSqlExecutionMode.ResultsToText;
	}

	private void SetOLESqlModeOnDocData(bool on)
	{
		lock (_LockLocal)
		{
			IVsUserData iVsUserData = GetIVsUserData();
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

	public bool SuppressSavePromptWhenClosingEditor()
	{
		if (IsQueryWindow)
		{
			return !UserSettings.Instance.Current.General.PromptForSaveWhenClosingQueryWindows;
		}

		return false;
	}

	public void RaiseSqlExecutionModeChangedEvent(EnSqlExecutionMode newSqlExecMode)
	{
		SqlExecutionModeChangedEvent?.Invoke(this, new(newSqlExecMode));
	}

	public void RaiseResultSettingsChangedEvent(IBQueryExecutionResultsSettings resultSettings)
	{
		ResultSettingsChangedEvent?.Invoke(this, new(resultSettings));
	}
}
