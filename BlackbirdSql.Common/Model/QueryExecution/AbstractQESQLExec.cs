// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLExec
using System;
using System.Data;
using System.Data.Common;
using System.Globalization;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Config;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core.Ctl.Diagnostics;
using FirebirdSql.Data.FirebirdClient;


namespace BlackbirdSql.Common.Model.QueryExecution;

public abstract class AbstractQESQLExec : IDisposable
{
	protected enum EnExecState
	{
		Initial,
		Executing,
		ExecutingBatch,
		Cancelling,
		Discarded
	}

	public const int C_Int1MB = 1048576;

	public const int C_Int5MB = 5242880;

	public const string C_PlanConnectionType = "FbConnection"; // "SqlCeConnection"


	public const int int2GB = int.MaxValue;


	protected QESQLBatchCollection _SetConnectionOptionsBatches = new QESQLBatchCollection();

	protected QESQLBatchCollection _RestoreConnectionOptionsBatches = new QESQLBatchCollection();

	protected IBQESQLBatchConsumer _BatchConsumer;

	protected IDbConnection _Conn;
	protected IDbConnection _SSconn;

	protected EnQESQLBatchSpecialAction _SpecialActions;

	protected int _ExecTimeout;

	private QESQLCommandBuilder _ExecOptions;

	protected IBTextSpan _TextSpan;

	private bool _ExecOptionHasBeenChanged;

	protected int _CurBatchIndex = -1;

	protected QESQLBatch _CurBatch;

	protected string _TextPlan = null;

	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;

	private Thread _ExecThread;

	protected EnExecState _ExecState;

	// A protected 'this' object lock
	protected object _LockObject = new object();

	private static readonly TimeSpan S_SleepTimeout = new TimeSpan(0, 0, 0, 0, 50);

	private AbstractConnectionStrategy ConnectionStrategy => QryMgr.ConnectionStrategy;

	protected QESQLCommandBuilder ExecOptions => _ExecOptions;

	protected QueryManager QryMgr { get; set; }


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

	public void Execute(IBTextSpan textSpan, IDbConnection conn, int execTimeout, IBQESQLBatchConsumer batchConsumer, QESQLCommandBuilder execOptions)
	{
		_Conn = conn;
		_BatchConsumer = batchConsumer;
		_ExecTimeout = execTimeout;
		// Tracer.Trace(GetType(), "QESQLExec.Execute", " execOptions.WithEstimatedExecutionPlan: " + execOptions.WithEstimatedExecutionPlan);
		_ExecOptions = execOptions.Clone() as QESQLCommandBuilder;
		_TextSpan = textSpan;
		_SpecialActions = EnQESQLBatchSpecialAction.None;

		if (_ExecState == EnExecState.Executing || _ExecState == EnExecState.ExecutingBatch)
		{
			InvalidOperationException ex = new(ControlsResources.ExecutionNotCompleted);
			Tracer.LogExThrow(GetType(), ex);
			throw ex;
		}

		_ExecState = EnExecState.Initial;
		_ExecThread = new(ProcessScript)
		{
			CurrentCulture = CultureInfo.CurrentCulture,
			CurrentUICulture = CultureInfo.CurrentUICulture,
			Name = "Batch Execution"
		};
		_ExecThread.Start();
	}

