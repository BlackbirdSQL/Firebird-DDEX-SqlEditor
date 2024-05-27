
using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;



namespace BlackbirdSql.Sys;

[Guid(LibraryData.NativeDbBatchParserServiceGuid)]


public interface IBsNativeDbBatchParser : IDisposable
{
	CancellationTokenSource AsyncTokenSource { get; }
	bool Cancelled { get; }

	IDbConnection Connection { get; }

	bool IsAsync { get; }
	bool IsLocalConnection { get; }

	public DbDataReader PlanReader { get; }

	DataTable PlanTable { get; }


	EnSqlStatementAction CurrentAction { get; }

	long ExecutionTimeout { get; }

	EnSqlExecutionType ExecutionType { get; }


	long TotalRowsSelected { get; }

	IDbTransaction Transaction { get; }


	void AddRowsSelected(long rowsSelected);

	EnSqlStatementAction AdvanceToNextAction();

	void BeginTransaction();

	void Cancel();

	bool CloseConnection();

	void CommitTransaction();

	IDbConnection RenewConnection(string connectionString);


	void RollbackTransaction();

	int Parse();


	EnParserAction GetNextStatement(ref IBsNativeDbStatementWrapper statement);
}
