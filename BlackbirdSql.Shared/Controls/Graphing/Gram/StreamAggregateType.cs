// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StreamAggregateType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
internal class StreamAggregateType : RelOpBaseType
{
	private ColumnReferenceType[] groupByField;

	private RollupInfoType rollupInfoField;

	private RelOpType relOpField;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	internal ColumnReferenceType[] GroupBy
	{
		get
		{
			return groupByField;
		}
		set
		{
			groupByField = value;
		}
	}

	internal RollupInfoType RollupInfo
	{
		get
		{
			return rollupInfoField;
		}
		set
		{
			rollupInfoField = value;
		}
	}

	internal RelOpType RelOp
	{
		get
		{
			return relOpField;
		}
		set
		{
			relOpField = value;
		}
	}
}
