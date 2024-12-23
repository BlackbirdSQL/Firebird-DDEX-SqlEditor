﻿// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatch

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Forms;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Core.Interfaces;
using BlackbirdSql.Shared.Enums;
using BlackbirdSql.Shared.Events;
using BlackbirdSql.Shared.Interfaces;
using BlackbirdSql.Shared.Model.QueryExecution;
using BlackbirdSql.Shared.Properties;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Shared.Ctl.QueryExecution;


// =========================================================================================================
//										QESQLBatch Class
//
/// <summary>
/// Batch processing class.
/// </summary>
// =========================================================================================================
public class QESQLBatch : IBsDataReaderHandler, IDisposable
{

	// --------------------------------------------
	#region Constructors / Destructors - QESQLBatch
	// --------------------------------------------


	public QESQLBatch(QueryManager qryMgr)
	{
		_QryMgr = qryMgr;
	}



	public void Dispose()
	{
		// Evs.Trace(GetType(), nameof(Dispose), "", null);

		Dispose(true);
		GC.SuppressFinalize(this);
	}



	protected virtual void Dispose(bool disposing)
	{
		// Evs.Trace(GetType(), "Dispose(bool)", "disposing = {0}", disposing);

		if (!disposing)
			return;

		lock (_LockLocal)
		{
			if (_ActiveResultSet != null)
			{
				_ActiveResultSet.Dispose();
				_ActiveResultSet = null;
			}
		}
	}


	#endregion Constructors / Destructors





	// =========================================================================================================
	#region Fields and Constants - QESQLBatch
	// =========================================================================================================


	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	private QueryResultSet _ActiveResultSet;
	private int _BatchIndex;
	private bool _ContainsErrors = false;
	private int _ExecTimeout = SysConstants.C_DefaultCommandTimeout;
	private EnBatchState _ExecutionState;
	private bool _NoResultsExpected;
	private readonly QueryManager _QryMgr;
	private long _RowsAffected;
	private EnSpecialActions _SpecialActions;
	private IBsNativeDbStatementWrapper _SqlStatement = null;
	// private bool _SuppressProviderMessageHeaders;
	private IBsTextSpan _TextSpan;
	private long _TotalRowsAffected;


	#endregion Fields and Constants





	// =========================================================================================================
	#region Property Accessors - QESQLBatch
	// =========================================================================================================


	public IBsNativeDbStatementWrapper SqlStatement
	{
		get { return _SqlStatement; }
		set { _SqlStatement = value; }
	}


	public bool NoResultsExpected
	{
		set { _NoResultsExpected = value; }
	}


	public int ExecTimeout
	{
		get { return _ExecTimeout; }
		set { _ExecTimeout = value; }
	}


	public int BatchIndex
	{
		get { return _BatchIndex; }
		set { _BatchIndex = value; }
	}


	private QueryManager QryMgr => _QryMgr;

	public long RowsAffected => _RowsAffected;




	public event EventHandler CancellingEvent;
	public event BatchErrorMessageEventHandler ErrorMessageEvent;
	public event EventHandler FinishedResultSetEvent;
	public event BatchMessageEventHandler MessageEvent;
	public event BatchNewResultSetEventHandler NewResultSetEventAsync;
	public event BatchSpecialActionEventHandler SpecialActionEvent;
	public event BatchStatementCompletedEventHandler StatementCompletedEvent;


	#endregion Property Accessors





	// =========================================================================================================
	#region Methods - QESQLBatch
	// =========================================================================================================


	private bool CheckCancelled(CancellationToken cancelToken)
	{
		lock (_LockLocal)
		{
			if (_ExecutionState == EnBatchState.Cancelling)
				return true;

			// Evs.Trace(GetType(), "QESQLBatch.Cancel", "Cancelled: {0}.", cancelToken.Cancelled());

			if (!cancelToken.Cancelled())
				return false;

			_ExecutionState = EnBatchState.Cancelling;

			if (_ActiveResultSet != null)
			{
				CancellingEvent?.Invoke(this, new EventArgs());

				// Evs.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: calling InitiateStopRetrievingData", "", null);
				_ActiveResultSet.InitiateStopRetrievingData();
				// Evs.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: InitiateStopRetrievingData returned", "", null);
			}

			// Evs.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: executing Cancel command", "", null);
			/*
			try
			{
				_SqlStatement?.Cancel();
			}
			catch (Exception e2)
			{
				Diag.Ex(e2);
			}
			*/

			// Evs.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: Cancel command returned", "", null);
		}

		return true;
	}



