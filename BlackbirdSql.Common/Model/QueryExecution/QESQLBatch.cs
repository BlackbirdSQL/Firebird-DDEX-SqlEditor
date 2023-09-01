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

using BlackbirdSql.Core;
using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Ctl;
using BlackbirdSql.Common.Exceptions;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Properties;

using FirebirdSql.Data.FirebirdClient;
using System.CodeDom;
using BlackbirdSql.Common.Interfaces;

namespace BlackbirdSql.Common.Model.QueryExecution;


public class QESQLBatch : IDisposable
{
	/*
	public class MessagesToEat
	{
		public const int C_ChangeDatabaseErrorNumber = 5701;
	}
	*/

	protected enum BatchState
	{
		Initial,
		Executing,
		Executed,
		ProcessingResults,
		Cancelling
	}

	private const int C_ChangeDatabaseErrorNumber = 5701;

	protected bool _noResultsExpected;

	protected string _ScriptSQLText = "";

	protected int _execTimeout = 30;

	protected EnQESQLBatchSpecialAction _SpecialActions;

	protected IDbCommand _Command;

	protected QEResultSet _ActiveResultSet;

	protected BatchState _State;

	protected ITextSpan _TextSpan;

	protected int _batchIndex;

	protected long _RowsAffected;

	private bool _suppressProviderMessageHeaders;

	protected static string[] _preYukonExecutionPlanColumns = new string[20]
	{
		"Rows", "Executes", "StmtText", "StmtId", "NodeId", "Parent", "PhysicalOp", "LogicalOp", "Argument", "DefinedValues",
		"EstimateRows", "EstimateIO", "EstimateCPU", "AvgRowSize", "TotalSubtreeCost", "OutputList", "Warnings", "Type", "Parallel", "EstimateExecutions"
	};

	private ConnectionStrategy ConnectionStrategy => QueryExecutor.ConnectionStrategy;

	private QueryExecutor QueryExecutor { get; set; }

	public bool ContainsErrors { private get; set; }

	public bool SuppressProviderMessageHeaders => _suppressProviderMessageHeaders;

	public static string YukonXmlExecutionPlanColumn => "Microsoft SQL Server 2005 XML Showplan";

	public string Text
	{
		get
		{
			return _ScriptSQLText;
		}
		set
		{
			Tracer.Trace(GetType(), "QESQLBatch.Text", "value = {0}", value);
			_ScriptSQLText = value;
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
		get
		{
			return _noResultsExpected;
		}
		set
		{
			Tracer.Trace(GetType(), "QESQLBatch.NoResultsExpected", "value = {0}", value);
			_noResultsExpected = value;
		}
	}

	public int ExecTimeout
	{
		get
		{
			return _execTimeout;
		}
		set
		{
			Tracer.Trace(GetType(), "QESQLBatch.ExecTimeout", "value = {0}", value);
			_execTimeout = value;
		}
	}

	public ITextSpan TextSpan
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
		get
		{
			return _batchIndex;
		}
		set
		{
			Tracer.Trace(GetType(), "QESQLBatch.BatchIndex", "value = {0}", value);
			_batchIndex = value;
		}
	}

	public long RowsAffected => _RowsAffected;

	public event QESQLBatchErrorMessageEventHandler ErrorMessage;

	public event QESQLBatchMessageEventHandler Message;

	public event QESQLBatchNewResultSetEventHandler NewResultSet;

	// public event QESQLBatchSpecialActionEventHandler SpecialAction;

	public event QESQLBatchStatementCompletedEventHandler StatementCompleted;

	public event EventHandler Cancelling;

	public event EventHandler FinishedResultSet;

	public QESQLBatch(QueryExecutor queryExecutor)
	{
		QueryExecutor = queryExecutor;
		ContainsErrors = false;
	}

	public QESQLBatch(bool bNoResultsExpected, string sqlText, int execTimeout, QueryExecutor queryExecutor)
		: this(bNoResultsExpected, sqlText, execTimeout, EnQESQLBatchSpecialAction.None, queryExecutor)
	{
	}

