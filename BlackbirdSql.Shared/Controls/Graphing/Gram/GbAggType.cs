// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.GbAggType
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
internal class GbAggType : RelOpBaseType
{
	private ColumnReferenceType[] groupByField;

	private DefinedValuesListTypeDefinedValue[] aggFunctionsField;

	private RelOpType[] relOpField;

	private bool isScalarField;

	private bool isScalarFieldSpecified;

	private string aggTypeField;

	private string hintTypeField;

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

	[XmlArrayItem("DefinedValue", IsNullable = false)]
	internal DefinedValuesListTypeDefinedValue[] AggFunctions
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
	internal RelOpType[] RelOp
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
	internal bool IsScalar
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
	internal bool IsScalarSpecified
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
	internal string AggType
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
	internal string HintType
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
