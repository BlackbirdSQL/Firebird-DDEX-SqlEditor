
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_NativeDbBatchParserServiceGuid)]


public interface IBsNativeDbBatchParser : IDisposable
{
	IDbConnection Connection { get; }

	int Current {  get; }

	bool IsLocalConnection { get; }

	public DbDataReader PlanReader { get; }

	DataTable PlanTable { get; }


	EnSqlStatementAction CurrentAction { get; }

	long ExecutionTimeout { get; }

	EnSqlExecutionType ExecutionType { get; }

	int StatementCount { get; }

	long TotalRowsSelected { get; }

	IDbTransaction Transaction { get; }


	void AddRowsSelected(long rowsSelected);

	EnSqlStatementAction AdvanceToNextAction();

	void BeginTransaction();

	// void Cancel();

	bool CloseConnection();

	Task<bool> CommitTransactionsAsync(CancellationToken cancelToken);

	IDbConnection RenewConnection(string connectionString);


	Task<bool> RollbackTransactionsAsync(CancellationToken cancelToken);

	int Parse();


	EnParserAction GetNextStatement(ref IBsNativeDbStatementWrapper statement);
}
