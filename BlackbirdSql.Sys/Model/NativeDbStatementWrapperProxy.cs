// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QEOLESQLExec
using System;
using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Sys.Model;


public class NativeDbStatementWrapperProxy : IBsNativeDbStatementWrapper
{

	private NativeDbStatementWrapperProxy()
	{
	}

	private NativeDbStatementWrapperProxy(IBsNativeDbBatchParser owner, object statement, int index)
	{
		_NativeObject = NativeDb.CreateDbStatementWrapper(owner, statement, index);
	}

	public static IBsNativeDbStatementWrapper CreateInstance(IBsNativeDbBatchParser owner, object statement, int index)
	{
		return new NativeDbStatementWrapperProxy(owner, statement, index);
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


	public int Index => _NativeObject.Index;

	public long RowsSelected => _NativeObject.RowsSelected;

	public int StatementCount => _NativeObject.StatementCount;

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



	public async Task<int> ExecuteAsync(bool autoCommit, CancellationToken cancelToken)
	{
		return await _NativeObject.ExecuteAsync(autoCommit, cancelToken);
	}



	public async Task<bool> GeneratePlanAsync(CancellationToken cancelToken)
	{
		return await _NativeObject.GeneratePlanAsync(cancelToken);
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

