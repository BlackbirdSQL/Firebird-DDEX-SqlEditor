// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.UpdateType
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
internal class UpdateType : RowsetType
{
	private SetPredicateElementType[] setPredicateField;

	private SingleColumnReferenceType probeColumnField;

	private SingleColumnReferenceType actionColumnField;

	private SingleColumnReferenceType originalActionColumnField;

	private AssignType[] assignmentMapField;

	private ObjectType[] sourceTableField;

	private ObjectType[] targetTableField;

	private RelOpType relOpField;

	private bool withOrderedPrefetchField;

	private bool withOrderedPrefetchFieldSpecified;

	private bool withUnorderedPrefetchField;

	private bool withUnorderedPrefetchFieldSpecified;

	private bool dMLRequestSortField;

	private bool dMLRequestSortFieldSpecified;

	[XmlElement("SetPredicate")]
	internal SetPredicateElementType[] SetPredicate
	{
		get
		{
			return setPredicateField;
		}
		set
		{
			setPredicateField = value;
		}
	}

	internal SingleColumnReferenceType ProbeColumn
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

	internal SingleColumnReferenceType ActionColumn
	{
		get
		{
			return actionColumnField;
		}
		set
		{
			actionColumnField = value;
		}
	}

	internal SingleColumnReferenceType OriginalActionColumn
	{
		get
		{
			return originalActionColumnField;
		}
		set
		{
			originalActionColumnField = value;
		}
	}

	[XmlArrayItem("Assign", IsNullable = false)]
	internal AssignType[] AssignmentMap
	{
		get
		{
			return assignmentMapField;
		}
		set
		{
			assignmentMapField = value;
		}
	}

	[XmlArrayItem("Object", IsNullable = false)]
	internal ObjectType[] SourceTable
	{
		get
		{
			return sourceTableField;
		}
		set
		{
			sourceTableField = value;
		}
	}

	[XmlArrayItem("Object", IsNullable = false)]
	internal ObjectType[] TargetTable
	{
		get
		{
			return targetTableField;
		}
		set
		{
			targetTableField = value;
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
	internal bool WithOrderedPrefetch
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
	internal bool WithOrderedPrefetchSpecified
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
	internal bool WithUnorderedPrefetch
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
	internal bool WithUnorderedPrefetchSpecified
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

	[XmlAttribute]
	internal bool DMLRequestSort
	{
		get
		{
			return dMLRequestSortField;
		}
		set
		{
			dMLRequestSortField = value;
		}
	}

	[XmlIgnore]
	internal bool DMLRequestSortSpecified
	{
		get
		{
			return dMLRequestSortFieldSpecified;
		}
		set
		{
			dMLRequestSortFieldSpecified = value;
		}
	}
}
