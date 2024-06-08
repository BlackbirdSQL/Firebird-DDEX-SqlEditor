// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ParallelismType
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
public class ParallelismType : RelOpBaseType
{
	private ColumnReferenceType[] partitionColumnsField;

	private OrderByTypeOrderByColumn[] orderByField;

	private ColumnReferenceType[] hashKeysField;

	private SingleColumnReferenceType probeColumnField;

	private ScalarExpressionType predicateField;

	private ParallelismTypeActivation activationField;

	private ParallelismTypeBrickRouting brickRoutingField;

	private RelOpType relOpField;

	private EnPartitionType partitioningTypeField;

	private bool partitioningTypeFieldSpecified;

	private bool remotingField;

	private bool remotingFieldSpecified;

	private bool localParallelismField;

	private bool localParallelismFieldSpecified;

	private bool inRowField;

	private bool inRowFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] PartitionColumns
	{
		get
		{
			return partitionColumnsField;
		}
		set
		{
			partitionColumnsField = value;
		}
	}

	[XmlArrayItem("OrderByColumn", IsNullable = false)]
	public OrderByTypeOrderByColumn[] OrderBy
	{
		get
		{
			return orderByField;
		}
		set
		{
			orderByField = value;
		}
	}

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] HashKeys
	{
		get
		{
			return hashKeysField;
		}
		set
		{
			hashKeysField = value;
		}
	}

	public SingleColumnReferenceType ProbeColumn
	{
		get
		{
			return probeColumnField;
		}
		set
		{
			probeColumnField = value;
		}
	}

	public ScalarExpressionType Predicate
	{
		get
		{
			return predicateField;
		}
		set
		{
			predicateField = value;
		}
	}

	public ParallelismTypeActivation Activation
	{
		get
		{
			return activationField;
		}
		set
		{
			activationField = value;
		}
	}

	public ParallelismTypeBrickRouting BrickRouting
	{
		get
		{
			return brickRoutingField;
		}
		set
		{
			brickRoutingField = value;
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
	public EnPartitionType PartitioningType
	{
		get
		{
			return partitioningTypeField;
		}
		set
		{
			partitioningTypeField = value;
		}
	}

	[XmlIgnore]
	public bool PartitioningTypeSpecified
	{
		get
		{
			return partitioningTypeFieldSpecified;
		}
		set
		{
			partitioningTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool Remoting
	{
		get
		{
			return remotingField;
		}
		set
		{
			remotingField = value;
		}
	}

	[XmlIgnore]
	public bool RemotingSpecified
	{
		get
		{
			return remotingFieldSpecified;
		}
		set
		{
			remotingFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool LocalParallelism
	{
		get
		{
			return localParallelismField;
		}
		set
		{
			localParallelismField = value;
		}
	}

	[XmlIgnore]
	public bool LocalParallelismSpecified
	{
		get
		{
			return localParallelismFieldSpecified;
		}
		set
		{
			localParallelismFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool InRow
	{
		get
		{
			return inRowField;
		}
		set
		{
			inRowField = value;
		}
	}

	[XmlIgnore]
	public bool InRowSpecified
	{
		get
		{
			return inRowFieldSpecified;
		}
		set
		{
			inRowFieldSpecified = value;
		}
	}
}