	public QESQLBatch(bool bNoResultsExpected, string sqlText, int execTimeout, EnQESQLBatchSpecialAction specialActions, QueryExecutor queryExecutor)
	{
		Tracer.Trace(GetType(), "QESQLBatch.QESQLBatch", "bNoResultsExpected = {0}, execTimeout = {1}, specialActions = {2}, sqlText = \"{3}\"", bNoResultsExpected, execTimeout, specialActions, sqlText);
		_noResultsExpected = bNoResultsExpected;
		_ScriptSQLText = sqlText;
		_execTimeout = execTimeout;
		_SpecialActions = specialActions;
		QueryExecutor = queryExecutor;
		ContainsErrors = false;
	}

	public void Dispose()
	{
		Tracer.Trace(GetType(), "QESQLBatch.Dispose", "", null);
		Dispose(bDisposing: true);
		GC.SuppressFinalize(this);
	}

	protected virtual void Dispose(bool bDisposing)
	{
		Tracer.Trace(GetType(), "QESQLBatch.Dispose", "bDisposing = {0}", bDisposing);
		if (!bDisposing)
		{
			return;
		}

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
		Tracer.Trace(GetType(), "QESQLBatch.SetSuppressProviderMessageHeaders", "shouldSuppress = {0}", shouldSuppress);
		_suppressProviderMessageHeaders = shouldSuppress;
	}

	public void Reset()
	{
		Tracer.Trace(GetType(), "QESQLBatch.Reset", "", null);
		lock (this)
		{
			_State = BatchState.Initial;
			_Command = null;
			_TextSpan = null;
			_RowsAffected = 0L;
			ContainsErrors = false;
		}
	}

	public EnScriptExecutionResult Execute(IDbConnection conn, EnQESQLBatchSpecialAction specialActions)
	{
		Tracer.Trace(GetType(), "QESQLBatch.Execute", "specialActions = {0}", specialActions);
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
		return DoBatchExecution(conn, _ScriptSQLText);
	}

