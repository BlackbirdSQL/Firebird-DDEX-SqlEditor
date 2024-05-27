using System.Data;
using System.Data.Common;
using System.Threading;



namespace BlackbirdSql.Sys;


public class NativeDbBatchParserProxy : IBsNativeDbBatchParser
{
	private NativeDbBatchParserProxy()
	{
	}

	private NativeDbBatchParserProxy(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script)
	{
		_NativeObject = DbNative.CreateDbBatchParser(executionType, qryMgr, script);
	}


	public static IBsNativeDbBatchParser CreateInstance(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script)
	{
		return new NativeDbBatchParserProxy(executionType, qryMgr, script);
	}


	protected virtual void Dispose(bool isDisposing)
	{
		if (!isDisposing)
			return;

		_NativeObject.Dispose();
	}

	public virtual void Dispose()
	{
		Dispose(true);
	}


	private readonly IBsNativeDbBatchParser _NativeObject = null;


	public CancellationTokenSource AsyncTokenSource => _NativeObject.AsyncTokenSource;
	public IDbConnection Connection => _NativeObject.Connection;


	public bool Cancelled => _NativeObject.Cancelled;



	public EnSqlStatementAction CurrentAction => _NativeObject.CurrentAction;

	public long ExecutionTimeout => _NativeObject.ExecutionTimeout;

	public EnSqlExecutionType ExecutionType => _NativeObject.ExecutionType;

	public bool IsAsync => _NativeObject.IsAsync;
	public bool IsLocalConnection => _NativeObject.IsLocalConnection;


	public DbDataReader PlanReader => _NativeObject.PlanReader;

	public DataTable PlanTable => _NativeObject.PlanTable;


	public long TotalRowsSelected => _NativeObject.TotalRowsSelected;


	public IDbTransaction Transaction => _NativeObject.Transaction;


	public void BeginTransaction()
	{
		_NativeObject.BeginTransaction();
	}

	public void Cancel()
	{
		_NativeObject.Cancel();
	}

	public bool CloseConnection()
	{
		return _NativeObject.CloseConnection();
	}

	public void CommitTransaction()
	{
		_NativeObject.CommitTransaction();
	}


	public IDbConnection RenewConnection(string connectionString)
	{
		return _NativeObject.RenewConnection(connectionString);
	}


	public void RollbackTransaction()
	{
		_NativeObject.RollbackTransaction();
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

