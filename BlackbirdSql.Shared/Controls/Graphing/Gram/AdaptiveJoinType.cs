// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.AdaptiveJoinType
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
public class AdaptiveJoinType : RelOpBaseType
{
	private ColumnReferenceType[] hashKeysBuildField;

	private ColumnReferenceType[] hashKeysProbeField;

	private ScalarExpressionType buildResidualField;

	private ScalarExpressionType probeResidualField;

	private StarJoinInfoType starJoinInfoField;

	private ScalarExpressionType predicateField;

	private ScalarExpressionType passThruField;

	private ColumnReferenceType[] outerReferencesField;

	private SingleColumnReferenceType partitionIdField;

	private RelOpType[] relOpField;

	private bool bitmapCreatorField;

	private bool bitmapCreatorFieldSpecified;

	private bool optimizedField;

	private bool withOrderedPrefetchField;

	private bool withOrderedPrefetchFieldSpecified;

	private bool withUnorderedPrefetchField;

	private bool withUnorderedPrefetchFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] HashKeysBuild
	{
		get
		{
			return hashKeysBuildField;
		}
		set
		{
			hashKeysBuildField = value;
		}
	}

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] HashKeysProbe
	{
		get
		{
			return hashKeysProbeField;
		}
		set
		{
			hashKeysProbeField = value;
		}
	}

	public ScalarExpressionType BuildResidual
	{
		get
		{
			return buildResidualField;
		}
		set
		{
			buildResidualField = value;
		}
	}

	public ScalarExpressionType ProbeResidual
	{
		get
		{
			return probeResidualField;
		}
		set
		{
			probeResidualField = value;
		}
	}

	public StarJoinInfoType StarJoinInfo
	{
		get
		{
			return starJoinInfoField;
		}
		set
		{
			starJoinInfoField = value;
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

	public ScalarExpressionType PassThru
	{
		get
		{
			return passThruField;
		}
		set
		{
			passThruField = value;
		}
	}

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] OuterReferences
	{
		get
		{
			return outerReferencesField;
		}
		set
		{
			outerReferencesField = value;
		}
	}

	public SingleColumnReferenceType PartitionId
	{
		get
		{
			return partitionIdField;
		}
		set
		{
			partitionIdField = value;
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
	public bool BitmapCreator
	{
		get
		{
			return bitmapCreatorField;
		}
		set
		{
			bitmapCreatorField = value;
		}
	}

	[XmlIgnore]
	public bool BitmapCreatorSpecified
	{
		get
		{
			return bitmapCreatorFieldSpecified;
		}
		set
		{
			bitmapCreatorFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool Optimized
	{
		get
		{
			return optimizedField;
		}
		set
		{
			optimizedField = value;
		}
	}

	[XmlAttribute]
	public bool WithOrderedPrefetch
	{
		get
		{
			return withOrderedPrefetchField;
		}
		set
		{
			withOrderedPrefetchField = value;
		}
	}

	[XmlIgnore]
	public bool WithOrderedPrefetchSpecified
	{
		get
		{
			return withOrderedPrefetchFieldSpecified;
		}
		set
		{
			withOrderedPrefetchFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool WithUnorderedPrefetch
	{
		get
		{
			return withUnorderedPrefetchField;
		}
		set
		{
			withUnorderedPrefetchField = value;
		}
	}

	[XmlIgnore]
	public bool WithUnorderedPrefetchSpecified
	{
		get
		{
			return withUnorderedPrefetchFieldSpecified;
		}
		set
		{
			withUnorderedPrefetchFieldSpecified = value;
		}
	}
}
