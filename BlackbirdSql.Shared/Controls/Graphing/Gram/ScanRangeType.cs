// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ScanRangeType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class ScanRangeType
{
	private ColumnReferenceType[] rangeColumnsField;

	private ScalarType[] rangeExpressionsField;

	private EnCompareOpType scanTypeField;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] RangeColumns
	{
		get
		{
			return rangeColumnsField;
		}
		set
		{
			rangeColumnsField = value;
		}
	}

	[XmlArrayItem("ScalarOperator", IsNullable = false)]
	public ScalarType[] RangeExpressions
	{
		get
		{
			return rangeExpressionsField;
		}
		set
		{
			rangeExpressionsField = value;
		}
	}

	[XmlAttribute]
	public EnCompareOpType ScanType
	{
		get
		{
			return scanTypeField;
		}
		set
		{
			scanTypeField = value;
		}
	}
}
