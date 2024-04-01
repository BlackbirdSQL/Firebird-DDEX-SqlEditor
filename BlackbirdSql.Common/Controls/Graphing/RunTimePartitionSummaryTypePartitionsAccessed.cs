// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RunTimePartitionSummaryTypePartitionsAccessed
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Common.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
public class RunTimePartitionSummaryTypePartitionsAccessed
{
	private RunTimePartitionSummaryTypePartitionsAccessedPartitionRange[] partitionRangeField;

	private ulong partitionCountField;

	[XmlElement("PartitionRange")]
	public RunTimePartitionSummaryTypePartitionsAccessedPartitionRange[] PartitionRange
	{
		get
		{
			return partitionRangeField;
		}
		set
		{
			partitionRangeField = value;
		}
	}

	[XmlAttribute]
	public ulong PartitionCount
	{
		get
		{
			return partitionCountField;
		}
		set
		{
			partitionCountField = value;
		}
	}
}
