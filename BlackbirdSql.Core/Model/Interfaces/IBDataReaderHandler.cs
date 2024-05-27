
using System;
using System.Data;
using BlackbirdSql.Core.Model.Enums;



namespace BlackbirdSql.Core.Model.Interfaces;


public interface IBDataReaderHandler
{

	EnScriptExecutionResult HandleExecutionExceptions(Exception exception, bool outputTrace);

	EnScriptExecutionResult ProcessReader(IDbConnection conn, IDataReader dataReader, bool isSpecialAction,
		long rowsSelected, long totalRowsSelected, bool isBatch, bool canComplete);
}
