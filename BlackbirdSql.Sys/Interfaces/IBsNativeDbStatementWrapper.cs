// Microsoft.SqlServer.BatchParser, Version=16.100.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// ManagedBatchParser.IBatchSource

using System;
using System.Data;
using System.Data.Common;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Sys.Events;



namespace BlackbirdSql.Sys.Interfaces;

[Guid(LibraryData.C_NativeDbStatementServiceGuid)]


public interface IBsNativeDbStatementWrapper : IDisposable
{
	DbCommand Command { get; }

	EnSqlStatementAction CurrentAction { get; }

	DbDataReader CurrentActionReader { get; }

	long ExecutionTimeout { get; }

	EnSqlExecutionType ExecutionType { get; }

	int Index { get; }
	long RowsSelected { get; }

	long TotalRowsSelected { get; }

	bool IsSpecialAction { get; }

	string Script { get; }

	EnSqlStatementType StatementType { get; }

	IDbTransaction Transaction { get; }


	event EventHandler<StatementExecutionEventArgs> AfterExecutionEvent;


	// void Cancel();

	void DisposeCommand();

	Task<int> ExecuteAsync(bool autoCommit, CancellationToken cancelToken);

	Task<bool> GeneratePlanAsync(CancellationToken cancelToken);

	void UpdateRowsSelected(long rowsSelected);
}