	public async Task<EnScriptExecutionResult> ExecuteAsync(IDbConnection conn, EnSpecialActions specialActions,
		CancellationToken cancelToken, CancellationToken syncToken)
	{
		// Evs.Trace(GetType(), nameof(ExecuteAsync), "conn.State = {0}", conn.State);
		lock (_LockLocal)
		{
			if (CheckCancelled(cancelToken))
			{
				_ExecutionState = EnBatchState.Initial;
				return EnScriptExecutionResult.Cancel;
			}
		}

		if (_ActiveResultSet != null)
			_ActiveResultSet = null;

		_SpecialActions = specialActions;


		// Evs.Trace(GetType(), nameof(ExecuteAsync), " Creating command - _SpecialActions: " + _SpecialActions);

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;

		lock (_LockLocal)
			_ExecutionState = EnBatchState.Executing;


		try
		{
			// ----------------------------------------------------------------------------------------- //
			// **************** Final 4Execution Point (13) - QESQLBatch.ExecuteAsync() ***************** //
			// ----------------------------------------------------------------------------------------- //

			// Evs.Trace(GetType(), nameof(ExecuteAsync), "Async Executing _SqlStatement.");

			try
			{
				await _SqlStatement.ExecuteAsync(false, cancelToken);
			}
			catch (Exception ex)
			{
				if (cancelToken.Cancelled())
					Diag.Expected(ex);
				throw;
			}


			// Evs.Trace(GetType(), nameof(ExecuteAsync), "Executed _SqlStatement.");

			// Evs.Trace(GetType(), nameof(ExecuteAsync), "Executed _SqlStatement. ExecutionType: {0}, CurrentAction: {1}, CurrentActionReader: {2}, HasRows: {3}.",
			//	_SqlStatement.ExecutionType, _SqlStatement.CurrentAction, _SqlStatement.CurrentActionReader,
			//	_SqlStatement.CurrentActionReader == null ? "null" : (_SqlStatement.CurrentActionReader.HasRows ? "HasRows" : "NoRows" ));

			if (CheckCancelled(cancelToken))
				return EnScriptExecutionResult.Cancel;


			_NoResultsExpected = _SqlStatement.CurrentActionReader == null || !_SqlStatement.CurrentActionReader.HasRows;

			int statementIndex = _SqlStatement.Index;
			int statementCount = _SqlStatement.StatementCount;
			long rowsSelected = _SqlStatement.RowsSelected;
			long totalRowsSelected = _SqlStatement.TotalRowsSelected;
			bool isSpecialAction = _SqlStatement.IsSpecialAction;


			if (_NoResultsExpected && !isSpecialAction)
			{
				// result = batchResult;

				if (result != EnScriptExecutionResult.Failure)
				{
					RaiseStatementCompleted(statementIndex, rowsSelected, totalRowsSelected, false);

					// Evs.Trace(GetType(), nameof(ExecuteAsync), " ExecuteNonQuery returned!");
					lock (_LockLocal)
					{
						if (CheckCancelled(cancelToken))
						{
							result = EnScriptExecutionResult.Cancel;
						}
						else
						{
							result = EnScriptExecutionResult.Success;
							_ExecutionState = EnBatchState.Executed;
						}
					}
				}
			}
			else
			{
				IDataReader dataReader = _SqlStatement.CurrentActionReader;

				// Evs.Trace(GetType(), nameof(ExecuteAsync), "ExecutionType: {0}, CurrentAction: {1}, _NoResultsExpected: {2}, rowsAffected: {3}, totalRowsAffected: {4}, isSpecialAction: {5}.",
				//	_SqlStatement.ExecutionType, _SqlStatement.CurrentAction, _NoResultsExpected,
				//	rowsAffected, totalRowsAffected, isSpecialAction);

				result = await ProcessReaderAsync(conn, dataReader, isSpecialAction, statementIndex,
					statementCount, rowsSelected, totalRowsSelected, true, cancelToken);

				if (!CheckCancelled(cancelToken) && !isSpecialAction
					&& _SqlStatement.CurrentAction == EnSqlStatementAction.ProcessQuery)
				{
					_SqlStatement.UpdateRowsSelected(_RowsAffected);
				}

			}

		}
		catch (Exception ex)
		{
			if (!cancelToken.Cancelled())
				Diag.Expected(ex);

			ex.SetServer(QryMgr.Strategy.DisplayServerName);
			ex.SetDatabase(QryMgr.Strategy.DatasetDisplayName);

			result = HandleExecutionExceptions(ex, _SqlStatement.Index, cancelToken);
		}
		finally
		{
			if (CheckCancelled(cancelToken))
			{
				lock (_LockLocal)
					_ExecutionState = EnBatchState.Initial;
			}
		}

		// Evs.Trace(GetType(), nameof(ExecuteAsync), "Completed. Result {0}", result);

		return result;
	}



