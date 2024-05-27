#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Common.Model.QueryExecution;
using BlackbirdSql.Core.Model.Enums;
using BlackbirdSql.Sys;




namespace BlackbirdSql.Common.Model.Events;


public class QESQLBatchExecutedEventArgs(EnScriptExecutionResult res, QESQLBatch batch, EnSqlExecutionType executionType)
	: ScriptExecutionCompletedEventArgs(res, executionType)
{
	public QESQLBatch Batch { get; private set; } = batch;
}
