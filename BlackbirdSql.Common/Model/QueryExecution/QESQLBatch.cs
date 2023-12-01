#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Threading;

using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Ctl.Exceptions;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Ctl.Diagnostics;
using BlackbirdSql.Core.Model;

using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Common.Model.QueryExecution;

public class QESQLBatch : IDisposable
{
	/*
	public class MessagesToEat
	{
		public const int C_ChangeDatabaseErrorNumber = 5701;
	}
	*/

	protected enum EnBatchState
	{
		Initial,
		Executing,
		Executed,
		ProcessingResults,
		Cancelling
	}

	private const int C_ChangeDatabaseErrorNumber = 5701;

	public const string C_PlanConnectionType = "FbConnection"; // "SqlCeConnection"

	protected bool _NoResultsExpected;

	protected string _SqlScript = "";

	protected int _ExecTimeout = ModelConstants.C_DefaultCommandTimeout;

	protected EnQESQLBatchSpecialAction _SpecialActions;

	protected IDbCommand _Command;

	protected QEResultSet _ActiveResultSet;

	protected EnBatchState _State;

	protected IBTextSpan _TextSpan;

	protected int _batchIndex;

	protected long _RowsAffected;

	private bool _suppressProviderMessageHeaders;

	protected static string[] _preYukonExecutionPlanColumns = new string[20]
	{
		"Rows", "Executes", "StmtText", "StmtId", "NodeId", "Parent", "PhysicalOp", "LogicalOp", "Argument", "DefinedValues",
		"EstimateRows", "EstimateIO", "EstimateCPU", "AvgRowSize", "TotalSubtreeCost", "OutputList", "Warnings", "Type", "Parallel", "EstimateExecutions"
	};

	public IDbCommand Command => _Command;

	private AbstractConnectionStrategy ConnectionStrategy => QryMgr.ConnectionStrategy;

	private QueryManager QryMgr { get; set; }

	public bool ContainsErrors { private get; set; }

	public bool SuppressProviderMessageHeaders => _suppressProviderMessageHeaders;

	public string SqlScript
	{
		get
		{
			return _SqlScript;
		}
		set
		{
			_SqlScript = value;
		}
	}

	public EnQESQLBatchSpecialAction SpecialActions
	{
		get
		{
			return _SpecialActions;
		}
		set
		{
			_SpecialActions = value;
		}
	}

	public bool NoResultsExpected
	{
		get { return _NoResultsExpected; }
		set { _NoResultsExpected = value; }
	}

	public int ExecTimeout
	{
		get { return _ExecTimeout; }
		set { _ExecTimeout = value; }
	}

	public IBTextSpan TextSpan
	{
		get
		{
			return _TextSpan;
		}
		set
		{
			_TextSpan = value;
		}
	}

	public int BatchIndex
	{
		get { return _batchIndex; }
		set { _batchIndex = value; }
	}

	public long RowsAffected => _RowsAffected;

	public event QESQLBatchErrorMessageEventHandler ErrorMessageEvent;

	public event QESQLBatchMessageEventHandler MessageEvent;

	public event QESQLBatchNewResultSetEventHandler NewResultSetEvent;

	public event QESQLBatchSpecialActionEventHandler SpecialActionEvent;

	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;

	public event QESQLDataLoadedEventHandler DataLoadedEvent;

	public event EventHandler CancellingEvent;

	public event EventHandler FinishedResultSetEvent;

	public QESQLBatch(QueryManager qryMgr)
	{
		QryMgr = qryMgr;
		ContainsErrors = false;
	}

	public QESQLBatch(bool bNoResultsExpected, string sqlScript, int execTimeout, QueryManager qryMgr)
		: this(bNoResultsExpected, sqlScript, execTimeout, EnQESQLBatchSpecialAction.None, qryMgr)
	{
	}

