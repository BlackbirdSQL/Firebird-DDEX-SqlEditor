// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.GetType
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
public class GetType : RelOpBaseType
{
	private ColumnReferenceType[] bookmarksField;

	private OutputColumnsType outputColumnsField;

	private ScalarType[] generatedDataField;

	private RelOpType[] relOpField;

	private int numRowsField;

	private bool numRowsFieldSpecified;

	private bool isExternalField;

	private bool isExternalFieldSpecified;

	private bool isDistributedField;

	private bool isDistributedFieldSpecified;

	private bool isHashDistributedField;

	private bool isHashDistributedFieldSpecified;

	private bool isReplicatedField;

	private bool isReplicatedFieldSpecified;

	private bool isRoundRobinField;

	private bool isRoundRobinFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] Bookmarks
	{
		get
		{
			return bookmarksField;
		}
		set
		{
			bookmarksField = value;
		}
	}

	public OutputColumnsType OutputColumns
	{
		get
		{
			return outputColumnsField;
		}
		set
		{
			outputColumnsField = value;
		}
	}

	[XmlArrayItem("ScalarOperator", IsNullable = false)]
	public ScalarType[] GeneratedData
	{
		get
		{
			return generatedDataField;
		}
		set
		{
			generatedDataField = value;
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
	public int NumRows
	{
		get
		{
			return numRowsField;
		}
		set
		{
			numRowsField = value;
		}
	}

	[XmlIgnore]
	public bool NumRowsSpecified
	{
		get
		{
			return numRowsFieldSpecified;
		}
		set
		{
			numRowsFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool IsExternal
	{
		get
		{
			return isExternalField;
		}
		set
		{
			isExternalField = value;
		}
	}

	[XmlIgnore]
	public bool IsExternalSpecified
	{
		get
		{
			return isExternalFieldSpecified;
		}
		set
		{
			isExternalFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool IsDistributed
	{
		get
		{
			return isDistributedField;
		}
		set
		{
			isDistributedField = value;
		}
	}

	[XmlIgnore]
	public bool IsDistributedSpecified
	{
		get
		{
			return isDistributedFieldSpecified;
		}
		set
		{
			isDistributedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool IsHashDistributed
	{
		get
		{
			return isHashDistributedField;
		}
		set
		{
			isHashDistributedField = value;
		}
	}

	[XmlIgnore]
	public bool IsHashDistributedSpecified
	{
		get
		{
			return isHashDistributedFieldSpecified;
		}
		set
		{
			isHashDistributedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool IsReplicated
	{
		get
		{
			return isReplicatedField;
		}
		set
		{
			isReplicatedField = value;
		}
	}

	[XmlIgnore]
	public bool IsReplicatedSpecified
	{
		get
		{
			return isReplicatedFieldSpecified;
		}
		set
		{
			isReplicatedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool IsRoundRobin
	{
		get
		{
			return isRoundRobinField;
		}
		set
		{
			isRoundRobinField = value;
		}
	}

	[XmlIgnore]
	public bool IsRoundRobinSpecified
	{
		get
		{
			return isRoundRobinFieldSpecified;
		}
		set
		{
			isRoundRobinFieldSpecified = value;
		}
	}
}
