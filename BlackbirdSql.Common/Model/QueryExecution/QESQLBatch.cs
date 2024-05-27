// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatch

using System;
using System.Collections.Generic;
using System.Data;
using System.Globalization;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using BlackbirdSql.Common.Ctl.Interfaces;
using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.Events;
using BlackbirdSql.Common.Model.Interfaces;
using BlackbirdSql.Common.Properties;
using BlackbirdSql.Core;
using BlackbirdSql.Core.Controls;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Core.Model.Interfaces;
using BlackbirdSql.Sys;




namespace BlackbirdSql.Common.Model.QueryExecution;


public class QESQLBatch : IBDataReaderHandler, IDisposable
{
	public QESQLBatch(QueryManager qryMgr)
	{
		QryMgr = qryMgr;
		ContainsErrors = false;
	}

	/*
	public QESQLBatch(bool bNoResultsExpected, int execTimeout, QueryManager qryMgr)
		: this(bNoResultsExpected, null, execTimeout, EnSqlSpecialActions.None, qryMgr)
	{
	}


	public QESQLBatch(bool bNoResultsExpected, NativeDbStatementWrapperProxy sqlStatement, int execTimeout, EnSqlSpecialActions specialActions, QueryManager qryMgr)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.QESQLBatch", "bNoResultsExpected = {0}, execTimeout = {1}, specialActions = {2}, sqlScript = \"{3}\"", bNoResultsExpected, execTimeout, specialActions, sqlScript);
		_NoResultsExpected = bNoResultsExpected;
		_SqlStatement = sqlStatement;
		_ExecTimeout = execTimeout;
		_SpecialActions = specialActions;
		QryMgr = qryMgr;
		ContainsErrors = false;
	}
	*/

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