	public void Cancel()
	{
		Tracer.Trace(GetType(), "QESQLBatch.Cancel", "", null);
		lock (this)
		{
			if (_State == BatchState.Cancelling)
			{
				return;
			}

			_State = BatchState.Cancelling;
			if (_ActiveResultSet != null)
			{
				Cancelling?.Invoke(this, new EventArgs());

				Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLBatch.Cancel: calling InitiateStopRetrievingData", "", null);
				_ActiveResultSet.InitiateStopRetrievingData();
				Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLBatch.Cancel: InitiateStopRetrievingData returned", "", null);
				if (_Command != null)
				{
					try
					{
						Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLBatch.Cancel: calling m_command.Cancel", "", null);
						_Command.Cancel();
						Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLBatch.Cancel: m_command.Cancel returned", "", null);
					}
					catch (Exception e)
					{
						Tracer.LogExCatch(GetType(), e);
					}
				}
			}
			else if (_Command != null)
			{
				Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLBatch.Cancel: executing Cancel command", "", null);
				try
				{
					_Command.Cancel();
				}
				catch (Exception e2)
				{
					Tracer.LogExCatch(GetType(), e2);
				}

				Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLBatch.Cancel: Cancel command returned", "", null);
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

		if (dataReader.FieldCount == 1 && string.Compare(dataReader.GetName(0), YukonXmlExecutionPlanColumn, StringComparison.OrdinalIgnoreCase) == 0)
		{
			batchSpecialAction = EnQESQLBatchSpecialAction.ExpectYukonXmlExecutionPlan;
			return true;
		}

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

		return false;
	}

	protected void HandleExceptionMessage(Exception ex)
	{
		Tracer.Trace(GetType(), "QESQLBatch.HandleExceptionMessage", "", null);
		QESQLBatchErrorMessageEventArgs args = new QESQLBatchErrorMessageEventArgs(
			string.Format(CultureInfo.CurrentCulture, SharedResx.BatchError, ex.Message), "");
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
		Tracer.Trace(GetType(), "QESQLBatch.HandleSqlMessages", "", null);
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
					text = string.Format(CultureInfo.CurrentCulture, SharedResx.SQLErrorFormatFirebird, error.Message,
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
					text = !_suppressProviderMessageHeaders ? string.Format(CultureInfo.CurrentCulture, SharedResx.SQLErrorFormat4, error.Message, error.Number, error.Class, -1) : string.Format(CultureInfo.CurrentCulture, SharedResx.SQLErrorFormat4_NoSource, error.Number, error.Class, -1);
				}

				if (flag && ErrorMessage != null)
				{
					QESQLBatchErrorMessageEventArgs args = new QESQLBatchErrorMessageEventArgs(text, error.Message, error.LineNumber, _TextSpan);
					RaiseErrorMessage(args);
				}
				else if (!flag && Message != null)
				{
					QESQLBatchMessageEventArgs args2 = new QESQLBatchMessageEventArgs(text, error.Message);
					RaiseMessage(args2);
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
		Tracer.Trace(GetType(), "QESQLBatch.HandleSqlMessages", "", null);
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
					text = string.Format(CultureInfo.CurrentCulture, SharedResx.SQLErrorFormatFirebird, error.Message,
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
					text = !_suppressProviderMessageHeaders ? string.Format(CultureInfo.CurrentCulture, SharedResx.SQLErrorFormat4, error.Message, error.Number, error.Class, -1) : string.Format(CultureInfo.CurrentCulture, SharedResx.SQLErrorFormat4_NoSource, error.Number, error.Class, -1);
				}

				if (flag && ErrorMessage != null)
				{
					QESQLBatchErrorMessageEventArgs args = new QESQLBatchErrorMessageEventArgs(text, error.Message, error.LineNumber, _TextSpan);
					RaiseErrorMessage(args);
				}
				else if (!flag && Message != null)
				{
					QESQLBatchMessageEventArgs args2 = new QESQLBatchMessageEventArgs(text, error.Message);
					RaiseMessage(args2);
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
		Tracer.Trace(GetType(), "QESQLBatch.OnSqlInfoMessage", "", null);
		HandleSqlMessages(a.Errors);
	}

	public void OnSqlStatementCompleted(object sender, StatementCompletedEventArgs e)
	{
		Tracer.Trace(GetType(), "QESQLBatch.OnSqlStatementCompleted", "", null);
		if (StatementCompleted != null)
		{
			QESQLBatchStatementCompletedEventArgs args = new QESQLBatchStatementCompletedEventArgs(e.RecordCount, (_SpecialActions & EnQESQLBatchSpecialAction.ExecuteWithDebugging) != 0);
			StatementCompleted(this, args);
		}

		if (Message != null)
		{
			QESQLBatchMessageEventArgs args2 = new QESQLBatchMessageEventArgs(string.Format(CultureInfo.CurrentCulture, SharedResx.RowsAffectedMessage, e.RecordCount.ToString(CultureInfo.InvariantCulture)));
			RaiseMessage(args2);
		}
	}

	public void RaiseMessage(QESQLBatchMessageEventArgs args)
	{
		Message?.Invoke(this, args);
	}

	public void RaiseErrorMessage(QESQLBatchErrorMessageEventArgs args)
	{
		ErrorMessage?.Invoke(this, args);
	}

	/*
	protected ScriptExecutionResult ProcessResultSetForExecutionPlan(IDataReader dataReader, QESQLBatchSpecialAction batchSpecialAction)
	{
		if (dataReader == null)
		{
			ArgumentNullException ex = new("dataReader");
			Diag.Dug(ex);
			throw ex;
		}

		Tracer.Trace(GetType(), "QESQLBatch.ProcessResultSetForExecutionPlan", "", null);
		Tracer.Trace(GetType(), Tracer.Level.Information, "ProcessResultSetForExecutionPlan", "firing SpecialAction event for showplan", null);
		QESQLBatchSpecialActionEventArgs args = new QESQLBatchSpecialActionEventArgs(batchSpecialAction, this, dataReader);
		try
		{
			SpecialAction(this, args);
			lock (this)
			{
				if (_State == BatchState.Cancelling)
				{
					return ScriptExecutionResult.Cancel;
				}

				return ScriptExecutionResult.Success;
			}
		}
		catch (ThreadAbortException e)
		{
			Tracer.LogExThrow(GetType(), e, "ThreadAbortException was raised during QESqlBatch::ProcessResultSetForExecutionPlan()");
			throw;
		}
		catch (Exception e2)
		{
			Tracer.LogExThrow(GetType(), e2, "Exception  was raised raised during QESqlBatch::ProcessResultSetForExecutionPlan()");
			throw;
		}
	}
	*/

	protected EnScriptExecutionResult ProcessResultSet(IDataReader dataReader, string script)
	{
		if (dataReader == null)
		{
			ArgumentNullException ex = new("dataReader");
			Diag.Dug(ex);
			throw ex;
		}

		Tracer.Trace(GetType(), "QESQLBatch.ProcessResultSet", "", null);

		/*
		if ((_SpecialActions & QESQLBatchSpecialAction.ExecutionPlanMask) != 0 && SpecialAction != null && IsExecutionPlanResultSet(dataReader, out var batchSpecialAction) && (_SpecialActions & batchSpecialAction) != 0)
		{
			return ProcessResultSetForExecutionPlan(dataReader, batchSpecialAction);
		}
		*/

		lock (this)
		{
			_ActiveResultSet = new QEResultSet(dataReader, QueryExecutor, script);
		}

		try
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "ProcessResultSet", "result set has been created!");
			EnScriptExecutionResult result = EnScriptExecutionResult.Success;
			QESQLBatchNewResultSetEventArgs args = new QESQLBatchNewResultSetEventArgs(_ActiveResultSet);
			Tracer.Trace(GetType(), Tracer.Level.Information, "ProcessResultSet", "firing the event!");
			NewResultSet(this, args);
			_RowsAffected += _ActiveResultSet.TotalNumberOfRows;
			if (_State != BatchState.Cancelling)
			{
				return result;
			}

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

	protected EnScriptExecutionResult DoBatchExecution(IDbConnection conn, string script)
	{
		Tracer.Trace(GetType(), "QESQLBatch.DoBatchExecution", "conn.State = {0}", conn.State);
		lock (this)
		{
			if (_State == BatchState.Cancelling)
			{
				_State = BatchState.Initial;
				return EnScriptExecutionResult.Cancel;
			}
		}

		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Success;
		string text;
		if ((_SpecialActions & EnQESQLBatchSpecialAction.ExecuteWithDebugging) != 0)
		{
			if (conn == null || conn is not FbConnection)
			{
				InvalidOperationException ex = new(SharedResx.CannotDebugConnectionType);
				Diag.Dug(ex);
				throw ex;
			}

			text = TextSpan != null ? SqlDebugBatchContext.Instance.GetBatchDebugScript(script, TextSpan) : script;
			if (text == null)
			{
				lock (this)
				{
					_State = BatchState.Initial;
					return EnScriptExecutionResult.Cancel;
				}
			}
		}
		else
		{
			text = script;
		}

		IDbCommand dbCommand = conn.CreateCommand();
		dbCommand.CommandText = text;
		dbCommand.CommandTimeout = _execTimeout;
		IBatchExecutionHandler batchExecutionHandler = ConnectionStrategy.CreateBatchExecutionHandler();
		batchExecutionHandler?.Register(conn, dbCommand, this);
		conn.GetType().ToString().EndsWith("SqlCeConnection", StringComparison.Ordinal);
		IDataReader dataReader = null;
		lock (this)
		{
			_State = BatchState.Executing;
			_Command = dbCommand;
			dbCommand = null;
		}

		try
		{
			if (_noResultsExpected)
			{
				Tracer.Trace(GetType(), Tracer.Level.Verbose, "DoBatchExecution", "calling ExecuteNonQuery!");
				int i = _Command.ExecuteNonQuery();

				StatementCompletedEventArgs args = new(i);
				OnSqlStatementCompleted(_Command, args);

				Tracer.Trace(GetType(), Tracer.Level.Information, "DoBatchExecution", " ExecuteNonQuery returned!");
				lock (this)
				{
					if (_State == BatchState.Cancelling)
					{
						scriptExecutionResult = EnScriptExecutionResult.Cancel;
					}
					else
					{
						scriptExecutionResult = EnScriptExecutionResult.Success;
						_State = BatchState.Executed;
					}
				}
			}
			else
			{
				Tracer.Trace(GetType(), Tracer.Level.Verbose, "DoBatchExecution", ": calling ExecuteReader!");
				dataReader = _Command.ExecuteReader(CommandBehavior.SequentialAccess);

				StatementCompletedEventArgs args = new(dataReader == null ? 0 : dataReader.RecordsAffected);
				OnSqlStatementCompleted(_Command, args);



				Tracer.Trace(GetType(), Tracer.Level.Information, "DoBatchExecution", ": got the reader!");
				lock (this)
				{
					if (_State == BatchState.Cancelling)
					{
						scriptExecutionResult = EnScriptExecutionResult.Cancel;
					}
					else
					{
						_State = BatchState.ProcessingResults;
					}
				}

				/*
				if (dataReader != null)
				{
					Type type = dataReader.GetType();
					if (type.FullName.Equals("Microsoft.SqlServerCe.Client.SqlCeDataReader", StringComparison.OrdinalIgnoreCase))
					{
						PropertyInfo property = type.GetProperty("HideSystemColumns");
						property?.SetValue(dataReader, false, null);
					}
				}
				*/

				if (NewResultSet != null && scriptExecutionResult != EnScriptExecutionResult.Cancel)
				{
					EnScriptExecutionResult scriptExecutionResult2 = EnScriptExecutionResult.Success;
					bool flag = false;
					do
					{
						if (dataReader.FieldCount <= 0)
						{
							Tracer.Trace(GetType(), Tracer.Level.Warning, "DoBatchExecution", ": result set is empty");
							flag = dataReader.NextResult();
							continue;
						}

						Tracer.Trace(GetType(), Tracer.Level.Information, "DoBatchExecution", ": processing result set");
						scriptExecutionResult2 = ProcessResultSet(dataReader, script);
						if (scriptExecutionResult2 != EnScriptExecutionResult.Success)
						{
							Tracer.Trace(GetType(), Tracer.Level.Verbose, "DoBatchExecution", ": something wrong while processing the result set: {0}", scriptExecutionResult2);
							scriptExecutionResult = scriptExecutionResult2;
							break;
						}

						Tracer.Trace(GetType(), Tracer.Level.Verbose, "DoBatchExecution", ": successfully processed the result set");
						FinishedResultSet?.Invoke(this, new EventArgs());

						flag = dataReader.NextResult();
					}
					while (flag);
					if (ContainsErrors)
					{
						Tracer.Trace(GetType(), Tracer.Level.Warning, "DoBatchExecution", ": successfull processed result set, but there were errors shown to the user");
						scriptExecutionResult = EnScriptExecutionResult.Failure;
					}

					if (scriptExecutionResult != EnScriptExecutionResult.Cancel)
					{
						lock (this)
						{
							_State = BatchState.Executed;
						}
					}
				}
				else
				{
					Tracer.Trace(GetType(), Tracer.Level.Warning, "DoBatchExecution", ": no NewResultSet handler was specified or Cancel was received!");
				}
			}
		}
		catch (IOException ex)
		{
			Tracer.LogExCatch(GetType(), ex);
			scriptExecutionResult = EnScriptExecutionResult.Failure;
			HandleExceptionMessage(ex);
		}
		catch (OverflowException ex2)
		{
			Tracer.LogExCatch(GetType(), ex2);
			scriptExecutionResult = EnScriptExecutionResult.Failure;
			HandleExceptionMessage(ex2);
		}
		catch (ThreadAbortException e)
		{
			Tracer.LogExCatch(GetType(), e);
			scriptExecutionResult = EnScriptExecutionResult.Failure;
		}
		catch (SystemException ex3)
		{
			Tracer.LogExCatch(GetType(), ex3);
			lock (this)
			{
				scriptExecutionResult = _State != BatchState.Cancelling ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;
			}

			if (scriptExecutionResult != EnScriptExecutionResult.Cancel)
			{
				if (ex3.GetType().ToString().EndsWith("FbException", StringComparison.Ordinal)
					|| ex3.GetType().ToString().EndsWith("FbException", StringComparison.Ordinal))
				{
					HandleExceptionMessages(ex3);
				}
				else
				{
					Tracer.LogExCatch(GetType(), ex3);
					HandleExceptionMessage(ex3);
				}
			}
		}
		catch (Exception ex4)
		{
			Tracer.LogExCatch(GetType(), ex4);
			HandleExceptionMessage(ex4);
			scriptExecutionResult = EnScriptExecutionResult.Failure;
		}
		finally
		{
			batchExecutionHandler?.UnRegister(conn, _Command, this);
			conn.GetType().ToString().EndsWith("SqlCeConnection", StringComparison.Ordinal);
			Tracer.Trace(GetType(), Tracer.Level.Information, "DoBatchExecution", "Closing the data reader");
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

				Tracer.Trace(GetType(), Tracer.Level.Verbose, "DoBatchExecution", "data reader is closed");
			}

			lock (this)
			{
				_State = BatchState.Initial;
				_Command.Dispose();
				_Command = null;
			}
		}

		Tracer.Trace(GetType(), Tracer.Level.Information, "DoBatchExecution", "returning {0}", scriptExecutionResult);
		return scriptExecutionResult;
	}
}
