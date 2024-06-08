#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Sys.Enums;
using BlackbirdSql.Core.Enums;
using BlackbirdSql.Shared.Ctl.QueryExecution;

namespace BlackbirdSql.Shared.Events;


public class QESQLBatchExecutedEventArgs(EnScriptExecutionResult res, QESQLBatch batch, EnSqlExecutionType executionType)
	: ScriptExecutionCompletedEventArgs(res, executionType)
{
	public QESQLBatch Batch { get; private set; } = batch;
}
