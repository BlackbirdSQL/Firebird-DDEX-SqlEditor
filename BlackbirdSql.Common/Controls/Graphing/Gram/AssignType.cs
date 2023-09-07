// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.AssignType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class AssignType
{
	private object itemField;

	private ScalarType scalarOperatorField;

	private ColumnReferenceType[] sourceColumnField;

	private ColumnReferenceType[] targetColumnField;

	[XmlElement("ColumnReference", typeof(ColumnReferenceType), Order = 0)]
	[XmlElement("ScalarOperator", typeof(ScalarType), Order = 0)]
	public object Item
	{
		get
		{
			return itemField;
		}
		set
		{
			itemField = value;
		}
	}

	[XmlElement(Order = 1)]
	public ScalarType ScalarOperator
	{
		get
		{
			return scalarOperatorField;
		}
		set
		{
			scalarOperatorField = value;
		}
	}

	[XmlElement("SourceColumn", Order = 2)]
	public ColumnReferenceType[] SourceColumn
	{
		get
		{
			return sourceColumnField;
		}
		set
		{
			sourceColumnField = value;
		}
	}

	[XmlElement("TargetColumn", Order = 3)]
	public ColumnReferenceType[] TargetColumn
	{
		get
		{
			return targetColumnField;
		}
		set
		{
			targetColumnField = value;
		}
	}
}