		lock (_LockLocal)
		{
			if (_ActiveResultSet != null)
			{
				_ActiveResultSet.Dispose();
				_ActiveResultSet = null;
			}
		}
	}




	/*
	public class MessagesToEat
	{
		public const int C_ChangeDatabaseErrorNumber = 5701;
	}
	*/

	public enum EnBatchState
	{
		Initial,
		Executing,
		Executed,
		ProcessingResults,
		Cancelling
	}



	private const int C_ChangeDatabaseErrorNumber = 5701;



	// A private 'this' object lock
	private readonly object _LockLocal = new object();

	protected bool _NoResultsExpected;

	protected IBsNativeDbStatementWrapper _SqlStatement = null;

	protected int _ExecTimeout = SysConstants.C_DefaultCommandTimeout;

	protected EnSqlSpecialActions _SpecialActions;


	protected QEResultSet _ActiveResultSet;

	protected EnBatchState _State;

	protected IBTextSpan _TextSpan;

	protected int _BatchIndex;

	protected long _RowsAffected;
	protected long _TotalRowsAffected;

	private bool _SuppressProviderMessageHeaders;


	public IBsNativeDbStatementWrapper SqlStatement
	{
		get { return _SqlStatement; }
		set { _SqlStatement = value; }
	}

	private AbstractConnectionStrategy ConnectionStrategy => QryMgr.ConnectionStrategy;

	private QueryManager QryMgr { get; set; }

	public bool ContainsErrors { private get; set; }

	public bool SuppressProviderMessageHeaders => _SuppressProviderMessageHeaders;


	/*
	public string SqlScript
	{
		get { return _SqlScript; }
		set { _SqlScript = value; }
	}
	*/

	public EnSqlSpecialActions SpecialActions
	{
		get { return _SpecialActions; }
		set { _SpecialActions = value; }
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
		get { return _TextSpan; }
		set { _TextSpan = value; }
	}

	public int BatchIndex
	{
		get { return _BatchIndex; }
		set { _BatchIndex = value; }
	}

	public long RowsAffected => _RowsAffected;
	public long TotalRowsAffected => _TotalRowsAffected;

	public event QESQLBatchErrorMessageEventHandler ErrorMessageEvent;

	public event QESQLBatchMessageEventHandler MessageEvent;

	public event QESQLBatchNewResultSetEventHandler NewResultSetEvent;

	public event QESQLBatchSpecialActionEventHandler SpecialActionEvent;

	public event QESQLStatementCompletedEventHandler StatementCompletedEvent;

	public event EventHandler CancellingEvent;

	public event EventHandler FinishedResultSetEvent;




	public void SetSuppressProviderMessageHeaders(bool shouldSuppress)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.SetSuppressProviderMessageHeaders", "shouldSuppress = {0}", shouldSuppress);
		_SuppressProviderMessageHeaders = shouldSuppress;
	}

	public void Reset()
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Reset", "", null);
		lock (_LockLocal)
		{
			_State = EnBatchState.Initial;
			_SqlStatement = null;
			_TextSpan = null;
			_RowsAffected = 0L;
			ContainsErrors = false;
		}
	}

	public void ResetTotal()
	{
		_TotalRowsAffected = 0L;
	}

	public EnScriptExecutionResult Process(IDbConnection conn, EnSqlSpecialActions specialActions)
	{
		// Tracer.Trace(GetType(), "Prepare()", " ExecutionOptions.EstimatedPlanOnly: " + QryMgr.LiveSettings.EstimatedPlanOnly);

		if (_ActiveResultSet != null)
			_ActiveResultSet = null;

		_SpecialActions = specialActions;

		// ----------------------------------------------------------------------------------- //
		// ******************** Execution Point (10) - Process() ******************** //
		// ----------------------------------------------------------------------------------- //
		return Execute(conn);
	}



	public void Cancel()
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Cancel", "", null);
		lock (_LockLocal)
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
			}

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: executing Cancel command", "", null);
			try
			{
				_SqlStatement?.Cancel();
			}
			catch (Exception e2)
			{
				Diag.Dug(e2);
			}

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "QESQLBatch.Cancel: Cancel command returned", "", null);
		}
	}


	protected static bool IsExecutionPlanResultSet(IDataReader dataReader, out EnSqlSpecialActions batchSpecialAction)
	{
		batchSpecialAction = EnSqlSpecialActions.None;

		if (dataReader == null)
		{
			ArgumentNullException ex = new("dataReader");
			Diag.Dug(ex);
			throw ex;
		}


		if (dataReader.FieldCount == 1)
		{
			if (string.Compare(dataReader.GetName(0), DbNative.XmlActualPlanColumn, StringComparison.OrdinalIgnoreCase) == 0)
			{
				batchSpecialAction = EnSqlSpecialActions.ActualPlanIncluded;
				return true;
			}
			else if (string.Compare(dataReader.GetName(0), DbNative.XmlEstimatedPlanColumn, StringComparison.OrdinalIgnoreCase) == 0)
			{
				batchSpecialAction = EnSqlSpecialActions.EstimatedPlanOnly;
				return true;
			}
		}

		return false;
	}

	protected void HandleCriticalExceptionMessage(Exception ex)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.HandleExceptionMessage", "", null);

		if (ex.IsSqlException())
		{
			HandleSqlMessages(ex.GetErrors(), true);
			MessageCtl.ShowEx(ex, ex.Message, ControlsResources.ErrQueryExecutionCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);

			return;
		}


		QESQLBatchErrorMessageEventArgs args = new QESQLBatchErrorMessageEventArgs(
			string.Format(CultureInfo.CurrentCulture, ControlsResources.BatchError, ex.Message), "");
		RaiseErrorMessage(args);
	}

	protected void HandleDbExceptionMessage(Exception ex)
	{
		if (ex.IsSqlException())
		{
			HandleSqlMessages(ex.GetErrors(), true);
			MessageCtl.ShowEx(ex, ex.Message, ControlsResources.ErrQueryExecutionCaption, MessageBoxButtons.OK, MessageBoxIcon.Hand);
		}
	}

	public void HandleSqlMessages(IList<object> errors, bool isException)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.HandleSqlMessages", "Error count: {0}.", errors.Count);

		foreach (object error in DbNative.GetErrorEnumerator(errors))
		{
			// Tracer.Trace(GetType(), "HandleSqlMessages()", "GetErrorNumber: {0}", DbNative.GetErrorNumber(error));

			if (DbNative.GetErrorNumber(error) != C_ChangeDatabaseErrorNumber)
			{
				string text = string.Empty;
				bool isError = false;

				/*
				int num = 0;

				if (_TextSpan != null && _TextSpan.LineWithinTextSpan >= 0)
					num = _TextSpan.LineWithinTextSpan + _TextSpan.AnchorLine;
				*/

				if (isException)
				{
					// Tracer.Trace(GetType(), "HandleSqlMessages()", "GetErrorMesage: {0}", DbNative.GetErrorMessage(error));

					if (DbNative.GetErrorMessage(error) != null && DbNative.GetErrorMessage(error).Trim() != string.Empty)
					{
						isError = true;
						text = DbNative.GetErrorMessage(error).Trim();
						// text = ControlsResources.SQLErrorFormatDbEngine.FmtRes(error.Message == null ? string.Empty : error.Message.Trim(),
						//	error.Number, error.Class, error.LineNumber + num);
					}

					/*
					text = ((error.Procedure != null && (error.Procedure == null || error.Procedure.Length != 0))
						? ((!_SuppressProviderMessageHeaders)
							? string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat6, error.Source, error.Number, error.Class, error.State, error.Procedure, error.LineNumber + num)
							: string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat6_NoSource, error.Number, error.Class, error.State, error.Procedure, error.LineNumber + num))
						: ((!_SuppressProviderMessageHeaders)
							? string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat5, error.Source, error.Number, error.Class, error.State, error.LineNumber + num)
							: string.Format(CultureInfo.CurrentCulture, LanguageServicesResources.SQLErrorFormat5_NoSource, error.Number, error.Class, error.State, error.LineNumber + num)));
					*/
				}
				else if (DbNative.GetErrorClass(error) > 0 && DbNative.GetErrorNumber(error) > 0)
				{
					text = !_SuppressProviderMessageHeaders ? string.Format(CultureInfo.CurrentCulture,
						ControlsResources.SQLErrorFormat4, DbNative.GetErrorMessage(error), DbNative.GetErrorNumber(error),
						DbNative.GetErrorClass(error), -1) : string.Format(CultureInfo.CurrentCulture,
						ControlsResources.SQLErrorFormat4_NoSource, DbNative.GetErrorNumber(error),
						DbNative.GetErrorClass(error), -1);
					text = text.Trim();
				}

				if (isError)
				{
					ContainsErrors = true;
					if (ErrorMessageEvent != null)
						RaiseErrorMessage(new(text, string.Empty, -1, _TextSpan));
				}
				else if (MessageEvent != null && text != string.Empty)
				{
					RaiseMessage(new(text, DbNative.GetErrorMessage(error)));
				}
			}
		}
	}



	public void OnSqlInfoMessage(object sender, DbInfoMessageEventArgs e)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.OnSqlInfoMessage", "", null);
		IList<object> errors = DbNative.GetInfoMessageEventArgsErrors(e);
		HandleSqlMessages(errors, false);
	}

	public void OnSqlStatementCompleted(object sender, QESQLStatementCompletedEventArgs e)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.OnSqlStatementCompleted", "", null);
		StatementCompletedEvent?.Invoke(sender, e);

		if (MessageEvent != null)
		{
			RaiseMessage(new(string.Format(CultureInfo.CurrentCulture, ControlsResources.RowsSelectedMessage,
				e.RowsSelected.ToString(CultureInfo.InvariantCulture),
				e.TotalRowsSelected.ToString(CultureInfo.InvariantCulture))));
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

	protected EnScriptExecutionResult ProcessResultSetForExecutionPlan(IDataReader dataReader, EnSqlSpecialActions batchSpecialAction)
	{
		if (dataReader == null && SqlStatement == null)
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

			lock (_LockLocal)
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
			Diag.ThrowException(e);
			// Tracer.LogExThrow(GetType(), e, "ThreadAbortException was raised during QESqlBatch::ProcessResultSetForExecutionPlan()");
			throw;
		}
		catch (Exception e2)
		{
			Diag.ThrowException(e2);
			// Tracer.LogExThrow(GetType(), e2, "Exception  was raised raised during QESqlBatch::ProcessResultSetForExecutionPlan()");
			throw;
		}
	}

	protected EnScriptExecutionResult ProcessResultSet(IDataReader dataReader)
	{
		if (dataReader == null)
		{
			ArgumentNullException ex = new("dataReader");
			Diag.Dug(ex);
			throw ex;
		}

		bool isPlan = IsExecutionPlanResultSet(dataReader, out EnSqlSpecialActions batchSpecialActiont);

		// Tracer.Trace(GetType(), "ProcessResultSet()", "_SpecialActions: {0}, SpecialActionEvent: {1}, IsExecutionPlanResultSet(): {2}, batchSpecialAction: {3}.",
		//	_SpecialActions, SpecialActionEvent, isPlan, batchSpecialActiont);


		if ((_SpecialActions & EnSqlSpecialActions.ExecutionPlansMask) != 0
			&& SpecialActionEvent != null
			&& IsExecutionPlanResultSet(dataReader, out EnSqlSpecialActions batchSpecialAction)
			&& (_SpecialActions & batchSpecialAction) != 0)
		{
			return ProcessResultSetForExecutionPlan(dataReader, batchSpecialAction);
		}

		if (NewResultSetEvent == null)
		{
			ArgumentNullException ex = new("NewResultSetEvent");
			Diag.Dug(ex);
			throw ex;
		}

		lock (_LockLocal)
		{
			_ActiveResultSet = new QEResultSet(dataReader, _SqlStatement, QryMgr);
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
			_TotalRowsAffected += _ActiveResultSet.TotalNumberOfRows;


			// Moved because everything is now in a batch and this must only be executed at batch completion.
			// if (_State != EnBatchState.Initial && _State != EnBatchState.Cancelling)
			//	DataLoadedEvent?.Invoke(this, new(_SqlStatement, _RowsAffected, dataReader.RecordsAffected, DateTime.Now, false));

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
			lock (_LockLocal)
			{
				_ActiveResultSet = null;
			}
		}
	}

	protected EnScriptExecutionResult Execute(IDbConnection conn)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Execute", "conn.State = {0}", conn.State);
		lock (_LockLocal)
		{
			if (_State == EnBatchState.Cancelling)
			{
				_State = EnBatchState.Initial;
				return EnScriptExecutionResult.Cancel;
			}
		}

		// Tracer.Trace(GetType(), "QESQLBatch.Execute", " Creating command - _SpecialActions: " + _SpecialActions);

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;

		IBBatchExecutionHandler batchExecutionHandler = ConnectionStrategy.CreateBatchExecutionHandler();
		batchExecutionHandler?.Register(conn, _SqlStatement, this);

		lock (_LockLocal)
			_State = EnBatchState.Executing;


		try
		{
			// ----------------------------------------------------------------------------------------- //
			// ******************** Final Execution Point (11) - Execute() ******************** //
			// ----------------------------------------------------------------------------------------- //

			// Tracer.Trace(GetType(), "Execute()", "Executing _SqlStatement.");

			_SqlStatement.AsyncExecute(false);

			// Tracer.Trace(GetType(), "Execute()", "Executed _SqlStatement. ExecutionType: {0}, CurrentAction: {1}, CurrentActionReader: {2}, HasRows: {3}.",
			//	_SqlStatement.ExecutionType, _SqlStatement.CurrentAction, _SqlStatement.CurrentActionReader,
			//	_SqlStatement.CurrentActionReader == null ? "null" : (_SqlStatement.CurrentActionReader.HasRows ? "HasRows" : "NoRows" ));

			if (_State == EnBatchState.Cancelling)
				return EnScriptExecutionResult.Cancel;


			_NoResultsExpected = _SqlStatement.CurrentActionReader == null || !_SqlStatement.CurrentActionReader.HasRows;


			long rowsSelected = _SqlStatement.RowsSelected;
			long totalRowsSelected = _SqlStatement.TotalRowsSelected;
			bool isSpecialAction = _SqlStatement.IsSpecialAction;


			if (_NoResultsExpected && !isSpecialAction)
			{ 
				// result = batchResult;

				if (result != EnScriptExecutionResult.Failure)
				{
					QESQLStatementCompletedEventArgs args = new(rowsSelected, totalRowsSelected, false);
					OnSqlStatementCompleted(_SqlStatement, args);

					// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "Execute()", " ExecuteNonQuery returned!");
					lock (_LockLocal)
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
			}
			else
			{
				IDataReader dataReader = _SqlStatement.CurrentActionReader;

				// Tracer.Trace(GetType(), "Execute()", "ExecutionType: {0}, CurrentAction: {1}, _NoResultsExpected: {2}, rowsAffected: {3}, totalRowsAffected: {4}, isSpecialAction: {5}.",
				//	_SqlStatement.ExecutionType, _SqlStatement.CurrentAction, _NoResultsExpected,
				//	rowsAffected, totalRowsAffected, isSpecialAction);

				result = ProcessReader(conn, dataReader, isSpecialAction, rowsSelected, totalRowsSelected, false, true);

				if (_State != EnBatchState.Cancelling && !isSpecialAction && _SqlStatement.CurrentAction == EnSqlStatementAction.ProcessQuery)
					_SqlStatement.UpdateRowsSelected(_RowsAffected);

			}

		}
		catch (Exception ex)
		{
			result = HandleExecutionExceptions(ex, true);
		}
		finally
		{
			batchExecutionHandler?.UnRegister(conn, _SqlStatement, this);

			if (_State != EnBatchState.Cancelling)
			{
				lock (_LockLocal)
					_State = EnBatchState.Initial;
			}
		}

		// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "Execute()", "returning {0}", result);

		return result;
	}



	public EnScriptExecutionResult ProcessReader(IDbConnection conn, IDataReader dataReader, 
		bool isSpecialAction, long rowsSelected, long totalRowsSelected, bool isBatch, bool canComplete)
	{
		// Tracer.Trace(GetType(), "QESQLBatch.Execute", "conn.State = {0}", conn.State);

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;


		try
		{

			// ----------------------------------------------------------------------------------------- //
			// ******************** After Execution Point (12) - Execute() ******************** //
			// ----------------------------------------------------------------------------------------- //


			// if (!isSpecialAction)
			//	OnSqlStatementCompleted(_SqlStatement, new(rowsSelected, totalRowsSelected, false));

			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "Execute()", ": got the reader!");
			lock (_LockLocal)
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

			if (NewResultSetEvent != null && result != EnScriptExecutionResult.Cancel)
			{
				EnScriptExecutionResult processingResult = EnScriptExecutionResult.Success;
				bool hasMoreRows = false;

				do
				{
					if (dataReader == null)
						break;

					if (dataReader.FieldCount <= 0)
					{
						Tracer.Warning(GetType(), "Execute()", "Result set is empty.");
						hasMoreRows = dataReader.NextResult();
						continue;
					}

					// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "Execute()", ": processing result set");

					// ------------------------------------------------------------------------------- //
					// ******************** Output Point (1) - Execute() ******************** //
					// ------------------------------------------------------------------------------- //
					processingResult = ProcessResultSet(dataReader);

					if (processingResult != EnScriptExecutionResult.Success)
					{
						// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "Execute()", ": something wrong while processing the result set: {0}", processingResult);
						result = processingResult;
						break;
					}

					// Tracer.Trace(GetType(), Tracer.EnLevel.Verbose, "Execute()", ": successfully processed the result set");
					FinishedResultSetEvent?.Invoke(this, new EventArgs());

					// hasMoreRows = dataReader.NextResult();
					hasMoreRows = _SqlStatement.AsyncNextResult();
				}
				while (hasMoreRows);

				if (ContainsErrors)
				{
					Tracer.Warning(GetType(), "Execute()", "Successfully processed result set, but there were errors shown to the user.");
					result = EnScriptExecutionResult.Failure;
				}

				if (result != EnScriptExecutionResult.Cancel)
				{
					lock (_LockLocal)
					{
						if (canComplete)
							_State = EnBatchState.Executed;
					}

					if (!isSpecialAction)
						OnSqlStatementCompleted(_SqlStatement, new(_RowsAffected, _TotalRowsAffected, false));
				}

			}
			else
			{
				Tracer.Warning(GetType(), "Execute()", "No NewResultSet handler was specified or Cancel was received!");
			}
		}
		catch (Exception ex)
		{
			result = HandleExecutionExceptions(ex, true);
		}
		finally
		{
			// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "Execute()", "Closing the data reader");
			try
			{
				if (!isBatch)
				{
					dataReader?.Close();
					dataReader = null;
				}
			}
			catch (ThreadAbortException exta)
			{
				Diag.Dug(exta);
			}
			catch (SystemException exsy)
			{
				Diag.Dug(exsy);
			}
			catch (Exception ex)
			{
				Diag.Dug(ex);
			}
		}

		// Tracer.Trace(GetType(), Tracer.EnLevel.Information, "Execute()", "returning {0}", result);

		return result;
	}

	public EnScriptExecutionResult HandleExecutionExceptions(Exception exception, bool outputTrace)
	{
		// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "Exception: {0}.", exception.Message);

		EnScriptExecutionResult result = EnScriptExecutionResult.Success;

		if (exception.IsSqlException())
		{
			// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "DbException: {0}.", exception.GetType().Name);

			lock (_LockLocal)
				result = _State != EnBatchState.Cancelling ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;

			if (result != EnScriptExecutionResult.Cancel)
			{
#if DEBUG
				if (outputTrace)
					Diag.StackException(exception);
				else
					Diag.Dug(exception);
#endif

				HandleDbExceptionMessage(exception);
			}
		}
		else if (exception.HasExceptionType<IOException>())
		{
			// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "IOException: {0}.", exception.GetType().Name);

			if (outputTrace)
				Diag.StackException(exception);
			else
				Diag.Dug(exception);

			result = EnScriptExecutionResult.Failure;
			HandleCriticalExceptionMessage(exception);
		}
		else if (exception.HasExceptionType<OverflowException>())
		{
			// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "OverflowException: {0}.", exception.GetType().Name);

			if (outputTrace)
				Diag.StackException(exception);
			else
				Diag.Dug(exception);

			result = EnScriptExecutionResult.Failure;
			HandleCriticalExceptionMessage(exception);
		}
		else if (exception.HasExceptionType<ThreadAbortException>())
		{
			// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "ThreadAbortException: {0}.", exception.GetType().Name);
			if (outputTrace)
				Diag.StackException(exception);
			else
				Diag.Dug(exception);

			result = EnScriptExecutionResult.Failure;
		}
		else if (exception.HasExceptionType<SystemException>())
		{
			// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "SystemException: {0}.", exception.GetType().Name);

			lock (_LockLocal)
				result = _State != EnBatchState.Cancelling ? EnScriptExecutionResult.Failure : EnScriptExecutionResult.Cancel;

			if (result != EnScriptExecutionResult.Cancel)
			{
				if (outputTrace)
					Diag.StackException(exception);
				else
					Diag.Dug(exception);

				HandleCriticalExceptionMessage(exception);
			}
		}
		else
		{
			// Tracer.Trace(GetType(), "HandleExecutionExceptions()", "Exception.Exception: {0}.", exception.GetType().Name);

			if (outputTrace)
				Diag.StackException(exception);
			else
				Diag.Dug(exception);

			HandleCriticalExceptionMessage(exception);
			result = EnScriptExecutionResult.Failure;
		}

		return result;

	}

}