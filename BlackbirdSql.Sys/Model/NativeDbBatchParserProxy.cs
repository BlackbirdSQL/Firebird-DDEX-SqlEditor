using System.Data;
using System.Data.Common;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;



namespace BlackbirdSql.Sys.Model;


public class NativeDbBatchParserProxy : IBsNativeDbBatchParser
{
	private NativeDbBatchParserProxy()
	{
	}

	private NativeDbBatchParserProxy(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script)
	{
		_NativeObject = NativeDb.CreateDbBatchParser(executionType, qryMgr, script);
	}


	public static IBsNativeDbBatchParser CreateInstance(EnSqlExecutionType executionType, IBsQueryManager qryMgr, string script)
	{
		return new NativeDbBatchParserProxy(executionType, qryMgr, script);
	}


	protected virtual void Dispose(bool isDisposing)
	{
		if (!isDisposing)
			return;

		_NativeObject?.Dispose();
		_NativeObject = null;
	}

	public virtual void Dispose()
	{
		Dispose(true);
	}




	private IBsNativeDbBatchParser _NativeObject = null;


	public IDbConnection Connection => _NativeObject.Connection;


	// public bool Cancelled => _NativeObject.Cancelled;

	public int Current => _NativeObject.Current;

	public EnSqlStatementAction CurrentAction => _NativeObject.CurrentAction;

	public long ExecutionTimeout => _NativeObject.ExecutionTimeout;

	public EnSqlExecutionType ExecutionType => _NativeObject.ExecutionType;

	public bool IsLocalConnection => _NativeObject.IsLocalConnection;


	public DbDataReader PlanReader => _NativeObject.PlanReader;

	public DataTable PlanTable => _NativeObject.PlanTable;

	public int StatementCount => _NativeObject.StatementCount;

	public long TotalRowsSelected => _NativeObject.TotalRowsSelected;


	public IDbTransaction Transaction => _NativeObject.Transaction;


	public void BeginTransaction()
	{
		_NativeObject.BeginTransaction();
	}

	/*
	public void Cancel()
	{
		_NativeObject.Cancel();
	}
	*/

	public bool CloseConnection()
	{
		return _NativeObject.CloseConnection();
	}

	public async Task<bool> CommitTransactionsAsync(CancellationToken cancelToken)
	{
		return await _NativeObject.CommitTransactionsAsync(cancelToken);
	}


	public IDbConnection RenewConnection(string connectionString)
	{
		return _NativeObject.RenewConnection(connectionString);
	}


	public async Task<bool> RollbackTransactionsAsync(CancellationToken cancelToken)
	{
		return await _NativeObject.RollbackTransactionsAsync(cancelToken);
	}


	int IBsNativeDbBatchParser.Parse()
	{
		return _NativeObject.Parse();
	}


	public void AddRowsSelected(long rowsSelected)
	{
		_NativeObject.AddRowsSelected(rowsSelected);
	}


	public EnSqlStatementAction AdvanceToNextAction()
	{
		return _NativeObject.AdvanceToNextAction();
	}

	void IBsNativeDbBatchParser.BeginTransaction()
	{
		_NativeObject.BeginTransaction();
	}


	EnParserAction IBsNativeDbBatchParser.GetNextStatement(ref IBsNativeDbStatementWrapper statement)
	{
		return _NativeObject.GetNextStatement(ref statement);
	}


}