	protected void HandleCriticalExceptionMessage(Exception ex, int statementIndex)
	{
		// Evs.Trace(GetType(), "QESQLBatch.HandleExceptionMessage", "", null);

		// HandleSqlMessages(ex.GetErrors(), true);
		string message = statementIndex < 0
			? Resources.ExceptionQueryBatchError.Fmt(ex.Message)
			: Resources.ExceptionQueryBatchStatementError.Fmt(statementIndex+1, ex.Message);

		BatchErrorMessageEventArgs args = new BatchErrorMessageEventArgs(message, "");
		RaiseErrorMessage(args);

		QryMgr.SetState(EnQueryState.Cancelling, true);
		QryMgr.SetState(EnQueryState.Faulted, true);
		QryMgr.SetState(EnQueryState.Prompting, true);
		RdtManager.ShowWindowFrameAsyeu(QryMgr.DocCookie);

		string server = ex.GetServer();
		string database = ex.GetDatabase();

		message = statementIndex < 0
			? Resources.BatchParseErrorDialogMessage.Fmt(server, database, ex.Message)
			: Resources.BatchErrorDialogMessage.Fmt(server, database, statementIndex+1, ex.Message);

		MessageCtl.ShowX(ex, message, Resources.ExceptionQueryExecutionCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		QryMgr.SetState(EnQueryState.Prompting, false);
	}



	public EnScriptExecutionResult HandleExecutionExceptions(Exception exception, int statementIndex, CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "Exception: {0}.", exception.Message);

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;

		

		if (exception.IsSqlException() || (statementIndex == -1 && exception is ArgumentException))
		{
			// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "DbException: {0}.", exception.GetType().Name);

			lock (_LockLocal)
				result = !CheckCancelled(cancelToken) ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;

			if (result != EnScriptExecutionResult.Cancel)
				HandleSqlMessages(exception, statementIndex);
		}
		else if (exception.HasExceptionType<IOException>())
		{
			// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "IOException: {0}.", exception.GetType().Name);

			result = EnScriptExecutionResult.Failure;
			HandleCriticalExceptionMessage(exception, statementIndex);
		}
		else if (exception.HasExceptionType<OverflowException>())
		{
			// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "OverflowException: {0}.", exception.GetType().Name);

			result = EnScriptExecutionResult.Failure;
			HandleCriticalExceptionMessage(exception, statementIndex);
		}
		else if (exception.HasExceptionType<ThreadAbortException>())
		{
			// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "ThreadAbortException: {0}.", exception.GetType().Name);

			result = EnScriptExecutionResult.Failure;
		}
		else if (exception.HasExceptionType<SystemException>())
		{
			// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "SystemException: {0}.", exception.GetType().Name);

			lock (_LockLocal)
				result = !CheckCancelled(cancelToken) ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;

			if (result != EnScriptExecutionResult.Cancel)
				HandleCriticalExceptionMessage(exception, statementIndex);
		}
		else
		{
			// Evs.Trace(GetType(), nameof(HandleExecutionExceptions), "Exception.Exception: {0}.", exception.GetType().Name);

			HandleCriticalExceptionMessage(exception, statementIndex);
			result = EnScriptExecutionResult.Failure;
		}

		return result;

	}



