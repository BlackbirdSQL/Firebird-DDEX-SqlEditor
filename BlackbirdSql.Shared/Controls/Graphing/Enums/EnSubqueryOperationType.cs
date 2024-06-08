// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SubqueryOperationType
using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Enums;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public enum EnSubqueryOperationType
{
	[XmlEnum("EQ ALL")]
	EQALL,
	[XmlEnum("EQ ANY")]
	EQANY,
	EXISTS,
	[XmlEnum("GE ALL")]
	GEALL,
	[XmlEnum("GE ANY")]
	GEANY,
	[XmlEnum("GT ALL")]
	GTALL,
	[XmlEnum("GT ANY")]
	GTANY,
	IN,
	[XmlEnum("LE ALL")]
	LEALL,
	[XmlEnum("LE ANY")]
	LEANY,
	[XmlEnum("LT ALL")]
	LTALL,
	[XmlEnum("LT ANY")]
	LTANY,
	[XmlEnum("NE ALL")]
	NEALL,
	[XmlEnum("NE ANY")]
	NEANY
}
