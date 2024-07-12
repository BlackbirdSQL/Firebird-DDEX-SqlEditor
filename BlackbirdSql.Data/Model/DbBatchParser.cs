
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Interfaces;
using BlackbirdSql.Sys.Model;
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

		if (_LocalTransaction != null)
		{
			try
			{
				if (_LocalTransaction.HasTransactions())
					_LocalTransaction.Rollback();
			}
			catch { }

			try
			{
				_LocalTransaction.Dispose();
			}
			catch { }

			_LocalTransaction = null;
		}

		if (_LocalConnection != null)
		{
			try
			{
				if (_LocalConnection.State == ConnectionState.Open)
					_LocalConnection.Close();
			}
			catch { }

			try
			{
				_LocalConnection.Dispose();
			}
			catch { }

			_LocalConnection = null;
		}
	}

	public virtual void Dispose()
	{
		Dispose(true);
	}




	private readonly IBQueryManager _QryMgr = null;

	// private bool _Cancelled = false;
	private int _StatementCount;
	private int _Current = -1;
	private int _CurrentActionIndex = 0;
	private long _TotalRowsSelected = 0;
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



	
	public int Current => _Current;

	private static readonly EnSqlStatementAction[][] _ActionArray = [
		[EnSqlStatementAction.ProcessQuery, EnSqlStatementAction.SpecialActions, EnSqlStatementAction.Completed],
		[EnSqlStatementAction.ProcessQuery, EnSqlStatementAction.SpecialWithActualPlan, EnSqlStatementAction.Completed],
		[EnSqlStatementAction.ProcessQuery, EnSqlStatementAction.SpecialWithEstimatedPlan, EnSqlStatementAction.Completed]];



	// public CancellationTokenSource AsyncTokenSource => _AsyncTokenSource ??= new();
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

	public int StatementCount => _StatementCount;

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
			try
			{
				if (_LocalTransaction.HasTransactions())
					_LocalTransaction.Commit();
			}
			catch { }

			try
			{
				_LocalTransaction.Dispose();
			}
			catch { }
		}

		_LocalTransaction = _LocalConnection.BeginTransaction(_QryMgr.TtsIsolationLevel);

	}

	/*
	public void Cancel()
	{
		AsyncTokenSource.Cancel();
		_Cancelled = true;
	}
	*/

	public bool CloseConnection()
	{
		if (_LocalConnection == null)
			return false;

		_LocalConnection.Close();

		return true;
	}

	public async Task<bool> CommitTransactionAsync(CancellationToken cancelToken)
	{
		if (_LocalConnection == null)
		{
			FbTransaction transaction = (FbTransaction)_QryMgr.Transaction;

			try
			{
				if (transaction != null && transaction.HasTransactions())
				{
					await transaction.CommitAsync(cancelToken);
					_QryMgr.DisposeTransaction(true);

					return true;
				}
			}
			catch
			{
				_QryMgr.DisposeTransaction(true);
			}


			return false;
		}

		if (_LocalTransaction == null)
			return false;


		bool result = false;

		try
		{
			if (!_LocalTransaction.HasTransactions())
				return false;

			await _LocalTransaction.CommitAsync(cancelToken);

			result = true;
		}
		catch { }

		try
		{
			_LocalTransaction.Dispose();
		}
		catch { }


		_LocalTransaction = null;

		return result;
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


	public async Task<bool> RollbackTransactionAsync(CancellationToken cancelToken)
	{
		if (_LocalConnection == null)
		{
			FbTransaction transaction = (FbTransaction)_QryMgr.Transaction;

			try
			{
				if (transaction != null && transaction.HasTransactions())
				{
					await transaction.RollbackAsync(cancelToken);
					_QryMgr.DisposeTransaction(true);

					return true;
				}
			}
			catch
			{
				_QryMgr.DisposeTransaction(true);
			}


			return false;
		}

		if (_LocalTransaction == null)
			return false;


		bool result = false;

		try
		{
			if (!_LocalTransaction.HasTransactions())
				return false;

			await _LocalTransaction.RollbackAsync(cancelToken);

			result = true;
		}
		catch { }

		try
		{
			_LocalTransaction.Dispose();
		}
		catch { }


		_LocalTransaction = null;

		return result;
	}


	int IBsNativeDbBatchParser.Parse()
	{
		_ScriptParser = new(_Script);

		_StatementCount = _ScriptParser.Parse();

		return _StatementCount;
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
			statement = NativeDbStatementWrapperProxy.CreateInstance(this, null, 0);

			return EnParserAction.Continue;
		}



		if (_Current == _StatementCount - 1)
		{
			statement = null;
			return EnParserAction.Completed;
		}

		_Current++;
		statement = NativeDbStatementWrapperProxy.CreateInstance(this, _ScriptParser.Results[_Current], _Current);

		Statements.Add(statement);

		return EnParserAction.Continue;
	}


}

