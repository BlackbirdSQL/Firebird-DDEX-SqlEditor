// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.GbAggType
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
public class GbAggType : RelOpBaseType
{
	private ColumnReferenceType[] groupByField;

	private DefinedValuesListTypeDefinedValue[] aggFunctionsField;

	private RelOpType[] relOpField;

	private bool isScalarField;

	private bool isScalarFieldSpecified;

	private string aggTypeField;

	private string hintTypeField;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] GroupBy
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

	[XmlArrayItem("DefinedValue", IsNullable = false)]
	public DefinedValuesListTypeDefinedValue[] AggFunctions
	{
		get
		{
			return aggFunctionsField;
		}
		set
		{
			aggFunctionsField = value;
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

	[XmlAttribute]
	public bool IsScalar
	{
		get
		{
			return isScalarField;
		}
		set
		{
			isScalarField = value;
		}
	}

	[XmlIgnore]
	public bool IsScalarSpecified
	{
		get
		{
			return isScalarFieldSpecified;
		}
		set
		{
			isScalarFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string AggType
	{
		get
		{
			return aggTypeField;
		}
		set
		{
			aggTypeField = value;
		}
	}

	[XmlAttribute]
	public string HintType
	{
		get
		{
			return hintTypeField;
		}
		set
		{
			hintTypeField = value;
		}
	}
}
