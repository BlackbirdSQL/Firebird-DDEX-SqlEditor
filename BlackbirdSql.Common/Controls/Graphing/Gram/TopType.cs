// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TopType
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
public class TopType : RelOpBaseType
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
	public ColumnReferenceType[] TieColumns
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

	public ScalarExpressionType OffsetExpression
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

	public ScalarExpressionType TopExpression
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

	public RelOpType RelOp
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
	public bool RowCount
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
	public bool RowCountSpecified
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
	public int Rows
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
	public bool RowsSpecified
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
	public bool IsPercent
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
	public bool IsPercentSpecified
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
	public bool WithTies
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
	public bool WithTiesSpecified
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
	public string TopLocation
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
