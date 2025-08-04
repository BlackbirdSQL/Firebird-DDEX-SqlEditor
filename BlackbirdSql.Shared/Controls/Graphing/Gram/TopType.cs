// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TopType
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
internal class TopType : RelOpBaseType
{
	private ColumnReferenceType[] tieColumnsField;

	private ScalarExpressionType offsetExpressionField;

	private ScalarExpressionType topExpressionField;

	private RelOpType relOpField;

	private bool rowCountField;

	private bool rowCountFieldSpecified;

	private int rowsField;

	private bool rowsFieldSpecified;

	private bool isPercentField;

	private bool isPercentFieldSpecified;

	private bool withTiesField;

	private bool withTiesFieldSpecified;

	private string topLocationField;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	internal ColumnReferenceType[] TieColumns
	{
		get
		{
			return tieColumnsField;
		}
		set
		{
			tieColumnsField = value;
		}
	}

	internal ScalarExpressionType OffsetExpression
	{
		get
		{
			return offsetExpressionField;
		}
		set
		{
			offsetExpressionField = value;
		}
	}

	internal ScalarExpressionType TopExpression
	{
		get
		{
			return topExpressionField;
		}
		set
		{
			topExpressionField = value;
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

	[XmlAttribute]
	internal bool RowCount
	{
		get
		{
			return rowCountField;
		}
		set
		{
			rowCountField = value;
		}
	}

	[XmlIgnore]
	internal bool RowCountSpecified
	{
		get
		{
			return rowCountFieldSpecified;
		}
		set
		{
			rowCountFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal int Rows
	{
		get
		{
			return rowsField;
		}
		set
		{
			rowsField = value;
		}
	}

	[XmlIgnore]
	internal bool RowsSpecified
	{
		get
		{
			return rowsFieldSpecified;
		}
		set
		{
			rowsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool IsPercent
	{
		get
		{
			return isPercentField;
		}
		set
		{
			isPercentField = value;
		}
	}

	[XmlIgnore]
	internal bool IsPercentSpecified
	{
		get
		{
			return isPercentFieldSpecified;
		}
		set
		{
			isPercentFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool WithTies
	{
		get
		{
			return withTiesField;
		}
		set
		{
			withTiesField = value;
		}
	}

	[XmlIgnore]
	internal bool WithTiesSpecified
	{
		get
		{
			return withTiesFieldSpecified;
		}
		set
		{
			withTiesFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal string TopLocation
	{
		get
		{
			return topLocationField;
		}
		set
		{
			topLocationField = value;
		}
	}
}
