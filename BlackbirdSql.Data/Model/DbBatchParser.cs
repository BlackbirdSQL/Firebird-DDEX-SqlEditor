
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Threading;
using BlackbirdSql.Sys;
using FirebirdSql.Data.FirebirdClient;
using FirebirdSql.Data.Isql;



namespace BlackbirdSql.Data.Model;


public class DbBatchParser : IBsNativeDbBatchParser
{
	private DbBatchParser()
	{
	}

	public DbBatchParser(EnSqlExecutionType executionType, IBQueryManager qryMgr, string script)
	{
		_ExecutionType = executionType;
		_QryMgr = qryMgr;
		_Script = script;
		_IsAsync = qryMgr.IsAsync;
	}


	protected virtual void Dispose(bool isDisposing)
	{
		if (!isDisposing)
			return;

		if (_Statements != null)
		{
			foreach (IBsNativeDbStatementWrapper statement in _Statements)
			{
				statement.Dispose();
			}
			_Statements = null;
		}

		_ScriptParser = null;
	}

	public virtual void Dispose()
	{
		Dispose(true);
	}




	private readonly IBQueryManager _QryMgr = null;

	private bool _Cancelled = false;
	private readonly bool _IsAsync = false;
	private int _Count;
	private int _Current = -1;
	private int _CurrentActionIndex = 0;
	private long _TotalRowsSelected = 0;
	private CancellationTokenSource _AsyncTokenSource = null;
	private readonly string _Script;
	private FbScript _ScriptParser;
	private readonly EnSqlExecutionType _ExecutionType;
	private FbConnection _LocalConnection = null;
	private FbTransaction _LocalTransaction = null;


	private DbDataReader _ActualPlanReader = null;
	private DbDataReader _EstimatedPlanReader = null;

	private DataTable _ActualPlanTable = null;
	private DataTable _EstimatedPlanTable = null;

	private List<IBsNativeDbStatementWrapper> _Statements = null;




	private static readonly EnSqlStatementAction[][] _ActionArray = [
		[EnSqlStatementAction.ProcessQuery, EnSqlStatementAction.SpecialActions, EnSqlStatementAction.Completed],
		[EnSqlStatementAction.ProcessQuery, EnSqlStatementAction.SpecialWithActualPlan, EnSqlStatementAction.Completed],
		[EnSqlStatementAction.ProcessQuery, EnSqlStatementAction.SpecialWithEstimatedPlan, EnSqlStatementAction.Completed]];



	public CancellationTokenSource AsyncTokenSource => _AsyncTokenSource ??= new();
	private DbDataReader ActualPlanReader => _ActualPlanReader ??= new DataTableReader(ActualPlanTable);
	private DbDataReader EstimatedPlanReader => _EstimatedPlanReader ??= new DataTableReader(EstimatedPlanTable);

	private DataTable ActualPlanTable
	{
		get
		{
			if (_ActualPlanTable != null)
				return _ActualPlanTable;

			_ActualPlanTable = new();
			_ActualPlanTable.Columns.Add(LibraryData.XmlActualPlanColumn, typeof(string));

			return _ActualPlanTable;
		}
	}

	public bool IsAsync => _IsAsync;
	public bool Cancelled => _Cancelled;


	IDbConnection IBsNativeDbBatchParser.Connection
	{
		get
		{
			if (_LocalConnection != null)
				return _LocalConnection;

			return _QryMgr.Connection;
		}
	}


	public EnSqlStatementAction CurrentAction => _ActionArray[(int)_ExecutionType][_CurrentActionIndex];


	private DataTable EstimatedPlanTable
	{
		get
		{
			if (_EstimatedPlanTable != null)
				return _EstimatedPlanTable;

			_EstimatedPlanTable = new();
			_EstimatedPlanTable.Columns.Add(LibraryData.XmlEstimatedPlanColumn, typeof(string));

			return _EstimatedPlanTable;
		}
	}

	public long ExecutionTimeout => _QryMgr.ExecutionTimeout;


	public EnSqlExecutionType ExecutionType => _ExecutionType;

	public bool IsLocalConnection => _LocalConnection != null;


