// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RunTimePartitionSummaryTypePartitionsAccessedPartitionRange
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Shared.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
public class RunTimePartitionSummaryTypePartitionsAccessedPartitionRange
{
	private ulong startField;

	private ulong endField;

	[XmlAttribute]
	public ulong Start
	{
		get
		{
			return startField;
		}
		set
		{
			startField = value;
		}
	}

	[XmlAttribute]
	public ulong End
	{
		get
		{
			return endField;
		}
		set
		{
			endField = value;
		}
	}
}
