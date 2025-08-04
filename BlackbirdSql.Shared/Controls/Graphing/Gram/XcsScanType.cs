// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.XcsScanType
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
internal class XcsScanType : RowsetType
{
	private ScalarExpressionType predicateField;

	private SingleColumnReferenceType partitionIdField;

	private ObjectType[] indexedViewInfoField;

	private RelOpType relOpField;

	private bool orderedField;

	private bool forcedIndexField;

	private bool forcedIndexFieldSpecified;

	private bool forceScanField;

	private bool forceScanFieldSpecified;

	private bool noExpandHintField;

	private bool noExpandHintFieldSpecified;

	private EnStorageType storageField;

	private bool storageFieldSpecified;

	internal ScalarExpressionType Predicate
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

	internal SingleColumnReferenceType PartitionId
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

	[XmlArrayItem("Object", IsNullable = false)]
	internal ObjectType[] IndexedViewInfo
	{
		get
		{
			return indexedViewInfoField;
		}
		set
		{
			indexedViewInfoField = value;
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
	internal bool Ordered
	{
		get
		{
			return orderedField;
		}
		set
		{
			orderedField = value;
		}
	}

	[XmlAttribute]
	internal bool ForcedIndex
	{
		get
		{
			return forcedIndexField;
		}
		set
		{
			forcedIndexField = value;
		}
	}

	[XmlIgnore]
	internal bool ForcedIndexSpecified
	{
		get
		{
			return forcedIndexFieldSpecified;
		}
		set
		{
			forcedIndexFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool ForceScan
	{
		get
		{
			return forceScanField;
		}
		set
		{
			forceScanField = value;
		}
	}

	[XmlIgnore]
	internal bool ForceScanSpecified
	{
		get
		{
			return forceScanFieldSpecified;
		}
		set
		{
			forceScanFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal bool NoExpandHint
	{
		get
		{
			return noExpandHintField;
		}
		set
		{
			noExpandHintField = value;
		}
	}

	[XmlIgnore]
	internal bool NoExpandHintSpecified
	{
		get
		{
			return noExpandHintFieldSpecified;
		}
		set
		{
			noExpandHintFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal EnStorageType Storage
	{
		get
		{
			return storageField;
		}
		set
		{
			storageField = value;
		}
	}

	[XmlIgnore]
	internal bool StorageSpecified
	{
		get
		{
			return storageFieldSpecified;
		}
		set
		{
			storageFieldSpecified = value;
		}
	}
}
