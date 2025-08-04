// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ItemsChoiceType
using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Enums;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace, IncludeInSchema = false)]
internal enum EnItemsChoiceType
{
	ColumnsWithNoStatistics,
	ColumnsWithStaleStatistics,
	ExchangeSpillDetails,
	HashSpillDetails,
	MemoryGrantWarning,
	PlanAffectingConvert,
	SortSpillDetails,
	SpillOccurred,
	SpillToTempDb,
	Wait
}
