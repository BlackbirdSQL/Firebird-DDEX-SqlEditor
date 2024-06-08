// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.DMLOpType
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
public class DMLOpType : RelOpBaseType
{
	private AssignType[] assignmentMapField;

	private ObjectType[] sourceTableField;

	private ObjectType[] targetTableField;

	private RelOpType[] relOpField;

	[XmlArrayItem("Assign", IsNullable = false)]
	public AssignType[] AssignmentMap
	{
		get
		{
			return assignmentMapField;
		}
		set
		{
			assignmentMapField = value;
		}
	}

	[XmlArrayItem("Object", IsNullable = false)]
	public ObjectType[] SourceTable
	{
		get
		{
			return sourceTableField;
		}
		set
		{
			sourceTableField = value;
		}
	}

	[XmlArrayItem("Object", IsNullable = false)]
	public ObjectType[] TargetTable
	{
		get
		{
			return targetTableField;
		}
		set
		{
			targetTableField = value;
		}
	}

	[XmlElement("RelOp")]
	public RelOpType[] RelOp
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