	public void HandleSqlMessages(Exception ex, int statementIndex)
	{
		// Evs.Trace(GetType(), "QESQLBatch.HandleSqlMessages", "Error count: {0}.", errors.Count);

		if (statementIndex == -1 && ex is ArgumentException)
		{
			string text = Resources.BatchParseErrorMessage.Fmt(ex.Message);

			_ContainsErrors = true;

			if (ErrorMessageEvent != null)
				RaiseErrorMessage(new(text, "", -1, _TextSpan));
		}
		else
		{
			IList<object> errors = ex.GetErrors();

			foreach (object error in NativeDb.GetErrorEnumerator(errors))
			{
				// Evs.Trace(GetType(), nameof(HandleSqlMessages), "GetErrorNumber: {0}", NativeDb.GetErrorNumber(error));

				string text = NativeDb.GetErrorMessage(error);
				int line = NativeDb.GetErrorLineNumber(error);

				if (string.IsNullOrWhiteSpace(text))
					continue;

				text = text.Trim();

				if (statementIndex > -1)
					text = Resources.BatchErrorMessage.Fmt(statementIndex + 1, text);

				_ContainsErrors = true;

				if (ErrorMessageEvent != null)
					RaiseErrorMessage(new(text, "", line, _TextSpan));
			}
		}

		QryMgr.SetState(EnQueryState.Cancelling, true);
		QryMgr.SetState(EnQueryState.Faulted, true);
	}



	private static bool IsExecutionPlanResultSet(IDataReader dataReader, out EnSpecialActions batchSpecialAction)
	{
		batchSpecialAction = EnSpecialActions.None;

		if (dataReader == null)
		{
			ArgumentNullException ex = new(nameof(dataReader));
			Diag.Ex(ex);
			throw ex;
		}


		if (dataReader.FieldCount == 1)
		{
			if (string.Compare(dataReader.GetName(0), NativeDb.XmlActualPlanColumn, StringComparison.OrdinalIgnoreCase) == 0)
			{
				batchSpecialAction = EnSpecialActions.ActualPlanIncluded;
				return true;
			}
			else if (string.Compare(dataReader.GetName(0), NativeDb.XmlEstimatedPlanColumn, StringComparison.OrdinalIgnoreCase) == 0)
			{
				batchSpecialAction = EnSpecialActions.EstimatedPlanOnly;
				return true;
			}
		}
		return false;
	}



	public EnScriptExecutionResult Parse(IBsNativeDbBatchParser batchParser)
	{
		// Evs.Trace(GetType(), nameof(Parse));

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;


		// ----------------------------------------------------------------------------------------- //
		// ******************** Parse Execution Point (8) - QESQLBatch.Parse() ********************* //
		// ----------------------------------------------------------------------------------------- //

		try
		{
			batchParser.Parse();
		}
		catch (Exception ex)
		{
			ex.SetServer(QryMgr.Strategy.DisplayServerName);
			ex.SetDatabase(QryMgr.Strategy.DatasetDisplayName);

			result = HandleExecutionExceptions(ex, -1, default);
		}

		return result;

	}



