// Microsoft.VisualStudio.Data.Tools.SqlEditor, Version=17.0.0.0, Culture=neutral, PublicKeyToken=b03f5f7f11d50a3a
// Microsoft.VisualStudio.Data.Tools.SqlEditor.QueryExecution.QESQLBatchSpecialAction

using System;


namespace BlackbirdSql.Shared.Enums;

[Flags]
public enum EnSpecialActions
{
	None = 0x0,
	// Execute script then create Yukon reader
	ActualPlanIncluded = 0x4,
	// Do not execute script. Create Yukon reader only
	EstimatedPlanOnly = 0x8,
	// 0x1 | 0x2 | 0x4 | 0x8
	ExecutionPlansMask = 0xF
}
