
using System;
using System.Data.Common;
using BlackbirdSql.Sys.Enums;

namespace BlackbirdSql.Sys.Events;


public class StatementExecutionEventArgs : EventArgs
{
	public StatementExecutionEventArgs(DbDataReader dataReader, string commandText, EnSqlStatementType statementType, int rowsAffected)
	{
		DataReader = dataReader;
		CommandText = commandText;
		StatementType = statementType;
		RowsAffected = rowsAffected;
	}



	public DbDataReader DataReader { get; private set; }

	public string CommandText { get; private set; }

	public EnSqlStatementType StatementType { get; private set; }

	public int RowsAffected { get; private set; }

}