	[System.Diagnostics.CodeAnalysis.SuppressMessage("Usage", "VSTHRD103:Call async methods when in an async method", Justification = "<Pending>")]
	public async Task<EnScriptExecutionResult> ProcessReaderAsync(IDbConnection conn, IDataReader dataReader,
		bool isSpecialAction, int statementIndex, int statementCount, long rowsSelected, long totalRowsSelected,
		bool canComplete, CancellationToken cancelToken)
	{
		// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "Entry conn.State = {0}", conn.State);

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;

		DataTableReader tableReader = null;

		try
		{

			// ----------------------------------------------------------------------------------------- //
			// ******************* After Execution Point (14) - ProcessReaderAsync() ******************* //
			// ----------------------------------------------------------------------------------------- //


			// if (!isSpecialAction)
			//	OnSqlStatementCompleted(_SqlStatement, new(rowsSelected, totalRowsSelected, false));

			// Evs.Trace(GetType(), nameof(ProcessReaderAsync), ": got the reader!");
			lock (_LockLocal)
			{
				if (CheckCancelled(cancelToken))
					result = EnScriptExecutionResult.Cancel;
				else
					_ExecutionState = EnBatchState.ProcessingResults;
			}

			if (NewResultSetEventAsync != null && result != EnScriptExecutionResult.Cancel)
			{
				EnScriptExecutionResult processingResult = EnScriptExecutionResult.Success;
				bool hasMoreRows = false;

				do
				{
					if (dataReader == null)
						break;

					tableReader = dataReader as DataTableReader;

					if (dataReader.FieldCount <= 0)
					{
						Evs.Warning(GetType(), "ProcessReaderAsync()", "Result set is empty.");

						if (tableReader != null)
						{
							hasMoreRows = tableReader.NextResult();
						}
						else
						{
							// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "ASYNC NextResultAsync()");

							try
							{
								hasMoreRows = await dataReader.NextResultAsync(cancelToken);
							}
							catch (Exception ex)
							{
								if (ex is OperationCanceledException || cancelToken.Cancelled())
									Diag.Expected(ex);
								else
									throw;
							}
						}

						if (cancelToken.Cancelled())
						{
							result = EnScriptExecutionResult.Cancel;
							break;
						}

						continue;
					}

					// Evs.Trace(GetType(), nameof(ProcessReaderAsync), ": processing result set");

					// ------------------------------------------------------------------------------- //
					// ************* Output Point (1) - QESQLBatch.ProcessReaderAsync() ************** //
					// ------------------------------------------------------------------------------- //
					processingResult = await ProcessResultSetAsync(dataReader, statementIndex, statementCount, cancelToken);

					if (processingResult == EnScriptExecutionResult.Cancel)
					{
						result = EnScriptExecutionResult.Cancel;
						break;
					}

					if (processingResult != EnScriptExecutionResult.Success)
					{
						// Evs.Trace(GetType(), nameof(ProcessReaderAsync), ": something wrong while processing the result set: {0}", processingResult);
						result = processingResult;
						break;
					}

					// Evs.Trace(GetType(), nameof(ProcessReaderAsync), ": successfully processed the result set");
					FinishedResultSetEvent?.Invoke(this, new EventArgs());

					// hasMoreRows = dataReader.NextResult();
					if (tableReader != null)
					{
						hasMoreRows = tableReader.NextResult();
					}
					else
					{
						// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "ASYNC NextResultAsync() 2");

						try
						{
							hasMoreRows = await dataReader.NextResultAsync(cancelToken);
						}
						catch (Exception ex)
						{
							if (ex is OperationCanceledException || cancelToken.Cancelled())
								Diag.Expected(ex);
							else
								throw;
						}

						if (cancelToken.Cancelled())
						{
							result = EnScriptExecutionResult.Cancel;
							break;
						}

					}
				}
				while (hasMoreRows);

				if (_ContainsErrors)
				{
					Evs.Warning(GetType(), "ProcessReaderAsync()",
						"Successfully processed result set, but there were errors shown to the user.");
					result = EnScriptExecutionResult.Failure;
				}

				if (result != EnScriptExecutionResult.Cancel)
				{
					lock (_LockLocal)
					{
						if (canComplete)
							_ExecutionState = EnBatchState.Executed;
					}

					if (!isSpecialAction)
						RaiseStatementCompleted(statementIndex, _RowsAffected, _TotalRowsAffected, false);
				}

			}
			else
			{
				Evs.Warning(GetType(), "ProcessReaderAsync()",
					"No NewResultSet handler was specified or Cancel was received!");
			}
		}
		catch (Exception ex)
		{
			Diag.Expected(ex);

			ex.SetServer(QryMgr.Strategy.DisplayServerName);
			ex.SetDatabase(QryMgr.Strategy.DatasetDisplayName);

			result = HandleExecutionExceptions(ex, statementIndex, cancelToken);
		}
		finally
		{
			// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "Finalizing");

			try
			{
				if (dataReader != null)
				{
					if (tableReader != null)
					{
						tableReader.Close();
					}
					else
					{
						// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "ASYNC CloseAsync()");

						try
						{
							await dataReader.CloseAsync(cancelToken);
						}
						catch (Exception ex)
						{
							if (ex is OperationCanceledException || cancelToken.Cancelled())
								Diag.Expected(ex);
							else
								throw;
						}
					}

					dataReader = null;
				}

				// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "Finalized");
			}
			catch (ThreadAbortException exta)
			{
				Diag.Ex(exta);
			}
			catch (SystemException exsy)
			{
				Diag.Ex(exsy);
			}
			catch (Exception ex)
			{
				Diag.Ex(ex);
			}
		}

		// Evs.Trace(GetType(), nameof(ProcessReaderAsync), "Completed. Result: {0}", result);

		return result;
	}



