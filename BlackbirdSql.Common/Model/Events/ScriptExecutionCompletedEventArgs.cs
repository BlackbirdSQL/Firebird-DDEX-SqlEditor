
using System;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Sys;



namespace BlackbirdSql.Common.Model.Events;


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
