// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ScalarType
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
public class ScalarType
{
	private object itemField;

	private InternalInfoType internalInfoField;

	private string scalarStringField;

	[XmlElement("Aggregate", typeof(AggregateType))]
	[XmlElement("Arithmetic", typeof(ArithmeticType))]
	[XmlElement("Assign", typeof(AssignType))]
	[XmlElement("Compare", typeof(CompareType))]
	[XmlElement("Const", typeof(ConstType))]
	[XmlElement("Convert", typeof(ConvertType))]
	[XmlElement("IF", typeof(ConditionalType))]
	[XmlElement("Identifier", typeof(IdentType))]
	[XmlElement("Intrinsic", typeof(IntrinsicType))]
	[XmlElement("Logical", typeof(LogicalType))]
	[XmlElement("MultipleAssign", typeof(MultAssignType))]
	[XmlElement("ScalarExpressionList", typeof(ScalarExpressionListType))]
	[XmlElement("Sequence", typeof(ScalarSequenceType))]
	[XmlElement("Subquery", typeof(SubqueryType))]
	[XmlElement("UDTMethod", typeof(UDTMethodType))]
	[XmlElement("UserDefinedAggregate", typeof(UDAggregateType))]
	[XmlElement("UserDefinedFunction", typeof(UDFType))]
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

	public InternalInfoType InternalInfo
	{
		get
		{
			return internalInfoField;
		}
		set
		{
			internalInfoField = value;
		}
	}

	[XmlAttribute]
	public string ScalarString
	{
		get
		{
			return scalarStringField;
		}
		set
		{
			scalarStringField = value;
		}
	}
}