	protected async Task<EnScriptExecutionResult> ProcessResultSetAsync(IDataReader dataReader,
		int statementIndex, int statementCount, CancellationToken cancelToken)
	{
		if (dataReader == null)
		{
			ArgumentNullException ex = new(nameof(dataReader));
			Diag.Ex(ex);
			throw ex;
		}

		bool isPlan = IsExecutionPlanResultSet(dataReader, out EnSpecialActions batchSpecialActiont);

		// Evs.Trace(GetType(), nameof(ProcessResultSetAsync), "Entry. _SpecialActions: {0}, SpecialActionEvent: {1}, IsExecutionPlanResultSet(): {2}, batchSpecialAction: {3}.",
		// 	_SpecialActions, SpecialActionEvent, isPlan, batchSpecialActiont);


		if ((_SpecialActions & EnSpecialActions.ExecutionPlansMask) != 0
			&& !cancelToken.Cancelled() && SpecialActionEvent != null
			&& IsExecutionPlanResultSet(dataReader, out EnSpecialActions batchSpecialAction)
			&& (_SpecialActions & batchSpecialAction) != 0)
		{
			return ProcessResultSetForExecutionPlan(dataReader, batchSpecialAction, cancelToken);
		}

		if (NewResultSetEventAsync == null)
		{
			ArgumentNullException ex = new(nameof(NewResultSetEventAsync));
			Diag.Ex(ex);
			throw ex;
		}

		lock (_LockLocal)
		{
			_ActiveResultSet = new QueryResultSet(dataReader, statementIndex, statementCount);
		}

		try
		{
			// Evs.Trace(GetType(), Tracer.EnLevel.Verbose, "ProcessResultSet", "result set has been created!");
			EnScriptExecutionResult result = EnScriptExecutionResult.Success;
			BatchNewResultSetEventArgs args = new(_ActiveResultSet, cancelToken);

			// Evs.Trace(GetType(), nameof(ProcessResultSetAsync), "firing new resultset event!");

			// The data reader loads into a mem or disk storage dataset here then notifies
			// the consumer result set for loading into a grid or text page.
			// Call sequence: QWSQLBatch.NewResultSetEvent -> IQESQLBatchConsumer.OnNewResultSet
			// The consumer (AbstractBatchConsumer ResultsHandler.BatchConsumer can be either of:
			//	GridBatchConsumer or TextOrFileBatchConsumer
			await NewResultSetEventAsync(this, args);

			_RowsAffected += _ActiveResultSet.TotalNumberOfRows;
			_TotalRowsAffected += _ActiveResultSet.TotalNumberOfRows;


			// Moved because everything is now in a batch and this must only be executed at batch completion.
			// if (_State != EnBatchState.Initial && _State != EnBatchState.Cancelling)
			//	DataLoadedEvent?.Invoke(this, new(_SqlStatement, _RowsAffected, dataReader.RecordsAffected, DateTime.Now, false));

			if (!CheckCancelled(cancelToken))
				return result;

			return EnScriptExecutionResult.Cancel;
		}
		catch (Exception ex)
		{
			Diag.Debug(ex);
			throw ex;
		}
		finally
		{
			lock (_LockLocal)
			{
				_ActiveResultSet = null;
			}
		}
	}



