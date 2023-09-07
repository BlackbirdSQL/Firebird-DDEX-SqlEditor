#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Data.Common;

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Common.Properties;

using Microsoft.VisualStudio.TextManager.Interop;
using BlackbirdSql.Common.Config.Interfaces;
using BlackbirdSql.Common.Enums;
using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Config;

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
		public IQueryExecutionResultsSettings ResultsSettings { get; private set; }

		public ResultsSettingsChangedEventArgs(IQueryExecutionResultsSettings resultSettings)
		{
			ResultsSettings = resultSettings;
		}
	}

	private QueryExecutor _QueryExecutor;

	public const string C_TName = "AuxiliaryDocData";

	private ISqlEditorStrategy _Strategy;

	private readonly object _LocalLock = new object();

	private bool? _IntellisenseEnabled;

	private string _DatabaseAtQueryExecutionStart;

	private object DocData { get; set; }

	public ISqlEditorStrategy Strategy
	{
		get
		{
			lock (_LocalLock)
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

			lock (_LocalLock)
			{
				_Strategy = value;
				QueryExecutor.ConnectionStrategy = Strategy.CreateConnectionStrategy();
				QueryExecutor.SqlCmdVariableResolver = Strategy.ResolveSqlCmdVariable;
			}
		}
	}

	public QueryExecutor QueryExecutor
	{
		get
		{
			lock (_LocalLock)
			{
				if (_QueryExecutor == null)
				{
					_QueryExecutor = new QueryExecutor(Strategy.CreateConnectionStrategy(), Strategy.ResolveSqlCmdVariable);
					_QueryExecutor.StatusChanged += QueryExecutorStatusChangedEventHandler;
					_QueryExecutor.ScriptExecutionStarted += QueryExecutorScriptExecutionStartedHandler;
					_QueryExecutor.ScriptExecutionCompleted += QueryExecutorScriptExecutionCompletedHandler;
				}

				return _QueryExecutor;
			}
		}
	}

	public bool? IntellisenseEnabled
	{
		get
		{
			lock (_LocalLock)
			{
				return _IntellisenseEnabled;
			}
		}
		set
		{
			lock (_LocalLock)
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
			lock (_LocalLock)
			{
				return QueryExecutor.ExecutionOptions.WithClientStats;
			}
		}
		set
		{
			lock (_LocalLock)
			{
				QueryExecutor.ExecutionOptions.WithClientStats = value;
			}
		}
	}

	public bool ActualExecutionPlanEnabled
	{
		get
		{
			lock (_LocalLock)
			{
				return QueryExecutor.ExecutionOptions.WithExecutionPlan;
			}
		}
		set
		{
			lock (_LocalLock)
			{
				QueryExecutor.ExecutionOptions.WithExecutionPlan = value;
			}
		}
	}

	public bool EstimatedExecutionPlanEnabled
	{
		get
		{
			lock (_LocalLock)
			{
				return QueryExecutor.ExecutionOptions.WithEstimatedExecutionPlan;
			}
		}
		set
		{
			lock (_LocalLock)
			{
				QueryExecutor.ExecutionOptions.WithEstimatedExecutionPlan = value;
			}
		}
	}

	public IQueryExecutionResultsSettings ResultsSettings
	{
		get
		{
			return QueryExecutor.QueryExecutionSettings.ExecutionResults;
		}
		set
		{
			Tracer.Trace(GetType(), "AuxiliaryDocData.ResultsSettings", "", null);
			if (QueryExecutor.IsExecuting)
			{
				InvalidOperationException ex = new();
				Diag.Dug(ex);
				throw ex;
			}

			QueryExecutor.QueryExecutionSettings.ExecutionResults = value;
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
			if (QueryExecutor.IsExecuting)
			{
				InvalidOperationException ex = new(SharedResx.SqlExecutionModeChangeFailed);
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

	public event EventHandler<SqlExecutionModeChangedEventArgs> SqlExecutionModeChanged;

	public event EventHandler<ResultsSettingsChangedEventArgs> ResultSettingsChanged;

	public AuxiliaryDocData(object docData)
	{
		DocData = docData;
	}

	private void QueryExecutorStatusChangedEventHandler(object sender, QueryExecutor.StatusChangedEventArgs args)
	{
		lock (_LocalLock)
		{
			if (args.Change == QueryExecutor.StatusType.ExecutionOptionsWithOleSqlChanged)
			{
				SetOLESqlModeOnDocData(QueryExecutor.IsWithOleSQLScripting);
			}
			else
			{
				if (args.Change != QueryExecutor.StatusType.Connected)
				{
					return;
				}

				IVsUserData iVsUserData = GetIVsUserData();
				if (QueryExecutor.IsConnected)
				{
					IDbConnection connection = QueryExecutor.ConnectionStrategy.Connection;
					if (connection.State != ConnectionState.Open)
					{
						return;
					}

					if (connection is DbConnection dbConnection)
					{
						string serverVersion = dbConnection.ServerVersion;
						Guid riidKey = LibraryData.CLSID_PropertySqlVersion;
						iVsUserData.SetData(ref riidKey, serverVersion);
						if (QueryExecutor.ConnectionStrategy is SqlConnectionStrategy connectionStrategy && connectionStrategy.IsDwConnection)
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

	private bool QueryExecutorScriptExecutionStartedHandler(object sender, QueryExecutor.ScriptExecutionStartedEventArgs args)
	{
		IDbConnection connection = QueryExecutor.ConnectionStrategy.Connection;
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
		IDbConnection connection = QueryExecutor.ConnectionStrategy.Connection;
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
		ResultsSettings = QueryExecutor.QueryExecutionSettings.ExecutionResults;
		QueryExecutor.QueryExecutionSettingsApplied = false;
	}

	public IVsUserData GetIVsUserData()
	{
		lock (_LocalLock)
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
		lock (_LocalLock)
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
		lock (_LocalLock)
		{
			if (disposing)
			{
				_Strategy?.Dispose();
				_QueryExecutor?.Dispose();
			}

			_QueryExecutor = null;
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
		if (SqlExecutionModeChanged != null)
		{
			SqlExecutionModeChangedEventArgs e = new SqlExecutionModeChangedEventArgs(newSqlExecMode);
			SqlExecutionModeChanged(this, e);
		}
	}

	public void RaiseResultSettingsChangedEvent(IQueryExecutionResultsSettings resultSettings)
	{
		if (ResultSettingsChanged != null)
		{
			ResultsSettingsChangedEventArgs e = new ResultsSettingsChangedEventArgs(resultSettings);
			ResultSettingsChanged(this, e);
		}
	}
}
