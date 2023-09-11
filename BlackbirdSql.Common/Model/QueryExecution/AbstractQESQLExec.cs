#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using System.Data;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using BlackbirdSql.Common.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core.Diagnostics;

using FirebirdSql.Data.FirebirdClient;
using System.Data.Common;
using BlackbirdSql.Core;

namespace BlackbirdSql.Common.Model.QueryExecution;

public abstract class AbstractQESQLExec : IDisposable
{
	protected enum ExecState
	{
		Initial,
		Executing,
		ExecutingBatch,
		Cancelling,
		Discarded
	}

	public const int C_Int64K = 65535;

	public const int C_Int1MB = 1048576;

	public const int C_Int2MB = 2097152;

	public const int C_Int5MB = 5242880;

	public const string C_PlanConnectionType = "FbConnection"; // "SqlCeConnection"


	public const int int2GB = int.MaxValue;


	protected QESQLBatchCollection _SetConnectionOptionsBatches = new QESQLBatchCollection();

	protected QESQLBatchCollection _RestoreConnectionOptionsBatches = new QESQLBatchCollection();

	protected IQESQLBatchConsumer _BatchConsumer;

	protected IDbConnection _Conn;
	protected IDbConnection _SSconn;

	protected EnQESQLBatchSpecialAction _SpecialActions;

	protected int _ExecTimeout;

	protected QESQLExecutionOptions _ExecOptions;

	protected ITextSpan _TextSpan;

	private bool _ExecOptionHasBeenChanged;

	protected int _CurBatchIndex = -1;

	protected QESQLBatch _CurBatch;

	protected string _TextPlan = null;

	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;

	private Thread _ExecThread;

	protected ExecState _ExecState;

	protected object _LockObject = new object();

	private static readonly TimeSpan S_SleepTimeout = new TimeSpan(0, 0, 0, 0, 50);

	private ConnectionStrategy ConnectionStrategy => QryMgr.ConnectionStrategy;

	protected QueryManager QryMgr { get; set; }

	public static int DefaultMaxCharsPerColumnForGrid => C_Int64K;

	public static int DefaultMaxCharsPerColumnForText => 256;

	public static int DefaultMaxCharsPerColumnForXml => 2097152;

	public static int DefaultTextSize => int.MaxValue;

	public static int DefaultSqlExecTimeout => 0;

	public static string DefaultBatchSeparator => "GO";

	public event ScriptExecutionCompletedEventHandler ExecutionCompletedEvent;

	public event QESQLBatchExecutedEventHandler BatchExecutionCompletedEvent;

	public event QESQLStartingBatchEventHandler StartingBatchExecutionEvent;

	// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;
	public event QESQLDataLoadedEventHandler DataLoadedEvent;

	public AbstractQESQLExec(QueryManager qryMgr)
	{
		QryMgr = qryMgr;
		_CurBatch = new(QryMgr)
		{
			NoResultsExpected = false
		};
	}