	protected EnScriptExecutionResult ProcessResultSetForExecutionPlan(IDataReader dataReader,
	EnSpecialActions batchSpecialAction, CancellationToken cancelToken)
	{
		if (dataReader == null && SqlStatement == null)
		{
			ArgumentNullException ex = new("dataReader/QESQLBatch::Command");
			Diag.Ex(ex);
			throw ex;
		}

		// Evs.Trace(GetType(), "QESQLBatch.ProcessResultSetForExecutionPlan", "", null);
		// Evs.Trace(GetType(), Tracer.EnLevel.Information, "ProcessResultSetForExecutionPlan", "firing SpecialAction event for showplan", null);
		BatchSpecialActionEventArgs args = new BatchSpecialActionEventArgs(batchSpecialAction, this, dataReader);

		try
		{
			if (SpecialActionEvent == null)
			{
				ArgumentNullException ex = new(nameof(SpecialActionEvent));
				throw ex;
			}

			SpecialActionEvent(this, args);

			lock (_LockLocal)
			{
				if (CheckCancelled(cancelToken))
				{
					return EnScriptExecutionResult.Cancel;
				}

				return EnScriptExecutionResult.Success;
			}
		}
		catch (ThreadAbortException e)
		{
			Diag.ThrowException(e);
			throw;
		}
		catch (Exception e2)
		{
			Diag.ThrowException(e2);
			throw;
		}
	}



	public void Reset()
	{
		// Evs.Trace(GetType(), "QESQLBatch.Reset", "", null);
		lock (_LockLocal)
		{
			_ExecutionState = EnBatchState.Initial;
			_SqlStatement = null;
			_TextSpan = null;
			_RowsAffected = 0L;
			_ContainsErrors = false;
		}
	}



	public void ResetTotal()
	{
		_TotalRowsAffected = 0L;
	}



	public void SetSuppressProviderMessageHeaders(bool shouldSuppress)
	{
		// Evs.Trace(GetType(), "QESQLBatch.SetSuppressProviderMessageHeaders", "shouldSuppress = {0}", shouldSuppress);
		// _SuppressProviderMessageHeaders = shouldSuppress;
	}


	#endregion Methods





	// =========================================================================================================
	#region Event Handling - QESQLBatch
	// =========================================================================================================


	private void RaiseErrorMessage(BatchErrorMessageEventArgs args)
	{
		ErrorMessageEvent?.Invoke(this, args);
	}



	private void RaiseMessage(BatchMessageEventArgs args)
	{
		MessageEvent?.Invoke(this, args);
	}



	private void RaiseStatementCompleted(int statementIndex, long rowsSelected, long totalRowsSelected, bool isParseOnly)
	{
		BatchStatementCompletedEventArgs args = new(statementIndex, rowsSelected, totalRowsSelected, false);

		// Evs.Trace(GetType(), "QESQLBatch.OnSqlStatementCompleted", "", null);
		StatementCompletedEvent?.Invoke(_SqlStatement, args);

		if (MessageEvent != null)
		{
			RaiseMessage(new(Resources.BatchRowsSelectedMessage.Fmt(
				statementIndex + 1, rowsSelected.ToString(CultureInfo.InvariantCulture),
				totalRowsSelected.ToString(CultureInfo.InvariantCulture))));
		}
	}


	#endregion Event Handling





	// =========================================================================================================
	#region Sub-Classes - QESQLBatch
	// =========================================================================================================


	public enum EnBatchState
	{
		Initial,
		Executing,
		Executed,
		ProcessingResults,
		Cancelling
	}


	#endregion Sub-Classes

}