	public void Cancel(bool bSync, TimeSpan timeout)
	{
		// Tracer.Trace(GetType(), "QESQLExec.Cancel", "", null);
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
				Tracer.Trace(GetType(), Tracer.EnLevel.Warning, "QESQLExec.Cancel: execution thread won't cancel. Forgetting about it", "", null);
				HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
				lock (_LockObject)
				{
					_ExecState = EnExecState.Discarded;
				}

				return;
			}

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLExec.Cancel: thread stopped gracefully", "", null);
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
		// Tracer.Trace(GetType(), "QESQLExec.Dispose", "", null);
		Dispose(bDisposing: true);
	}

	private void ProcessScript()
	{
		try
		{
			// Tracer.Trace(GetType(), "QESQLExec.ProcessScript");
			ProcessExecOptions(_Conn);
			_ExecOptionHasBeenChanged = false;
			_ExecResult = EnScriptExecutionResult.Failure;
			_CurBatchIndex = 0;
			_CurBatch.ExecTimeout = _ExecTimeout;
			_CurBatch.SetSuppressProviderMessageHeaders(ExecOptions.SuppressProviderMessageHeaders);
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: true);

			lock (_LockObject)
			{
				if (_ExecState == EnExecState.Cancelling)
				{
					OnExecutionCompleted(EnScriptExecutionResult.Cancel, false);
					return;
				}

				_ExecState = EnExecState.Executing;
			}

			_ExecResult = SetRestoreConnectionOptions(bSet: true, _Conn);

			if (_ExecResult == EnScriptExecutionResult.Success)
				ExecuteScript(_TextSpan);

			bool discarded = false;
			lock (_LockObject)
			{
				discarded = _ExecState == EnExecState.Discarded;
			}

			if (discarded)
			{
				Tracer.Trace(GetType(), Tracer.EnLevel.Warning, "QESQLExec.ProcessScript", "execution was discarded");
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

			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessScript", "execution was NOT discarded");
			// EstimatedExecutionPlan = gets the plan without executing the script. MS still seem to execute the script.
			// We have bypassed that and get the plan at ExecuteReader above.
			// ActualExecutionPlan = the WithExecutionPlan toggle is latched so we can get the actual because
			// ExecuteReader has been called.
			if (_ExecResult == EnScriptExecutionResult.Success && _Conn.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal) && ExecOptions.WithExecutionPlan && !ExecOptions.WithEstimatedExecutionPlan)
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

			bool isTextResult = _BatchConsumer is ResultsToTextOrFileBatchConsumer;

			OnExecutionCompleted(_ExecResult, isTextResult);
		}
		catch (Exception e)
		{
			Tracer.LogExCatch(GetType(), e);
			_ExecResult = EnScriptExecutionResult.Failure;
			OnExecutionCompleted(_ExecResult, false);
		}
	}

	private void StartCancelling()
	{
		// Tracer.Trace(GetType(), "QESQLExec.StartCancelling", "", null);
		EnExecState execState;
		lock (_LockObject)
		{
			execState = _ExecState;
			_ExecState = EnExecState.Cancelling;
			if (execState == EnExecState.ExecutingBatch)
			{
				_CurBatch.Cancel();
			}
		}

		CompleteAsyncCancelOperation(execState);
		// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.StartCancelling: returning", "", null);
	}

	private void CleanupBatchCollection(QESQLBatchCollection col)
	{
		// Tracer.Trace(GetType(), "QESQLExec.CleanupBatchCollection", "", null);
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
		// Tracer.Trace(GetType(), "QESQLExec.PostProcessSqlCeExecutionPlan", "", null);

		QESQLBatch qESQLBatch;

		// We do all this logically because the FirebirdSql client does not currently support any of the required commands.
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

	/// <summary>
	/// We handle execution plan settings programmatically through FBCommand so these
	/// SET commands won't apply even though they exist in SqlResources.
	/// We just clear the batch collection after setting them for brevity.
	/// </summary>
	/// <param name="dbConnection"></param>
	protected void ProcessExecOptions(IDbConnection dbConnection)
	{

		// Tracer.Trace(GetType(), "QESQLExec.ProcessExecOptions");

		_ExecOptionHasBeenChanged = true;

		StringBuilder stringBuilder = new StringBuilder(128);
		StringBuilder stringBuilder2 = new StringBuilder(128);

		CleanupBatchCollection(_SetConnectionOptionsBatches);
		CleanupBatchCollection(_RestoreConnectionOptionsBatches);

		_SpecialActions = EnQESQLBatchSpecialAction.None;
		string cmd1, cmd2, cmd3, cmd4;
		if (dbConnection.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal))
		{
			if (ExecOptions.WithEstimatedExecutionPlan)
			{
				cmd1 = ExecOptions.EditorExecutionSetPlanXml.SqlCmd(true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd1, _ExecTimeout, QryMgr));
				cmd2 = ExecOptions.EditorExecutionSetNoExec.SqlCmd(true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd2, _ExecTimeout, QryMgr));
				cmd3 = ExecOptions.EditorExecutionSetPlanXml.SqlCmd();
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd3, _ExecTimeout, QryMgr));
				cmd4 = ExecOptions.EditorExecutionSetNoExec.SqlCmd();
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd4, _ExecTimeout, QryMgr));
			}
			else if (ExecOptions.WithExecutionPlan)
			{
				cmd1 = ExecOptions.EditorExecutionSetStats.SqlCmd(true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd1, _ExecTimeout, QryMgr));
				cmd2 = ExecOptions.EditorExecutionSetPlanXml.SqlCmd(true);
				_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd2, _ExecTimeout, QryMgr));
				cmd3 = ExecOptions.EditorExecutionSetStats.SqlCmd();
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd3, _ExecTimeout, QryMgr));
				cmd4 = ExecOptions.EditorExecutionSetPlanXml.SqlCmd();
				_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd4, _ExecTimeout, QryMgr));
			}

			// Above left in for brevity. Just clear. We handle some in FbCommand logically
			// but most are isql and not supported by the Firebird client.
			CleanupBatchCollection(_SetConnectionOptionsBatches);
			CleanupBatchCollection(_RestoreConnectionOptionsBatches);

			return;
		}

		int major = ConnectionStrategy.GetServerVersion().Major;
		if (ExecOptions.WithNoExec)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting noexec off");
			stringBuilder2.AppendFormat("{0} ", ExecOptions.EditorExecutionSetNoExec.SqlCmd());
		}

		if (ExecOptions.WithStatisticsIO)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics io");
			stringBuilder.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStatisticsIO.SqlCmd(true));
			stringBuilder2.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStatisticsIO.SqlCmd());
		}

		if (ExecOptions.WithStatisticsTime)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics time");
			// stringBuilder.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStatisticsTime.SqlCmd(true));
			// stringBuilder2.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStatisticsTime.SqlCmd());
		}

		if (ExecOptions.WithDebugging)
		{
			_SpecialActions |= EnQESQLBatchSpecialAction.ExecuteWithDebugging;
		}

		if (ConnectionStrategy.IsExecutionPlanAndQueryStatsSupported)
		{
			if (ExecOptions.WithEstimatedExecutionPlan)
			{
				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting estimated execution plan");
				_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
				if (major >= -1 /* 9 */) // Always
				{
					_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true,
						ExecOptions.EditorExecutionSetPlanXml.SqlCmd(true), _ExecTimeout, QryMgr));
					_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true,
						ExecOptions.EditorExecutionSetNoExec.SqlCmd(true), _ExecTimeout, QryMgr));

					_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true,
						ExecOptions.EditorExecutionSetPlanXml.SqlCmd(), _ExecTimeout, QryMgr));
					_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true,
						ExecOptions.EditorExecutionSetNoExec.SqlCmd(), _ExecTimeout, QryMgr));
					
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedExecutionPlan;
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedYukonXmlExecutionPlan;
				}
				else
				{
					// _SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true,
					//		ExecOptions.EditorExecutionSetExecutionPlanAll.SqlCmd(true), _ExecTimeout, QryMgr));
					// _RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true,
					//		ExecOptions.EditorExecutionSetExecutionPlanAll.SqlCmd(), _ExecTimeout, QryMgr));
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedExecutionPlan;
				}
			}
			else if (ExecOptions.WithStatisticsProfile || ExecOptions.WithExecutionPlan)
			{
				// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics profile");
				if (major >= -1 /* 9 */)
				{
					stringBuilder.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStats.SqlCmd(true));
					stringBuilder2.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStats.SqlCmd());
					if (ExecOptions.WithExecutionPlan)
					{
						_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualExecutionPlan;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualYukonXmlExecutionPlan;
					}
				}
				else
				{
					stringBuilder.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStats.SqlCmd(true));
					stringBuilder2.AppendFormat("{0} ", ExecOptions.EditorExecutionSetStats.SqlCmd());
					if (ExecOptions.WithExecutionPlan)
					{
						_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualExecutionPlan;
					}
				}
			}
		}

		if (ExecOptions.ParseOnly)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting parseonly");
			stringBuilder.AppendFormat("{0} ", ExecOptions.EditorExecutionSetParseOnly.SqlCmd(true));
			stringBuilder2.AppendFormat("{0} ", ExecOptions.EditorExecutionSetParseOnly.SqlCmd());
		}

		if (ExecOptions.WithNoExec)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting noexec on");
			stringBuilder.AppendFormat("{0} ", ExecOptions.EditorExecutionSetNoExec.SqlCmd(true));
		}

		cmd1 = stringBuilder.ToString().Trim();
		cmd2 = stringBuilder2.ToString().Trim();
		if (cmd1.Length != 0)
		{
			_SetConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd1, _ExecTimeout, QryMgr));
			_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, cmd2, _ExecTimeout, QryMgr));
		}

		if (ConnectionStrategy.IsExecutionPlanAndQueryStatsSupported && ExecOptions.WithExecutionPlanText && !ExecOptions.WithStatisticsProfile && !ExecOptions.WithExecutionPlan && !ExecOptions.WithEstimatedExecutionPlan)
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessExecOptions", "setting showplan_text");
			_SetConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true,
				ExecOptions.EditorExecutionSetShowplanText.SqlCmd(true), _ExecTimeout, QryMgr));
			_RestoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true,
				ExecOptions.EditorExecutionSetShowplanText.SqlCmd(), _ExecTimeout, QryMgr));
		}

		// Above left in for brevity. Just clear. We handle some in FbCommand logically
		// but most are isql and not supported by the Firebird client.
		CleanupBatchCollection(_SetConnectionOptionsBatches);
		CleanupBatchCollection(_RestoreConnectionOptionsBatches);

	}

	protected EnScriptExecutionResult SetRestoreConnectionOptions(bool bSet, IDbConnection connection)
	{
		// Tracer.Trace(GetType(), "QESQLExec.SetRestoreConnectionOptions", "bSet = {0}", bSet);
		QESQLBatchCollection qESQLBatchCollection = bSet ? _SetConnectionOptionsBatches : _RestoreConnectionOptionsBatches;
		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Success;
		for (int i = 0; i < qESQLBatchCollection.Count; i++)
		{
			if (connection.State != ConnectionState.Open)
			{
				break;
			}

			QESQLBatch qESQLBatch = qESQLBatchCollection[i];

			if (!QESQLCommandBuilder.IsSupported(qESQLBatch))
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

	protected void ProcessBatchCommand(string sqlScript, IBTextSpan textSpan, out bool continueProcessing)
	{
		// Tracer.Trace(GetType(), "QESQLExec.ProcessBatchCommand", " ExecOptions.WithEstimatedExecutionPlan: " + ExecOptions.WithEstimatedExecutionPlan);

		continueProcessing = true;

		if (sqlScript.Trim().Length <= 0)
		{
			_ExecResult |= EnScriptExecutionResult.Success;
			return;
		}

		lock (_LockObject)
		{
			if (_ExecState == EnExecState.Cancelling)
			{
				_ExecResult = EnScriptExecutionResult.Cancel;
			}
			else
			{
				_CurBatch.Reset();
				_CurBatch.SqlScript = sqlScript;
				_CurBatch.TextSpan = textSpan;
				_CurBatch.BatchIndex = _CurBatchIndex;
				_CurBatchIndex++;
				_ExecState = EnExecState.ExecutingBatch;
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
				scriptExecutionResult = ExecuteBatchCommand(_CurBatch);
			}
			finally
			{
				lock (_LockObject)
				{
					// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "QESQLExec.ProcessBatchCommand", "execState = {0}", _ExecState);
					discarded = _ExecState == EnExecState.Discarded;
					if (_ExecState == EnExecState.Cancelling || discarded)
					{
						scriptExecutionResult = EnScriptExecutionResult.Cancel;
					}
					else
					{
						_ExecState = EnExecState.Executing;
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

	protected void HookupBatchWithConsumer(QESQLBatch batch, IBQESQLBatchConsumer batchConsumer, bool bHookUp)
	{
		// Tracer.Trace(GetType(), "QESQLExec.HookupBatchWithConsumer", "bHookUp = {0}", bHookUp);
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

	protected abstract void ExecuteScript(IBTextSpan textSpan);

	protected abstract EnScriptExecutionResult ExecuteBatchCommand(QESQLBatch batch);

	protected virtual void OnStartBatchExecution(QESQLBatch batch)
	{
		// Tracer.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		StartingBatchExecutionEvent?.Invoke(this, new QESQLStartingBatchEventArgs(-1, batch));
	}

	protected virtual void OnBatchExecutionCompleted(QESQLBatch batch, EnScriptExecutionResult batchResult)
	{
		// Tracer.Trace(GetType(), "QESQLExec.OnBatchExecutionCompleted", "m_curBatchIndex = {0}, batchResult = {1}, _ExecState = {2}", _CurBatchIndex, batchResult, _ExecState);
		if (BatchExecutionCompletedEvent != null)
		{
			bool isParseOnly = false;
			bool withEstimatedPlan = false;
			if (ExecOptions != null)
			{
				isParseOnly = ExecOptions.ParseOnly;
				withEstimatedPlan = ExecOptions.WithEstimatedExecutionPlan;
			}

			bool isTextResult = !isParseOnly && (_BatchConsumer is ResultsToTextOrFileBatchConsumer);

			BatchExecutionCompletedEvent(this, new (batchResult, batch, withEstimatedPlan, isParseOnly, isTextResult));
		}
	}


	public virtual void OnStatementCompleted(object sender, QESQLStatementCompletedEventArgs args)
	{
		// Tracer.Trace(GetType(), "AbstractQESQLExec.OnStatementCompleted", "sender: {0}, args.RecordCount: {1}, args.IsDebugging: {2}", sender, args.RecordCount, args.IsDebugging);

		// Added for StaticsPanel.RetrieveStatisticsIfNeeded();
		StatementCompletedEvent?.Invoke(sender, args);

		// Added for hitching the text plan onto the sql query
		if (((DbCommand)sender).Connection.GetType().ToString().EndsWith(C_PlanConnectionType, StringComparison.Ordinal) && ExecOptions.WithExecutionPlan && !ExecOptions.WithEstimatedExecutionPlan)
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


	protected virtual void OnExecutionCompleted(EnScriptExecutionResult execResult, bool isTextResults)
	{
		// Tracer.Trace(GetType(), "QESQLExec::OnExecutionCompleted():  QESQLExec.OnExecutionCompleted", "execResult = {0}", execResult);
		bool withEstimatedPlan = false;
		bool isParseOnly = false;
		if (ExecOptions != null)
		{
			withEstimatedPlan = ExecOptions.WithEstimatedExecutionPlan;
			isParseOnly = ExecOptions.ParseOnly;
		}

		try
		{
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
			Cleanup();
		}
		finally
		{
			ExecutionCompletedEvent?.Invoke(this, new ScriptExecutionCompletedEventArgs(execResult, withEstimatedPlan, isParseOnly, isTextResults));
		}
	}

	protected virtual void CompleteAsyncCancelOperation(EnExecState stateBeforeCancelOp)
	{
	}

	protected virtual void Cleanup()
	{
		// Tracer.Trace(GetType(), "QESQLExec.Cleanup", "", null);
		lock (_LockObject)
		{
			_ExecState = EnExecState.Initial;
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
		// Tracer.Trace(GetType(), "QESQLExec.Dispose", "bDisposing = {0}", bDisposing);
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
