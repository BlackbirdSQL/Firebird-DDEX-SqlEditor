#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using BlackbirdSql.Common.Model.Enums;
using BlackbirdSql.Common.Model.QueryExecution;




namespace BlackbirdSql.Common.Model.Events;


public class QESQLBatchExecutedEventArgs : ScriptExecutionCompletedEventArgs
{
	public QESQLBatch Batch { get; private set; }

	public QESQLBatchExecutedEventArgs(EnScriptExecutionResult res, QESQLBatch batch,
			bool withEstimatedPlan, bool isParseOnly, bool isTextResults)
		: base(res, withEstimatedPlan, isParseOnly, isTextResults)
	{
		Batch = batch;
	}
}