	public void Execute(ITextSpan textSpan, IDbConnection conn, int execTimeout, IQESQLBatchConsumer batchConsumer, QESQLExecutionOptions execOptions)
	{
		_Conn = conn;
		_BatchConsumer = batchConsumer;
		_ExecTimeout = execTimeout;
		Tracer.Trace(GetType(), "QESQLExec.Execute", " execOptions.WithEstimatedExecutionPlan: " + execOptions.WithEstimatedExecutionPlan);
		_ExecOptions = execOptions.Clone() as QESQLExecutionOptions;
		_TextSpan = textSpan;
		_SpecialActions = EnQESQLBatchSpecialAction.None;

		if (_ExecState == ExecState.Executing || _ExecState == ExecState.ExecutingBatch)
		{
			InvalidOperationException ex = new(ControlsResources.ExecutionNotCompleted);
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_ExecState = ExecState.Initial;
		_ExecThread = new(StartExecuting)
		{
			CurrentCulture = CultureInfo.CurrentCulture,
			CurrentUICulture = CultureInfo.CurrentUICulture,
			Name = "Batch Execution"
		};
		_ExecThread.Start();
	}

	public void Cancel(bool bSync, TimeSpan timeout)
	{
		Tracer.Trace(GetType(), "QESQLExec.Cancel", "", null);
		if (_ExecThread == null || _ExecThread.ThreadState == ThreadState.Unstarted || _ExecThread.ThreadState == ThreadState.Stopped)
		{
			return;
		}

		Thread thread = new(StartCancelling)
		{
			CurrentCulture = CultureInfo.CurrentCulture,
			CurrentUICulture = CultureInfo.CurrentUICulture
		};
		thread.Start();
		if (!bSync)
		{
			return;
		}

		TimeSpan timeSpan = S_SleepTimeout;
		Thread thread2 = null;
		try
		{
			lock (_LockObject)
			{
				thread2 = _ExecThread;
			}

			while (timeSpan < timeout && thread2 != null && thread2.IsAlive)
			{
				Thread.Sleep(S_SleepTimeout);
				Application.DoEvents();
				timeSpan += S_SleepTimeout;
				lock (_LockObject)
				{
					thread2 = _ExecThread;
				}
			}

			if (thread2 != null && thread2.IsAlive)
			{
				Tracer.Trace(GetType(), Tracer.Level.Warning, "QESQLExec.Cancel: execution thread won't cancel. Forgetting about it", "", null);
				HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
				lock (_LockObject)
				{
					_ExecState = ExecState.Discarded;
				}

				return;
			}

			Tracer.Trace(GetType(), Tracer.Level.Information, "QESQLExec.Cancel: thread stopped gracefully", "", null);
			if (_Conn != null)
			{
				try
				{
					_Conn.Close();
				}
				catch
				{
				}
			}
		}
		finally
		{
			thread2 = null;
		}
	}

	public void Dispose()
	{
		Tracer.Trace(GetType(), "QESQLExec.Dispose", "", null);
		Dispose(bDisposing: true);
	}

	private void StartExecuting()
	{
		try
		{
			Tracer.Trace(GetType(), "QESQLExec.StartExecuting");
			ProcessExecOptions(_Conn);
			_ExecOptionHasBeenChanged = false;
			_ExecResult = EnScriptExecutionResult.Failure;
			_CurBatchIndex = 0;
			_CurBatch.ExecTimeout = _ExecTimeout;
			_CurBatch.SetSuppressProviderMessageHeaders(_ExecOptions.SuppressProviderMessageHeaders);
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: true);

			lock (_LockObject)
			{
				if (_ExecState == ExecState.Cancelling)
				{
					OnExecutionCompleted(EnScriptExecutionResult.Cancel);
					return;
				}

				_ExecState = ExecState.Executing;
			}

			_ExecResult = SetRestoreConnectionOptions(bSet: true, _Conn);

			if (_ExecResult == EnScriptExecutionResult.Success)
				DoScriptExecution(_TextSpan);

			bool discarded = false;
			lock (_LockObject)
			{
				discarded = _ExecState == ExecState.Discarded;
			}

			if (discarded)
			{
				Tracer.Trace(GetType(), Tracer.Level.Warning, "QESQLExec.StartExecuting", "execution was discarded");
				Cleanup();
				if (_Conn != null && _Conn.State == ConnectionState.Open)
				{
					try
					{
						_Conn.Close();
					}
					catch
					{
					}
				}

				return;
			}

			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.StartExecuting", "execution was NOT discarded");
			// EstimatedExecutionPlan = gets the plan without executing the script. MS still seem to execute the script.
			// We have bypassed that and get the plan at ExecuteReader above.
			// ActualExecutionPlan = the WithExecutionPlan toggle is latched so we can get the actual because
			// ExecuteReader has been called.
			if (_ExecResult == EnScriptExecutionResult.Success && _Conn.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal) && _ExecOptions.WithExecutionPlan && !_ExecOptions.WithEstimatedExecutionPlan)
			{
				PostProcessSqlCeExecutionPlan();
			}

			if (_ExecOptionHasBeenChanged)
			{
				ProcessExecOptions(_Conn);
			}

			SetRestoreConnectionOptions(bSet: false, _Conn);
			if (_ExecResult == EnScriptExecutionResult.Halted)
			{
				_ExecResult = EnScriptExecutionResult.Failure;
			}

			OnExecutionCompleted(_ExecResult);
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
			_ExecResult = EnScriptExecutionResult.Failure;
			OnExecutionCompleted(_ExecResult);
		}
	}

	private void StartCancelling()
	{
		Tracer.Trace(GetType(), "QESQLExec.StartCancelling", "", null);
		ExecState execState;
		lock (_LockObject)
		{
			execState = _ExecState;
			_ExecState = ExecState.Cancelling;
			if (execState == ExecState.ExecutingBatch)
			{
				_CurBatch.Cancel();
			}
		}

		CompleteAsyncCancelOperation(execState);
		Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.StartCancelling: returning", "", null);
	}

	private void CleanupBatchCollection(QESQLBatchCollection col)
	{
		Tracer.Trace(GetType(), "QESQLExec.CleanupBatchCollection", "", null);
		if (col == null)
		{
			return;
		}

		foreach (QESQLBatch item in col)
		{
			item.Dispose();
		}

		col.Clear();
	}

	private void PostProcessSqlCeExecutionPlan()
	{
		Tracer.Trace(GetType(), "QESQLExec.PostProcessSqlCeExecutionPlan", "", null);

		QESQLBatch qESQLBatch;

		// The SELECT @@showplan command doesn't exist. We load the actual plan from the last command using GetCommandPlan() and
		// do a hack by placing the result in the query script instead.
		// QESQLBatch qESQLBatch = new QESQLBatch(bNoResultsExpected: false, "SELECT @@showplan", _ExecTimeout, QryMgr);
		lock (_LockObject)
		{
			_TextPlan ??= "FbCommand.GetCommandPlan() returned null";
			qESQLBatch = new QESQLBatch(bNoResultsExpected: false,
				"SELECT @@showplan:" + _TextPlan, _ExecTimeout, QryMgr);

			_TextPlan = null;
		}

		qESQLBatch.SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
		qESQLBatch.SpecialActions |= EnQESQLBatchSpecialAction.ExpectYukonXmlExecutionPlan;

		HookupBatchWithConsumer(qESQLBatch, _BatchConsumer, bHookUp: true);
		try
		{
			qESQLBatch.Execute(_Conn, qESQLBatch.SpecialActions);
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
			_ExecResult = EnScriptExecutionResult.Failure;
		}
		finally
		{
			HookupBatchWithConsumer(qESQLBatch, _BatchConsumer, bHookUp: false);
		}
	}

	protected void ProcessExecOptions(IDbConnection dbConnection)
	{
		Tracer.Trace(GetType(), "QESQLExec.ProcessExecOptions");
		_ExecOptionHasBeenChanged = true;
		StringBuilder stringBuilder = new StringBuilder(128);
		StringBuilder stringBuilder2 = new StringBuilder(128);
		CleanupBatchCollection(_SetConnectionOptionsBatches);
		CleanupBatchCollection(_RestoreConnectionOptionsBatches);
		_SpecialActions = EnQESQLBatchSpecialAction.None;
		string empty, empty2, empty3, empty4;
		if (dbConnection.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal))
		{
			if (_ExecOptions.WithEstimatedExecutionPlan)
			{
				empty = QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty, _ExecTimeout, QryMgr));
				empty2 = QESQLExecutionOptions.GetSetNoExecString(on: true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty2, _ExecTimeout, QryMgr));
				empty3 = QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: false);
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty3, _ExecTimeout, QryMgr));
				empty4 = QESQLExecutionOptions.GetSetNoExecString(on: false);
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty4, _ExecTimeout, QryMgr));
			}
			else if (_ExecOptions.WithExecutionPlan)
			{
				empty = QESQLExecutionOptions.GetSetStatisticsXml(on: true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty, _ExecTimeout, QryMgr));
				empty2 = QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty2, _ExecTimeout, QryMgr));
				empty3 = QESQLExecutionOptions.GetSetStatisticsXml(on: false);
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty3, _ExecTimeout, QryMgr));
				empty4 = QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: false);
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty4, _ExecTimeout, QryMgr));
			}

			return;
		}

		int major = ConnectionStrategy.GetServerVersion().Major;
		if (_ExecOptions.WithNoExec)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting noexec off");
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder2, QESQLExecutionOptions.GetSetNoExecString(on: false));
		}

		if (_ExecOptions.WithStatisticsIO)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics io");
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder, QESQLExecutionOptions.GetSetStatisticsIOString(on: true));
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder2, QESQLExecutionOptions.GetSetStatisticsIOString(on: false));
		}

		if (_ExecOptions.WithStatisticsTime)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics time");
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder, QESQLExecutionOptions.GetSetStatisticsTimeString(on: true));
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder2, QESQLExecutionOptions.GetSetStatisticsTimeString(on: false));
		}

		if (_ExecOptions.WithDebugging)
		{
			_SpecialActions |= EnQESQLBatchSpecialAction.ExecuteWithDebugging;
		}

		if (ConnectionStrategy.IsExecutionPlanAndQueryStatsSupported)
		{
			if (_ExecOptions.WithEstimatedExecutionPlan)
			{
				Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting estimated execution plan");
				_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
				if (major >= -1 /* 9 */) // Always
				{
					_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: true), _ExecTimeout, QryMgr));
					_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetNoExecString(on: true), _ExecTimeout, QryMgr));

					_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: false), _ExecTimeout, QryMgr));
					_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetNoExecString(on: false), _ExecTimeout, QryMgr));
					
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedExecutionPlan;
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedYukonXmlExecutionPlan;
				}
				else
				{
					_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanAllString(on: true), _ExecTimeout, QryMgr));
					_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanAllString(on: false), _ExecTimeout, QryMgr));
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedExecutionPlan;
				}
			}
			else if (_ExecOptions.WithStatisticsProfile || _ExecOptions.WithExecutionPlan)
			{
				Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics profile");
				if (major >= -1 /* 9 */)
				{
					QESQLExecutionOptions.AppendToStringBuilder(stringBuilder, QESQLExecutionOptions.GetSetStatisticsXml(on: true));
					QESQLExecutionOptions.AppendToStringBuilder(stringBuilder2, QESQLExecutionOptions.GetSetStatisticsXml(on: false));
					if (_ExecOptions.WithExecutionPlan)
					{
						_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualExecutionPlan;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualYukonXmlExecutionPlan;
					}
				}
				else
				{
					QESQLExecutionOptions.AppendToStringBuilder(stringBuilder, QESQLExecutionOptions.GetSetStatisticsProfileString(on: true));
					QESQLExecutionOptions.AppendToStringBuilder(stringBuilder2, QESQLExecutionOptions.GetSetStatisticsProfileString(on: false));
					if (_ExecOptions.WithExecutionPlan)
					{
						_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualExecutionPlan;
					}
				}
			}
		}

		if (_ExecOptions.ParseOnly)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting parseonly");
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder, QESQLExecutionOptions.GetSetParseOnlyString(on: true));
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder2, QESQLExecutionOptions.GetSetParseOnlyString(on: false));
		}

		if (_ExecOptions.WithNoExec)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting noexec on");
			QESQLExecutionOptions.AppendToStringBuilder(stringBuilder, QESQLExecutionOptions.GetSetNoExecString(on: true));
		}

		empty = stringBuilder.ToString().Trim();
		empty2 = stringBuilder2.ToString().Trim();
		if (empty.Length != 0)
		{
			_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty, _ExecTimeout, QryMgr));
			_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty2, _ExecTimeout, QryMgr));
		}

		if (ConnectionStrategy.IsExecutionPlanAndQueryStatsSupported && _ExecOptions.WithExecutionPlanText && !_ExecOptions.WithStatisticsProfile && !_ExecOptions.WithExecutionPlan && !_ExecOptions.WithEstimatedExecutionPlan)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting showplan_text");
			_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetShowplanTextString(on: true), _ExecTimeout, QryMgr));
			_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetShowplanTextString(on: false), _ExecTimeout, QryMgr));
		}
	}

	protected EnScriptExecutionResult SetRestoreConnectionOptions(bool bSet, IDbConnection connection)
	{
		Tracer.Trace(GetType(), "QESQLExec.SetRestoreConnectionOptions", "bSet = {0}", bSet);
		QESQLBatchCollection qESQLBatchCollection = bSet ? _SetConnectionOptionsBatches : _RestoreConnectionOptionsBatches;
		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Success;
		for (int i = 0; i < qESQLBatchCollection.Count; i++)
		{
			if (connection.State != ConnectionState.Open)
			{
				break;
			}

			QESQLBatch qESQLBatch = qESQLBatchCollection[i];

			if (!QESQLExecutionOptions.IsSupported(qESQLBatch))
				continue;

			HookupBatchWithConsumer(qESQLBatch, _BatchConsumer, bHookUp: true);
			try
			{
				scriptExecutionResult = qESQLBatchCollection[i].Execute(connection, _SpecialActions);
			}
			catch
			{
				scriptExecutionResult = EnScriptExecutionResult.Failure;
			}

			HookupBatchWithConsumer(qESQLBatch, _BatchConsumer, bHookUp: false);
			if (scriptExecutionResult != EnScriptExecutionResult.Success)
			{
				break;
			}
		}

		return scriptExecutionResult;
	}

	protected void ExecuteBatchCommon(string batch, ITextSpan textSpan, out bool continueProcessing)
	{
		Tracer.Trace(GetType(), "QESQLExec.ExecuteBatchCommon", " _ExecOptions.WithEstimatedExecutionPlan: " + _ExecOptions.WithEstimatedExecutionPlan);
		continueProcessing = true;
		if (batch.Trim().Length <= 0)
		{
			_ExecResult |= EnScriptExecutionResult.Success;
			return;
		}

		lock (_LockObject)
		{
			if (_ExecState == ExecState.Cancelling)
			{
				_ExecResult = EnScriptExecutionResult.Cancel;
			}
			else
			{
				_CurBatch.Reset();
				_CurBatch.Text = batch;
				_CurBatch.TextSpan = textSpan;
				_CurBatch.BatchIndex = _CurBatchIndex;
				_CurBatchIndex++;
				_ExecState = ExecState.ExecutingBatch;
			}
		}

		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Failure;
		if (_ExecResult != EnScriptExecutionResult.Cancel)
		{
			bool discarded = false;
			try
			{
				OnStartBatchExecution(_CurBatch);
				// Execution
				scriptExecutionResult = DoBatchExecution(_CurBatch);
			}
			finally
			{
				lock (_LockObject)
				{
					Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ExecuteBatchCommon", "execState = {0}", _ExecState);
					discarded = _ExecState == ExecState.Discarded;
					if (_ExecState == ExecState.Cancelling || discarded)
					{
						scriptExecutionResult = EnScriptExecutionResult.Cancel;
					}
					else
					{
						_ExecState = ExecState.Executing;
					}
				}
			}

			if (!discarded)
			{
				OnBatchExecutionCompleted(_CurBatch, scriptExecutionResult);
			}
		}
		else
		{
			scriptExecutionResult = EnScriptExecutionResult.Cancel;
		}

		if (scriptExecutionResult == EnScriptExecutionResult.Cancel || scriptExecutionResult == EnScriptExecutionResult.Halted)
		{
			_ExecResult = scriptExecutionResult;
			continueProcessing = false;
		}
		else
		{
			_ExecResult |= scriptExecutionResult;
		}
	}

	protected void HookupBatchWithConsumer(QESQLBatch batch, IQESQLBatchConsumer batchConsumer, bool bHookUp)
	{
		Tracer.Trace(GetType(), "QESQLExec.HookupBatchWithConsumer", "bHookUp = {0}", bHookUp);
		if (batch != null && batchConsumer != null)
		{
			if (!bHookUp)
			{
				batch.ErrorMessageEvent -= batchConsumer.OnErrorMessage;
				batch.MessageEvent -= batchConsumer.OnMessage;
				batch.NewResultSetEvent -= batchConsumer.OnNewResultSet;
				batch.CancellingEvent -= batchConsumer.OnCancelling;
				batch.FinishedResultSetEvent -= batchConsumer.OnFinishedProcessingResultSet;
				batch.SpecialActionEvent -= batchConsumer.OnSpecialAction;
				batch.StatementCompletedEvent -= batchConsumer.OnStatementCompleted;
				batch.StatementCompletedEvent -= OnStatementCompleted;
				// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
				batch.DataLoadedEvent -= DataLoadedEvent;
			}
			else
			{
				batch.ErrorMessageEvent += batchConsumer.OnErrorMessage;
				batch.MessageEvent += batchConsumer.OnMessage;
				batch.NewResultSetEvent += batchConsumer.OnNewResultSet;
				batch.CancellingEvent += batchConsumer.OnCancelling;
				batch.FinishedResultSetEvent += batchConsumer.OnFinishedProcessingResultSet;
				batch.SpecialActionEvent += batchConsumer.OnSpecialAction;
				// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
				batch.DataLoadedEvent += DataLoadedEvent;
				batch.StatementCompletedEvent += OnStatementCompleted;
				batch.StatementCompletedEvent += batchConsumer.OnStatementCompleted;
			}
		}
	}

	protected abstract void DoScriptExecution(ITextSpan textSpan);

	protected abstract EnScriptExecutionResult DoBatchExecution(QESQLBatch batch);

	protected virtual void OnStartBatchExecution(QESQLBatch batch)
	{
		Tracer.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		StartingBatchExecutionEvent?.Invoke(this, new QESQLStartingBatchEventArgs(-1, batch));
	}

	protected virtual void OnBatchExecutionCompleted(QESQLBatch batch, EnScriptExecutionResult batchResult)
	{
		Tracer.Trace(GetType(), "QESQLExec.OnBatchExecutionCompleted", "m_curBatchIndex = {0}, batchResult = {1}, _ExecState = {2}", _CurBatchIndex, batchResult, _ExecState);
		if (BatchExecutionCompletedEvent != null)
		{
			bool isParseOnly = false;
			bool withEstimatedPlan = false;
			if (_ExecOptions != null)
			{
				isParseOnly = _ExecOptions.ParseOnly;
				withEstimatedPlan = _ExecOptions.WithEstimatedExecutionPlan;
			}

			BatchExecutionCompletedEvent(this, new (batchResult, batch, withEstimatedPlan, isParseOnly));
		}
	}


	public virtual void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		Tracer.Trace(GetType(), "AbstractQESQLExec.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}, args.IsDebugging: {2}", sender, args.RecordCount, args.IsDebugging);

		// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
		StatementCompletedEvent?.Invoke(sender, args);

		// Added for hitching the text plan onto the sql query
		if (((DbCommand)sender).Connection.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal) && _ExecOptions.WithExecutionPlan && !_ExecOptions.WithEstimatedExecutionPlan)
		{
			lock (_LockObject)
			{
				if (_TextPlan == null)
					_TextPlan = "";
				else
					_TextPlan += "\n";

				_TextPlan += ((FbCommand)sender).GetCommandExplainedPlan();
			}
		}

	}


	protected virtual void OnExecutionCompleted(EnScriptExecutionResult execResult)
	{
		Tracer.Trace(GetType(), "QESQLExec::OnExecutionCompleted():  QESQLExec.OnExecutionCompleted", "execResult = {0}", execResult);
		bool withEstimatedPlan = false;
		bool isParseOnly = false;
		if (_ExecOptions != null)
		{
			withEstimatedPlan = _ExecOptions.WithEstimatedExecutionPlan;
			isParseOnly = _ExecOptions.ParseOnly;
		}

		try
		{
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
			Cleanup();
		}
		finally
		{
			ExecutionCompletedEvent?.Invoke(this, new ScriptExecutionCompletedEventArgs(execResult, withEstimatedPlan, isParseOnly));
		}
	}

	protected virtual void CompleteAsyncCancelOperation(ExecState stateBeforeCancelOp)
	{
	}

	protected virtual void Cleanup()
	{
		Tracer.Trace(GetType(), "QESQLExec.Cleanup", "", null);
		lock (_LockObject)
		{
			_ExecState = ExecState.Initial;
		}

		CleanupBatchCollection(_SetConnectionOptionsBatches);
		CleanupBatchCollection(_RestoreConnectionOptionsBatches);
		_CurBatchIndex = -1;
		if (_CurBatch != null && _BatchConsumer != null)
		{
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
		}

		_BatchConsumer = null;
		_ExecOptions = null;
		lock (_LockObject)
		{
			_ExecThread = null;
		}
	}

	protected virtual void Dispose(bool bDisposing)
	{
		Tracer.Trace(GetType(), "QESQLExec.Dispose", "bDisposing = {0}", bDisposing);
		if (bDisposing)
		{
			Cleanup();
			if (_CurBatch != null)
			{
				_CurBatch.Dispose();
				_CurBatch = null;
			}

			_LockObject = null;
		}
	}
}
