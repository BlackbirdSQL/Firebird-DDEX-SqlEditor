// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.NestedLoopsType
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
public class NestedLoopsType : RelOpBaseType
{
	private ScalarExpressionType predicateField;

	private ScalarExpressionType passThruField;

	private ColumnReferenceType[] outerReferencesField;

	private SingleColumnReferenceType partitionIdField;

	private SingleColumnReferenceType probeColumnField;

	private StarJoinInfoType starJoinInfoField;

	private RelOpType[] relOpField;

	private bool optimizedField;

	private bool withOrderedPrefetchField;

	private bool withOrderedPrefetchFieldSpecified;

	private bool withUnorderedPrefetchField;

	private bool withUnorderedPrefetchFieldSpecified;

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
