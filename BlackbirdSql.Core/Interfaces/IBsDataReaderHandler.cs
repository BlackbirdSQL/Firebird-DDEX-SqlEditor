
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;
using BlackbirdSql.Core.Enums;

namespace BlackbirdSql.Core.Interfaces;


internal interface IBsDataReaderHandler
{

	EnScriptExecutionResult HandleExecutionExceptions(Exception exception, int statementIndex, CancellationToken cancelToken);

	Task<EnScriptExecutionResult> ProcessReaderAsync(IDbConnection conn, IDataReader dataReader, bool isSpecialAction,
		int statementIndex, int statementCount, long rowsSelected, long totalRowsSelected, bool canComplete,
		CancellationToken cancelToken);
}
