// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SeekPredicateType
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
internal class SeekPredicateType
{
	private ScanRangeType prefixField;

	private ScanRangeType startRangeField;

	private ScanRangeType endRangeField;

	private SingleColumnReferenceType isNotNullField;

	internal ScanRangeType Prefix
	{
		get
		{
			return prefixField;
		}
		set
		{
			prefixField = value;
		}
	}

	internal ScanRangeType StartRange
	{
		get
		{
			return startRangeField;
		}
		set
		{
			startRangeField = value;
		}
	}

	internal ScanRangeType EndRange
	{
		get
		{
			return endRangeField;
		}
		set
		{
			endRangeField = value;
		}
	}

	internal SingleColumnReferenceType IsNotNull
	{
		get
		{
			return isNotNullField;
		}
		set
		{
			isNotNullField = value;
		}
	}
}
