// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec
using System;
using System.Data;
using System.Data.Common;



namespace BlackbirdSql.Sys;


public class NativeDbStatementWrapperProxy : IBsNativeDbStatementWrapper
{
	private NativeDbStatementWrapperProxy()
	{
	}

	private NativeDbStatementWrapperProxy(IBsNativeDbBatchParser owner, object statement)
	{
		_NativeObject = DbNative.CreateDbStatementWrapper(owner, statement);
	}

	public static IBsNativeDbStatementWrapper CreateInstance(IBsNativeDbBatchParser owner, object statement)
	{
		return new NativeDbStatementWrapperProxy(owner, statement);
	}

	public void DisposeCommand()
	{
		_NativeObject.DisposeCommand();
	}

	protected virtual void Dispose(bool isDisposing)
	{
		if (isDisposing)
			DisposeCommand();
	}

	public virtual void Dispose()
	{
		Dispose(true);
	}


	private readonly IBsNativeDbStatementWrapper _NativeObject;



	public DbCommand Command => _NativeObject.Command;

	public EnSqlStatementAction CurrentAction => _NativeObject.CurrentAction;
	public long ExecutionTimeout => _NativeObject.ExecutionTimeout;
	public EnSqlExecutionType ExecutionType => _NativeObject.ExecutionType;

	public bool IsSpecialAction => _NativeObject.IsSpecialAction;


	public long RowsSelected => _NativeObject.RowsSelected;

	public long TotalRowsSelected => _NativeObject.TotalRowsSelected;

	public DbDataReader CurrentActionReader => _NativeObject.CurrentActionReader;

	public IDbTransaction Transaction => _NativeObject.Transaction;

	public string Script => _NativeObject.Script;


	public EnSqlStatementType StatementType => _NativeObject.StatementType;

	/// <summary>
	/// The event trigged after a SQL statement execution.
	/// </summary>
	public event EventHandler<StatementExecutionEventArgs> AfterExecutionEvent
	{
		add { _NativeObject.AfterExecutionEvent += value; }
		remove { _NativeObject.AfterExecutionEvent -= value; }
	}



	public void Cancel()
	{
		_NativeObject.Cancel();
	}


	public int AsyncExecute(bool autoCommit)
	{
		return _NativeObject.AsyncExecute(autoCommit);
	}


	public bool AsyncNextResult()
	{
		return _NativeObject.AsyncNextResult();
	}

	public bool AsyncRead()
	{
		return _NativeObject.AsyncRead();
	}


	public void GeneratePlan()
	{
		_NativeObject.GeneratePlan();
	}



	public void UpdateRowsSelected(long rowsSelected)
	{
		_NativeObject.UpdateRowsSelected(rowsSelected);
	}



	public override string ToString()
	{
		return _NativeObject.ToString();
	}
}

