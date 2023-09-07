#region Assembly Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// location unknown
// Decompiled with ICSharpCode.Decompiler 7.1.0.6543
#endregion

using System;

// namespace Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution
namespace BlackbirdSql.Common.Model.Enums
{
	[Flags]
	public enum EnQESQLBatchSpecialAction
	{
		None = 0x0,
		// Execute script then create execution plan reader
		ExpectActualExecutionPlan = 0x1,
		// Do not execute script. Create execution plan reader only
		ExpectEstimatedExecutionPlan = 0x2,
		// Execute script then create Yukon reader
		ExpectActualYukonXmlExecutionPlan = 0x4,
		// Do not execute script. Create Yukon reader only
		ExpectEstimatedYukonXmlExecutionPlan = 0x8,
		// Not used
		ExecuteWithDebugging = 0x10,
		// The script contains the execution plan results
		ExpectYukonXmlExecutionPlan = 0xC,
		ExecutionPlanMask = 0xF
	}
}
