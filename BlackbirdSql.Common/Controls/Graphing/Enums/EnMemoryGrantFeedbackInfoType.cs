// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MemoryGrantFeedbackInfoType
using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;


namespace BlackbirdSql.Common.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public enum EnMemoryGrantFeedbackInfoType
{
	[XmlEnum("Yes: Adjusting")]
	YesAdjusting,
	[XmlEnum("Yes: Stable")]
	YesStable,
	[XmlEnum("No: First Execution")]
	NoFirstExecution,
	[XmlEnum("No: Accurate Grant")]
	NoAccurateGrant,
	[XmlEnum("No: Feedback Disabled")]
	NoFeedbackDisabled,
	[XmlEnum("Yes: Percentile Adjusting")]
	YesPercentileAdjusting
}
