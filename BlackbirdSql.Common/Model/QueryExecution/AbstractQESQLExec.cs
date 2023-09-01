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

using BlackbirdSql.Core.Diagnostics;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Core;
using FirebirdSql.Data.FirebirdClient;
using BlackbirdSql.Common.Interfaces;

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

	public const int int2GB = int.MaxValue;


	protected QESQLBatchCollection _setConnectionOptionsBatches = new QESQLBatchCollection();

	protected QESQLBatchCollection _restoreConnectionOptionsBatches = new QESQLBatchCollection();

	protected IQESQLBatchConsumer _BatchConsumer;

	protected IDbConnection _Conn;
	protected IDbConnection _SSconn;

	protected EnQESQLBatchSpecialAction _SpecialActions;

	protected int _execTimeout;

	protected QESQLExecutionOptions _ExecOptions;

	protected ITextSpan _TextSpan;

	private bool _ExecOptionHasBeenChanged;

	protected int _CurBatchIndex = -1;

	protected QESQLBatch _CurBatch;

	protected EnScriptExecutionResult _ExecResult = EnScriptExecutionResult.Failure;

	private Thread _ExecThread;

	protected ExecState _ExecState;

	protected object _StateCritSection = new object();

	private static readonly TimeSpan S_SleepTimeout = new TimeSpan(0, 0, 0, 0, 50);

	private ConnectionStrategy ConnectionStrategy => QueryExecutor.ConnectionStrategy;

	private QueryExecutor QueryExecutor { get; set; }

	public static int DefaultMaxCharsPerColumnForGrid => C_Int64K;

	public static int DefaultMaxCharsPerColumnForText => 256;

	public static int DefaultMaxCharsPerColumnForXml => 2097152;

	public static int DefaultTextSize => int.MaxValue;

	public static int DefaultSqlExecTimeout => 0;

	public static string DefaultBatchSeparator => "GO";

	public event ScriptExecutionCompletedEventHandler ExecutionCompleted;

	public event QESQLBatchExecutedEventHandler BatchExectionCompleted;

	public event QESQLStartingBatchEventHandler StartingBatchExecution;

	public AbstractQESQLExec(QueryExecutor queryExecutor)
	{
		QueryExecutor = queryExecutor;
		_CurBatch = new(QueryExecutor)
		{
			NoResultsExpected = false
		};
	}

	public void Execute(ITextSpan textSpan, IDbConnection conn, int execTimeout, IQESQLBatchConsumer batchConsumer, QESQLExecutionOptions execOptions)
	{
		Tracer.Trace(GetType(), "QESQLExec.Execute", "", null);
		_Conn = conn;
		_BatchConsumer = batchConsumer;
		_execTimeout = execTimeout;
		_ExecOptions = execOptions.Clone() as QESQLExecutionOptions;
		_TextSpan = textSpan;
		_SpecialActions = EnQESQLBatchSpecialAction.None;

		if (_ExecState == ExecState.Executing || _ExecState == ExecState.ExecutingBatch)
		{
			InvalidOperationException ex = new(SharedResx.ExecutionNotCompleted);
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
			lock (_StateCritSection)
			{
				thread2 = _ExecThread;
			}

			while (timeSpan < timeout && thread2 != null && thread2.IsAlive)
			{
				Thread.Sleep(S_SleepTimeout);
				Application.DoEvents();
				timeSpan += S_SleepTimeout;
				lock (_StateCritSection)
				{
					thread2 = _ExecThread;
				}
			}

			if (thread2 != null && thread2.IsAlive)
			{
				Tracer.Trace(GetType(), Tracer.Level.Warning, "QESQLExec.Cancel: execution thread won't cancel. Forgetting about it", "", null);
				HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
				lock (_StateCritSection)
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
			Tracer.Trace(GetType(), "QESQLExec.StartExecuting", "", null);
			ProcessExecOptions(_Conn);
			_ExecOptionHasBeenChanged = false;
			_ExecResult = EnScriptExecutionResult.Failure;
			_CurBatchIndex = 0;
			_CurBatch.ExecTimeout = _execTimeout;
			_CurBatch.SetSuppressProviderMessageHeaders(_ExecOptions.SuppressProviderMessageHeaders);
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: true);

			lock (_StateCritSection)
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
			{
				DoScriptExecution(_TextSpan);
			}

			bool flag = false;
			lock (_StateCritSection)
			{
				flag = _ExecState == ExecState.Discarded;
			}

			if (flag)
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
			if (_ExecResult == EnScriptExecutionResult.Success && _Conn.GetType().ToString().EndsWith("SqlCeConnection", StringComparison.Ordinal) && (_ExecOptions.WithExecutionPlan || _ExecOptions.WithEstimatedExecutionPlan))
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
		lock (_StateCritSection)
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
		QESQLBatch qESQLBatch = new QESQLBatch(bNoResultsExpected: false, "SELECT @@showplan", _execTimeout, QueryExecutor);
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
		Tracer.Trace(GetType(), "QESQLExec.ProcessExecOptions", "", null);
		_ExecOptionHasBeenChanged = true;
		StringBuilder stringBuilder = new StringBuilder(128);
		StringBuilder stringBuilder2 = new StringBuilder(128);
		CleanupBatchCollection(_setConnectionOptionsBatches);
		CleanupBatchCollection(_restoreConnectionOptionsBatches);
		_SpecialActions = EnQESQLBatchSpecialAction.None;
		string empty;
		string empty2;
		if (dbConnection.GetType().ToString().EndsWith("SqlCeConnection", StringComparison.Ordinal))
		{
			if (_ExecOptions.WithEstimatedExecutionPlan)
			{
				empty = QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: true);
				_setConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty, _execTimeout, QueryExecutor));
				empty2 = QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: false);
				_restoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty2, _execTimeout, QueryExecutor));
			}
			else if (_ExecOptions.WithExecutionPlan)
			{
				empty = QESQLExecutionOptions.GetSetStatisticsXml(bOn: true);
				_setConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty, _execTimeout, QueryExecutor));
				empty2 = QESQLExecutionOptions.GetSetStatisticsXml(bOn: false);
				_restoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty2, _execTimeout, QueryExecutor));
			}

			return;
		}

		int major = ConnectionStrategy.GetServerVersion().Major;
		if (_ExecOptions.WithNoExec)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting noexec off");
			stringBuilder2.AppendFormat("{0} ", QESQLExecutionOptions.GetSetNoExecString(bOn: false));
		}

		if (_ExecOptions.WithStatisticsIO)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics io");
			stringBuilder.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsIOString(bOn: true));
			stringBuilder2.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsIOString(bOn: false));
		}

		if (_ExecOptions.WithStatisticsTime)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics time");
			stringBuilder.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsTimeString(bOn: true));
			stringBuilder2.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsTimeString(bOn: false));
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
				if (major >= 9)
				{
					_setConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: true), _execTimeout, QueryExecutor));
					_restoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanXmlString(on: false), _execTimeout, QueryExecutor));
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedYukonXmlExecutionPlan;
				}
				else
				{
					_setConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanAllString(on: true), _execTimeout, QueryExecutor));
					_restoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetExecutionPlanAllString(on: false), _execTimeout, QueryExecutor));
					_SpecialActions |= EnQESQLBatchSpecialAction.ExpectEstimatedExecutionPlan;
				}
			}
			else if (_ExecOptions.WithStatisticsProfile || _ExecOptions.WithExecutionPlan)
			{
				Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting statistics profile");
				if (major >= 9)
				{
					stringBuilder.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsXml(bOn: true));
					stringBuilder2.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsXml(bOn: false));
					if (_ExecOptions.WithExecutionPlan)
					{
						_SpecialActions &= ~EnQESQLBatchSpecialAction.ExecutionPlanMask;
						_SpecialActions |= EnQESQLBatchSpecialAction.ExpectActualYukonXmlExecutionPlan;
					}
				}
				else
				{
					stringBuilder.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsProfileString(bOn: true));
					stringBuilder2.AppendFormat("{0} ", QESQLExecutionOptions.GetSetStatisticsProfileString(bOn: false));
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
			stringBuilder.AppendFormat("{0} ", QESQLExecutionOptions.GetSetParseOnlyString(bOn: true));
			stringBuilder2.AppendFormat("{0} ", QESQLExecutionOptions.GetSetParseOnlyString(bOn: false));
		}

		if (_ExecOptions.WithNoExec)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting noexec on");
			stringBuilder.AppendFormat("{0} ", QESQLExecutionOptions.GetSetNoExecString(bOn: true));
		}

		empty = stringBuilder.ToString().Trim();
		empty2 = stringBuilder2.ToString().Trim();
		if (empty.Length != 0)
		{
			_setConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty, _execTimeout, QueryExecutor));
			_restoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, empty2, _execTimeout, QueryExecutor));
		}

		if (ConnectionStrategy.IsExecutionPlanAndQueryStatsSupported && _ExecOptions.WithExecutionPlanText && !_ExecOptions.WithStatisticsProfile && !_ExecOptions.WithExecutionPlan && !_ExecOptions.WithEstimatedExecutionPlan)
		{
			Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ProcessExecOptions", "setting showplan_text");
			_setConnectionOptionsBatches.Insert(0, new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetShowplanTextString(bOn: true), _execTimeout, QueryExecutor));
			_restoreConnectionOptionsBatches.Add(new QESQLBatch(bNoResultsExpected: true, QESQLExecutionOptions.GetSetShowplanTextString(bOn: false), _execTimeout, QueryExecutor));
		}
	}

	protected EnScriptExecutionResult SetRestoreConnectionOptions(bool bSet, IDbConnection connection)
	{
		Tracer.Trace(GetType(), "QESQLExec.SetRestoreConnectionOptions", "bSet = {0}", bSet);
		QESQLBatchCollection qESQLBatchCollection = bSet ? _setConnectionOptionsBatches : _restoreConnectionOptionsBatches;
		EnScriptExecutionResult scriptExecutionResult = EnScriptExecutionResult.Success;
		for (int i = 0; i < qESQLBatchCollection.Count; i++)
		{
			if (connection.State != ConnectionState.Open)
			{
				break;
			}

			QESQLBatch qESQLBatch = qESQLBatchCollection[i];
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
		Tracer.Trace(GetType(), "QESQLExec.ExecuteBatchCommon", "", null);
		continueProcessing = true;
		if (batch.Trim().Length <= 0)
		{
			_ExecResult |= EnScriptExecutionResult.Success;
			return;
		}

		lock (_StateCritSection)
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
			bool flag = false;
			try
			{
				OnStartBatchExecution(_CurBatch);
				scriptExecutionResult = DoBatchExecution(_CurBatch);
			}
			finally
			{
				lock (_StateCritSection)
				{
					Tracer.Trace(GetType(), Tracer.Level.Verbose, "QESQLExec.ExecuteBatchCommon", "execState = {0}", _ExecState);
					flag = _ExecState == ExecState.Discarded;
					if (_ExecState == ExecState.Cancelling || flag)
					{
						scriptExecutionResult = EnScriptExecutionResult.Cancel;
					}
					else
					{
						_ExecState = ExecState.Executing;
					}
				}
			}

			if (!flag)
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
				batch.ErrorMessage -= batchConsumer.OnErrorMessage;
				batch.Message -= batchConsumer.OnMessage;
				batch.NewResultSet -= batchConsumer.OnNewResultSet;
				batch.Cancelling -= batchConsumer.OnCancelling;
				batch.FinishedResultSet -= batchConsumer.OnFinishedProcessingResultSet;
				// batch.SpecialAction -= batchConsumer.OnSpecialAction;
				batch.StatementCompleted -= batchConsumer.OnStatementCompleted;
			}
			else
			{
				batch.ErrorMessage += batchConsumer.OnErrorMessage;
				batch.Message += batchConsumer.OnMessage;
				batch.NewResultSet += batchConsumer.OnNewResultSet;
				batch.Cancelling += batchConsumer.OnCancelling;
				batch.FinishedResultSet += batchConsumer.OnFinishedProcessingResultSet;
				// batch.SpecialAction += batchConsumer.OnSpecialAction;
				batch.StatementCompleted += batchConsumer.OnStatementCompleted;
			}
		}
	}

	protected abstract void DoScriptExecution(ITextSpan textSpan);

	protected abstract EnScriptExecutionResult DoBatchExecution(QESQLBatch batch);

	protected virtual void OnStartBatchExecution(QESQLBatch batch)
	{
		Tracer.Trace(GetType(), "QESQLExec.OnStartBatchExecution", "m_curBatchIndex = {0}", _CurBatchIndex);
		StartingBatchExecution?.Invoke(this, new QESQLStartingBatchEventArgs(-1, batch));
	}

	protected virtual void OnBatchExecutionCompleted(QESQLBatch batch, EnScriptExecutionResult batchResult)
	{
		Tracer.Trace(GetType(), "QESQLExec.OnBatchExecutionCompleted", "m_curBatchIndex = {0}, batchResult = {1}, _ExecState = {2}", _CurBatchIndex, batchResult, _ExecState);
		if (BatchExectionCompleted != null)
		{
			bool isParseOnly = false;
			bool withEstimatedPlan = false;
			if (_ExecOptions != null)
			{
				isParseOnly = _ExecOptions.ParseOnly;
				withEstimatedPlan = _ExecOptions.WithEstimatedExecutionPlan;
			}

			BatchExectionCompleted(this, new QESQLBatchExecutedEventArgs(batchResult, batch, withEstimatedPlan, isParseOnly));
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
			ExecutionCompleted?.Invoke(this, new ScriptExecutionCompletedEventArgs(execResult, withEstimatedPlan, isParseOnly));
		}
	}

	protected virtual void CompleteAsyncCancelOperation(ExecState stateBeforeCancelOp)
	{
	}

	protected virtual void Cleanup()
	{
		Tracer.Trace(GetType(), "QESQLExec.Cleanup", "", null);
		lock (_StateCritSection)
		{
			_ExecState = ExecState.Initial;
		}

		CleanupBatchCollection(_setConnectionOptionsBatches);
		CleanupBatchCollection(_restoreConnectionOptionsBatches);
		_CurBatchIndex = -1;
		if (_CurBatch != null && _BatchConsumer != null)
		{
			HookupBatchWithConsumer(_CurBatch, _BatchConsumer, bHookUp: false);
		}

		_BatchConsumer = null;
		_ExecOptions = null;
		lock (_StateCritSection)
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

			_StateCritSection = null;
		}
	}
}
