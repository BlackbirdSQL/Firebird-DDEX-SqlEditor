
using System;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Sys.Enums;

namespace BlackbirdSql.Shared.Events;


public class ScriptExecutionCompletedEventArgs : EventArgs
{
	public ScriptExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
		EnSqlExecutionType executionType)
	{
		ExecutionResult = executionResult;
		ExecutionType = executionType;
	}

	public ScriptExecutionCompletedEventArgs(EnScriptExecutionResult executionResult)
	: this(executionResult, EnSqlExecutionType.QueryOnly)
	{
	}




	public EnScriptExecutionResult ExecutionResult { get; private set; }


	public EnSqlExecutionType ExecutionType { get; private set; }


}