	public DbDataReader PlanReader => _ExecutionType == EnSqlExecutionType.PlanOnly
		? EstimatedPlanReader
		: _ExecutionType == EnSqlExecutionType.QueryWithPlan ? ActualPlanReader : null;

	public DataTable PlanTable => _ExecutionType == EnSqlExecutionType.PlanOnly
		? EstimatedPlanTable
		: _ExecutionType == EnSqlExecutionType.QueryWithPlan ? ActualPlanTable : null;

	private List<IBsNativeDbStatementWrapper> Statements => _Statements ??= [];

	public long TotalRowsSelected => _TotalRowsSelected;

	public IDbTransaction Transaction
	{
		get
		{
			if (_LocalConnection != null)
				return _LocalTransaction;

			return _QryMgr?.Transaction;
		}
	}


	public void BeginTransaction()
	{
		if (_LocalConnection == null)
		{
			_QryMgr?.BeginTransaction();
			return;
		}

		if (_LocalTransaction != null)
		{
			if (HasTransactions(_LocalTransaction))
				_LocalTransaction.Commit();

			_LocalTransaction.Dispose();
		}

		_LocalTransaction = _LocalConnection.BeginTransaction(_QryMgr.TtsIsolationLevel);

	}

	public void Cancel()
	{
		AsyncTokenSource.Cancel();
		_Cancelled = true;
	}

	public bool CloseConnection()
	{
		if (_LocalConnection == null)
			return false;

		_LocalConnection.Close();

		return true;
	}

	public void CommitTransaction()
	{
		if (_LocalConnection == null)
		{
			_QryMgr?.CommitTransaction();
			return;
		}

		if (_LocalTransaction != null && HasTransactions(_LocalTransaction))
			_LocalTransaction.Commit();
	}

	public bool HasTransactions(IDbTransaction @this)
	{
		if (@this == null)
			return false;

		FbConnection connection = (FbConnection)@this.Connection;

		FbDatabaseInfo dbInfo = new(connection);

		return dbInfo.GetActiveTransactionsCount() > 0;
	}

	public IDbConnection RenewConnection(string connectionString)
	{
		if (_LocalConnection != null)
		{
			if (_LocalConnection.State != ConnectionState.Closed)
				CloseConnection();

			_LocalConnection.ConnectionString = connectionString;
		}
		else
		{
			_LocalConnection = new FbConnection(connectionString);
		}

		return _LocalConnection;
	}


	public void RollbackTransaction()
	{
		_QryMgr?.RollbackTransaction();
	}


	int IBsNativeDbBatchParser.Parse()
	{
		_ScriptParser = new(_Script);

		_Count = _ScriptParser.Parse();

		return _Count;
	}


	public void AddRowsSelected(long rowsSelected)
	{
		if (rowsSelected > 0)
			_TotalRowsSelected += rowsSelected;
	}


	public EnSqlStatementAction AdvanceToNextAction()
	{
		EnSqlStatementAction currentAction = CurrentAction;

		if (currentAction == EnSqlStatementAction.Completed)
			return currentAction;

		_Current = -1;
		_CurrentActionIndex++;

		return CurrentAction;
	}

	void IBsNativeDbBatchParser.BeginTransaction()
	{
		_QryMgr.BeginTransaction();
	}


	EnParserAction IBsNativeDbBatchParser.GetNextStatement(ref IBsNativeDbStatementWrapper statement)
	{
		if (CurrentAction == EnSqlStatementAction.SpecialActions
			|| CurrentAction == EnSqlStatementAction.SpecialWithActualPlan
			|| CurrentAction == EnSqlStatementAction.SpecialWithEstimatedPlan)
		{
			if (_Current >= 0)
			{
				statement = null;
				return EnParserAction.Completed;
			}

			_Current++;
			statement = NativeDbStatementWrapperProxy.CreateInstance(this, null);

			return EnParserAction.Continue;
		}



		if (_Current == _Count - 1)
		{
			statement = null;
			return EnParserAction.Completed;
		}

		_Current++;
		statement = NativeDbStatementWrapperProxy.CreateInstance(this, _ScriptParser.Results[_Current]);

		Statements.Add(statement);

		return EnParserAction.Continue;
	}


}

