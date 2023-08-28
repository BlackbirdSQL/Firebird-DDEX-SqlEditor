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
		ExpectActualExecutionPlan = 0x1,
		ExpectEstimatedExecutionPlan = 0x2,
		ExpectActualYukonXmlExecutionPlan = 0x4,
		ExpectEstimatedYukonXmlExecutionPlan = 0x8,
		ExecuteWithDebugging = 0x10,
		ExpectYukonXmlExecutionPlan = 0xC,
		ExecutionPlanMask = 0xF
	}
}
