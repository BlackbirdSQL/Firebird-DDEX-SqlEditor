#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;
using BlackbirdSql.Common.Model.Enums;

// using Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;
// using Ns = Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution;


// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Events
{
	public class ScriptExecutionCompletedEventArgs(EnScriptExecutionResult executionResult,
		bool withEstimatedPlan, bool isParseOnly) : EventArgs
	{
		public EnScriptExecutionResult ExecutionResult { get; private set; } = executionResult;


		public bool WithEstimatedPlan { get; private set; } = withEstimatedPlan;

		public bool IsParseOnly { get; private set; } = isParseOnly;


		public ScriptExecutionCompletedEventArgs(EnScriptExecutionResult executionResult)
			: this(executionResult, false, false)
		{
		}

	}
}