	public QESQLBatch(bool bNoResultsExpected, string sqlScript, int execTimeout, EnQESQLBatchSpecialAction specialActions, QueryManager qryMgr)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.QESQLBatch", "bNoResultsExpected = {0}, execTimeout = {1}, specialActions = {2}, sqlScript = \"{3}\"", bNoResultsExpected, execTimeout, specialActions, sqlScript);
		_NoResultsExpected = bNoResultsExpected;
		_SqlScript = sqlScript;
		_ExecTimeout = execTimeout;
		_SpecialActions = specialActions;
		QryMgr = qryMgr;
		ContainsErrors = false;
	}

	public void Dispose()
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Dispose", "", null);
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Dispose", "bDisposing = {0}", bDisposing);
		if (!bDisposing)
			return;

		lock (this)
		{
			if (_ActiveResultSet != null)
			{
				_ActiveResultSet.Dispose();
				_ActiveResultSet = null;
			}
		}
	}

	public void SetSuppressProviderMessageHeaders(bool shouldSuppress)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.SetSuppressProviderMessageHeaders", "shouldSuppress = {0}", shouldSuppress);
		_suppressProviderMessageHeaders = shouldSuppress;
	}

	public void Reset()
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Reset", "", null);
		lock (this)
		{
			_State = EnBatchState.Initial;
			_Command = null;
			_TextSpan = null;
			_RowsAffected = 0L;
			ContainsErrors = false;
		}
	}

	public EnScriptExecutionResult Execute(IDbConnection conn, EnQESQLBatchSpecialAction specialActions)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Execute", " ExecutionOptions.WithEstimatedExecutionPlan: " + QryMgr.LiveSettings.WithEstimatedExecutionPlan);
		lock (this)
		{
			if (_Command != null)
			{
				_Command = null;
			}
		}

		if (_ActiveResultSet != null)
		{
			_ActiveResultSet = null;
		}

		_SpecialActions = specialActions;
		// Execution
		return ExecuteInternal(conn, _SqlScript);
	}

	public void Cancel()
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Cancel", "", null);
		lock (this)
		{
			if (_State == EnBatchState.Cancelling)
			{
				return;
			}

			_State = EnBatchState.Cancelling;
			if (_ActiveResultSet != null)
			{
				CancellingEvent?.Invoke(this, new EventArgs());

				// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: calling InitiateStopRetrievingData", "", null);
				_ActiveResultSet.InitiateStopRetrievingData();
				// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: InitiateStopRetrievingData returned", "", null);
				if (_Command != null)
				{
					try
					{
						// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: calling m_command.Cancel", "", null);
						_Command.Cancel();
						// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: m_command.Cancel returned", "", null);
					}
					catch (Exception e)
					{
						Tracer.LogExCatch(GetType(), e);
					}
				}
			}
			else if (_Command != null)
			{
				// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: executing Cancel command", "", null);
				try
				{
					_Command.Cancel();
				}
				catch (Exception e2)
				{
					Tracer.LogExCatch(GetType(), e2);
				}

				// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: Cancel command returned", "", null);
			}
		}
	}

	protected static bool IsExecutionPlanResultSet(IDataReader dataReader, out EnQESQLBatchSpecialAction batchSpecialAction)
	{
		batchSpecialAction = EnQESQLBatchSpecialAction.None;
		if (dataReader == null)
		{
			ArgumentNullException ex = new("dataReader");
			Diag.Dug(ex);
			throw ex;
		}


		if (dataReader.FieldCount == 1 && string.Compare(dataReader.GetName(0), LibraryData.C_YukonXmlExecutionPlanColumn, StringComparison.OrdinalIgnoreCase) == 0)
		{
			batchSpecialAction = EnQESQLBatchSpecialAction.ExpectYukonXmlExecutionPlan;
			return true;
		}

		/*
		if (_preYukonExecutionPlanColumns.Length == dataReader.FieldCount)
		{
			for (int i = 0; i < dataReader.FieldCount; i++)
			{
				if (string.Compare(dataReader.GetName(i), _preYukonExecutionPlanColumns[i], StringComparison.OrdinalIgnoreCase) != 0)
				{
					return false;
				}
			}

			batchSpecialAction = EnQESQLBatchSpecialAction.ExpectActualExecutionPlan;
			return true;
		}

		if (_preYukonExecutionPlanColumns.Length == dataReader.FieldCount + 2)
		{
			for (int j = 2; j < dataReader.FieldCount; j++)
			{
				if (string.Compare(dataReader.GetName(j - 2), _preYukonExecutionPlanColumns[j], StringComparison.OrdinalIgnoreCase) != 0)
				{
					return false;
				}
			}

			batchSpecialAction = EnQESQLBatchSpecialAction.ExpectEstimatedExecutionPlan;
			return true;
		}
		*/

		return false;
	}

	protected void HandleExceptionMessage(Exception ex)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.HandleExceptionMessage", "", null);
		QESQLBatchErrorMessageEventArgs args = new QESQLBatchErrorMessageEventArgs(
			string.Format(CultureInfo.CurrentCulture, ControlsResources.BatchError, ex.Message), "");
		RaiseErrorMessage(args);
	}

	protected void HandleExceptionMessages(SystemException sysEx)
	{
		if (sysEx.GetType().ToString().EndsWith("FbException", StringComparison.Ordinal))
		{
			FbException ex = (FbException)sysEx;
			HandleSqlMessages(ex.Errors);
		}
	}

	public void HandleSqlMessages(FbErrorCollection errors)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.HandleSqlMessages", "", null);
		foreach (FbError error in errors)
		{
			if (error.Number != C_ChangeDatabaseErrorNumber)
			{
				string text = string.Empty;
				bool flag = false;
				int num = 0;
				if (_TextSpan != null && _TextSpan.LineWithinTextSpan >= 0)
				{
					num = _TextSpan.LineWithinTextSpan + _TextSpan.AnchorLine;
				}

				if (error.Class > 10)
				{
					flag = true;
					text = string.Format(CultureInfo.CurrentCulture, ControlsResources.SQLErrorFormatFirebird, error.Message,
						error.Number, error.Class, error.LineNumber + num);
					/*
					text = ((error.Procedure != null && (error.Procedure == null || error.Procedure.Length != 0))
						? ((!_suppressProviderMessageHeaders)
							? string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat6, error.Source, error.Number, error.Class, error.State, error.Procedure, error.LineNumber + num)
							: string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat6_NoSource, error.Number, error.Class, error.State, error.Procedure, error.LineNumber + num))
						: ((!_suppressProviderMessageHeaders)
							? string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat5, error.Source, error.Number, error.Class, error.State, error.LineNumber + num)
							: string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat5_NoSource, error.Number, error.Class, error.State, error.LineNumber + num)));
					*/
				}
				else if (error.Class > 0 && error.Number > 0)
				{
					flag = false;
					text = !_suppressProviderMessageHeaders ? string.Format(CultureInfo.CurrentCulture, ControlsResources.SQLErrorFormat4, error.Message, error.Number, error.Class, -1) : string.Format(CultureInfo.CurrentCulture, ControlsResources.SQLErrorFormat4_NoSource, error.Number, error.Class, -1);
				}

				if (flag && ErrorMessageEvent != null)
				{
					RaiseErrorMessage(new(text, error.Message, error.LineNumber, _TextSpan));
				}
				else if (!flag && MessageEvent != null)
				{
					RaiseMessage(new(text, error.Message));
				}

				if (flag)
				{
					ContainsErrors = true;
				}
			}
		}
	}

	public void HandleSqlMessages(SqlErrorCollection errors)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.HandleSqlMessages", "", null);
		foreach (FbError error in errors)
		{
			if (error.Number != C_ChangeDatabaseErrorNumber)
			{
				string text = string.Empty;
				bool flag = false;
				int num = 0;
				if (_TextSpan != null && _TextSpan.LineWithinTextSpan >= 0)
				{
					num = _TextSpan.LineWithinTextSpan + _TextSpan.AnchorLine;
				}

				if (error.Class > 10)
				{
					flag = true;
					text = string.Format(CultureInfo.CurrentCulture, ControlsResources.SQLErrorFormatFirebird, error.Message,
						error.Number, error.Class, error.LineNumber + num);
					/*
					text = ((error.Procedure != null && (error.Procedure == null || error.Procedure.Length != 0))
						? ((!_suppressProviderMessageHeaders)
							? string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat6, error.Source, error.Number, error.Class, error.State, error.Procedure, error.LineNumber + num)
							: string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat6_NoSource, error.Number, error.Class, error.State, error.Procedure, error.LineNumber + num))
						: ((!_suppressProviderMessageHeaders)
							? string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat5, error.Source, error.Number, error.Class, error.State, error.LineNumber + num)
							: string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat5_NoSource, error.Number, error.Class, error.State, error.LineNumber + num)));
					*/
				}
				else if (error.Class > 0 && error.Number > 0)
				{
					flag = false;
					text = !_suppressProviderMessageHeaders ? string.Format(CultureInfo.CurrentCulture, ControlsResources.SQLErrorFormat4, error.Message, error.Number, error.Class, -1) : string.Format(CultureInfo.CurrentCulture, ControlsResources.SQLErrorFormat4_NoSource, error.Number, error.Class, -1);
				}

				if (flag && ErrorMessageEvent != null)
				{
					RaiseErrorMessage(new(text, error.Message, error.LineNumber, _TextSpan));
				}
				else if (!flag && MessageEvent != null)
				{
					RaiseMessage(new(text, error.Message));
				}

				if (flag)
				{
					ContainsErrors = true;
				}
			}
		}
	}

	public void OnSqlInfoMessage(object sender, FbInfoMessageEventArgs a)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.OnSqlInfoMessage", "", null);
		HandleSqlMessages(a.Errors);
	}

	public void OnSqlStatementCompleted(object sender, QESQLStatementCompletedEventArgs e)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.OnSqlStatementCompleted", "", null);
		StatementCompletedEvent?.Invoke(sender, new(e.RecordCount, e.IsParseOnly, (_SpecialActions & EnQESQLBatchSpecialAction.ExecuteWithDebugging) != 0));

		if (MessageEvent != null)
		{
			RaiseMessage(new (string.Format(CultureInfo.CurrentCulture, ControlsResources.RowsAffectedMessage, e.RecordCount.ToString(CultureInfo.InvariantCulture))));
		}
	}

	public void RaiseMessage(QESQLBatchMessageEventArgs args)
	{
		MessageEvent?.Invoke(this, args);
	}

	public void RaiseErrorMessage(QESQLBatchErrorMessageEventArgs args)
	{
		ErrorMessageEvent?.Invoke(this, args);
	}

	protected EnScriptExecutionResult ProcessResultSetForExecutionPlan(IDataReader dataReader, EnQESQLBatchSpecialAction batchSpecialAction)
	{
		if (dataReader == null && Command == null)
		{
			ArgumentNullException ex = new("dataReader/QESQLBatch::Command");
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(GetType(), "QESQLBatch.ProcessResultSetForExecutionPlan", "", null);
		// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ProcessResultSetForExecutionPlan", "firing SpecialAction event for showplan", null);
		QESQLBatchSpecialActionEventArgs args = new QESQLBatchSpecialActionEventArgs(batchSpecialAction, this, dataReader);
		try
		{
			if (SpecialActionEvent == null)
			{
				ArgumentNullException ex = new("SpecialActionEvent");
				throw ex;
			}

			SpecialActionEvent(this, args);
			lock (this)
			{
				if (_State == EnBatchState.Cancelling)
				{
					return EnScriptExecutionResult.Cancel;
				}

				return EnScriptExecutionResult.Success;
			}
		}
		catch (ThreadAbortException e)
		{
			Tracer.LogExThrow(GetType(), e /*, "ThreadAbortException was raised during QESqlBatch::ProcessResultSetForExecutionPlan()" */);
			throw;
		}
		catch (Exception e2)
		{
			Tracer.LogExThrow(GetType(), e2 /*, "Exception  was raised raised during QESqlBatch::ProcessResultSetForExecutionPlan()" */);
			throw;
		}
	}

	protected EnScriptExecutionResult ProcessResultSet(IDataReader dataReader, string script)
	{
		if (dataReader == null)
		{
			ArgumentNullException ex = new("dataReader");
			Diag.Dug(ex);
			throw ex;
		}

		// Tracer.Trace(GetType(), "QESQLBatch.ProcessResultSet", "", null);


		if ((_SpecialActions & EnQESQLBatchSpecialAction.ExecutionPlanMask) != 0 && SpecialActionEvent != null && IsExecutionPlanResultSet(dataReader, out var batchSpecialAction) && (_SpecialActions & batchSpecialAction) != 0)
		{
			return ProcessResultSetForExecutionPlan(dataReader, batchSpecialAction);
		}

		if (NewResultSetEvent == null)
		{
			ArgumentNullException ex = new("NewResultSetEvent");
			Diag.Dug(ex);
			throw ex;
		}

		lock (this)
		{
			_ActiveResultSet = new QEResultSet(dataReader, QryMgr, script);
		}

		try
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ProcessResultSet", "result set has been created!");
			EnScriptExecutionResult result = EnScriptExecutionResult.Success;
			QESQLBatchNewResultSetEventArgs args = new QESQLBatchNewResultSetEventArgs(_ActiveResultSet);
			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ProcessResultSet", "firing the event!");

			// The data reader loads into a mem or disk storage dataset here then notifies
			// the consumer result set for loading into a grid or text page.
			// Call sequence: QWSQLBatch.NewResultSetEvent -> IQESQLBatchConsumer.OnNewResultSet
			// The consumer (AbstractQESQLBatchConsumer DisplaySQLResultsControl.BatchConsumer can be either of:
			//	ResultsToGridBatchConsumer or ResultsToTextOrFileBatchConsumer
			NewResultSetEvent(this, args);

			_RowsAffected += _ActiveResultSet.TotalNumberOfRows;

			if (_State != EnBatchState.Initial && _State != EnBatchState.Cancelling)
				DataLoadedEvent?.Invoke(this, new(_Command, _RowsAffected, dataReader.RecordsAffected, DateTime.Now, false, false));

			if (_State != EnBatchState.Cancelling)
				return result;

			return EnScriptExecutionResult.Cancel;
		}
		catch (Exception ex)
		{
			Diag.Dug(ex);
			throw ex;
		}
		finally
		{
			lock (this)
			{
				_ActiveResultSet = null;
			}
		}
	}

	protected EnScriptExecutionResult ExecuteInternal(IDbConnection conn, string script)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.ExecuteInternal", "conn.State = {0}", conn.State);
		lock (this)
		{
			if (_State == EnBatchState.Cancelling)
			{
				_State = EnBatchState.Initial;
				return EnScriptExecutionResult.Cancel;
			}
		}

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;
		string text;
		if ((_SpecialActions & EnQESQLBatchSpecialAction.ExecuteWithDebugging) != 0)
		{
			if (conn == null || conn is not FbConnection)
			{
				InvalidOperationException ex = new(ControlsResources.CannotDebugConnectionType);
				Diag.Dug(ex);
				throw ex;
			}

			text = TextSpan != null ? SqlDebugBatchContext.Instance.GetBatchDebugScript(script, TextSpan) : script;
			if (text == null)
			{
				lock (this)
				{
					_State = EnBatchState.Initial;
					return EnScriptExecutionResult.Cancel;
				}
			}
		}
		else
		{
			text = script;
		}

		// Tracer.Trace(GetType(), "QESQLBatch.ExecuteInternal", " Creating command - _SpecialActions: " + _SpecialActions);
		IDbCommand dbCommand = conn.CreateCommand();

		if (text.Length < 18 || text[..18].ToLower() != "select @@showplan:")
		{
			dbCommand.CommandText = text;
		}

		dbCommand.CommandTimeout = _ExecTimeout;
		IBBatchExecutionHandler batchExecutionHandler = ConnectionStrategy.CreateBatchExecutionHandler();
		batchExecutionHandler?.Register(conn, dbCommand, this);


		IDataReader dataReader = null;

		lock (this)
		{
			_State = EnBatchState.Executing;
			_Command = dbCommand;
			dbCommand = null;
		}

		try
		{
			bool expectEstimatedPlan = conn.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal)
					&& (_SpecialActions & EnQESQLBatchSpecialAction.ExpectEstimatedYukonXmlExecutionPlan) > 0
					&& (_SpecialActions & EnQESQLBatchSpecialAction.ExpectActualYukonXmlExecutionPlan) == 0;

			if (_NoResultsExpected && !expectEstimatedPlan)
			{
				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", "calling ExecuteNonQuery!");
				int i = _Command.ExecuteNonQuery();

				QESQLStatementCompletedEventArgs args = new(i, false, false);
				OnSqlStatementCompleted(_Command, args);

				// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ExecuteInternal", " ExecuteNonQuery returned!");
				lock (this)
				{
					if (_State == EnBatchState.Cancelling)
					{
						result = EnScriptExecutionResult.Cancel;
					}
					else
					{
						result = EnScriptExecutionResult.Success;
						_State = EnBatchState.Executed;
					}
				}
			}
			else
			{
				if (expectEstimatedPlan)
				{
					// We can't use the SET db command to configure the command output for ExecuteReader so we
					// get the the plan using GetCommandPlan().
					// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", ": creating reader from GetCommandPlan: " + _SpecialActions);

					DataTable table = new ();
					table.Columns.Add(LibraryData.C_YukonXmlExecutionPlanColumn, typeof(string));
					DataRow row = table.NewRow();

					_Command.Prepare();
					string str = ((FbCommand)_Command).GetCommandExplainedPlan();
					row[LibraryData.C_YukonXmlExecutionPlanColumn] = str;
					table.Rows.Add(row);

					dataReader = new DataTableReader(table);
				}
				else if (conn.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal)
					&& text.Length > 18 && text[..18].ToLower() == "select @@showplan:")
				{
					// The SELECT @@showplan command doesn't exist. We load the actual plan from the last command using GetCommandPlan() and
					// do a hack by placing the result in the query script after "SELECT @@showplan:" instead.
					// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", ": creating reader plan data from script: " + _SpecialActions);
					DataTable table = new();
					table.Columns.Add(LibraryData.C_YukonXmlExecutionPlanColumn, typeof(string));
					DataRow row = table.NewRow();

					row[LibraryData.C_YukonXmlExecutionPlanColumn] = text[18..];
					table.Rows.Add(row);

					dataReader = new DataTableReader(table);
				}
				else
				{
					// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", ": calling ExecuteReader: " + _SpecialActions);
					dataReader = _Command.ExecuteReader(CommandBehavior.SequentialAccess);

					OnSqlStatementCompleted(_Command, new(dataReader == null ? 0 : dataReader.RecordsAffected, false, false));
				}




				// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ExecuteInternal", ": got the reader!");
				lock (this)
				{
					if (_State == EnBatchState.Cancelling)
					{
						result = EnScriptExecutionResult.Cancel;
					}
					else
					{
						_State = EnBatchState.ProcessingResults;
					}
				}

				if (dataReader != null)
				{
					Type type = dataReader.GetType();
					if (type.FullName.Equals("Microsoft.SqlServerCe.Client.SqlCeDataReader", StringComparison.OrdinalIgnoreCase))
					{
						PropertyInfo property = type.GetProperty("HideSystemColumns");
						property?.SetValue(dataReader, false, null);
					}
				}

				if (NewResultSetEvent != null && result != EnScriptExecutionResult.Cancel)
				{
					EnScriptExecutionResult processingResult = EnScriptExecutionResult.Success;
					bool hasMoreRows = false;

					do
					{
						if (dataReader.FieldCount <= 0)
						{
							Tracer.Trace(GetType(), Tracer.EnLevel.Warning, "ExecuteInternal()", "Result set is empty.");
							hasMoreRows = dataReader.NextResult();
							continue;
						}

						// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ExecuteInternal", ": processing result set");

						processingResult = ProcessResultSet(dataReader, script);

						if (processingResult != EnScriptExecutionResult.Success)
						{
							// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", ": something wrong while processing the result set: {0}", processingResult);
							result = processingResult;
							break;
						}

						// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", ": successfully processed the result set");
						FinishedResultSetEvent?.Invoke(this, new EventArgs());

						hasMoreRows = dataReader.NextResult();
					}
					while (hasMoreRows);

					if (ContainsErrors)
					{
						Tracer.Trace(GetType(), Tracer.EnLevel.Warning, "ExecuteInternal()", "Successfully processed result set, but there were errors shown to the user.");
						result = EnScriptExecutionResult.Failure;
					}

					if (result != EnScriptExecutionResult.Cancel)
					{
						lock (this)
						{
							_State = EnBatchState.Executed;
						}
					}
				}
				else
				{
					Tracer.Trace(GetType(), Tracer.EnLevel.Warning, "ExecuteInternal()", "No NewResultSet handler was specified or Cancel was received!");
				}
			}
		}
		catch (IOException ex)
		{
			Tracer.LogExCatch(GetType(), ex);
			result = EnScriptExecutionResult.Failure;
			HandleExceptionMessage(ex);
		}
		catch (OverflowException ex2)
		{
			Tracer.LogExCatch(GetType(), ex2);
			result = EnScriptExecutionResult.Failure;
			HandleExceptionMessage(ex2);
		}
		catch (ThreadAbortException e)
		{
			Tracer.LogExCatch(GetType(), e);
			result = EnScriptExecutionResult.Failure;
		}
		catch (FbException exf)
		{
			Diag.Dug(exf, $"Command: " + _Command.CommandText);
			// Tracer.LogExCatch(GetType(), exf);
			lock (this)
			{
				result = _State != EnBatchState.Cancelling ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;
			}

			if (result != EnScriptExecutionResult.Cancel)
			{
				HandleExceptionMessages(exf);
			}
		}
		catch (SystemException ex3)
		{
			Tracer.LogExCatch(GetType(), ex3);
			lock (this)
			{
				result = _State != EnBatchState.Cancelling ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;
			}

			if (result != EnScriptExecutionResult.Cancel)
			{
				Tracer.LogExCatch(GetType(), ex3);
				HandleExceptionMessage(ex3);
			}
		}
		catch (Exception ex4)
		{
			Tracer.LogExCatch(GetType(), ex4);
			HandleExceptionMessage(ex4);
			result = EnScriptExecutionResult.Failure;
		}
		finally
		{
			batchExecutionHandler?.UnRegister(conn, _Command, this);
			conn.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal);
			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ExecuteInternal", "Closing the data reader");
			if (dataReader != null)
			{
				try
				{
					dataReader.Close();
					dataReader = null;
				}
				catch (ThreadAbortException e2)
				{
					Tracer.LogExCatch(GetType(), e2);
				}
				catch (SystemException e3)
				{
					Tracer.LogExCatch(GetType(), e3);
				}
				catch (Exception e4)
				{
					Tracer.LogExCatch(GetType(), e4);
				}

				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "ExecuteInternal", "data reader is closed");
			}

			lock (this)
			{
				_State = EnBatchState.Initial;
				_Command.Dispose();
				_Command = null;
			}
		}

		// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "ExecuteInternal", "returning {0}", result);
		return result;
	}
}
