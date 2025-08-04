// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.IndexScanType
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
internal class IndexScanType : RowsetType
{
	private SeekPredicatesType seekPredicatesField;

	private ScalarExpressionType[] predicateField;

	private SingleColumnReferenceType partitionIdField;

	private ObjectType[] indexedViewInfoField;

	private bool lookupField;

	private bool lookupFieldSpecified;

	private bool orderedField;

	private EnOrderType scanDirectionField;

	private bool scanDirectionFieldSpecified;

	private bool forcedIndexField;

	private bool forcedIndexFieldSpecified;

	private bool forceSeekField;

	private bool forceSeekFieldSpecified;

	private int forceSeekColumnCountField;

	private bool forceSeekColumnCountFieldSpecified;

	private bool forceScanField;

	private bool forceScanFieldSpecified;

	private bool noExpandHintField;

	private bool noExpandHintFieldSpecified;

	private EnStorageType storageField;

	private bool storageFieldSpecified;

	private bool dynamicSeekField;

	private bool dynamicSeekFieldSpecified;

	private string sBSFileUrlField;

	internal SeekPredicatesType SeekPredicates
	{
		get
		{
			return seekPredicatesField;
		}
		set
		{
			seekPredicatesField = value;
		}
	}

	[XmlElement("Predicate")]
	internal ScalarExpressionType[] Predicate
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

	[XmlAttribute]
	internal bool Lookup
	{
		get
		{
			return lookupField;
		}
		set
		{
			lookupField = value;
		}
	}

	[XmlIgnore]
	internal bool LookupSpecified
	{
		get
		{
			return lookupFieldSpecified;
		}
		set
		{
			lookupFieldSpecified = value;
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
	internal EnOrderType ScanDirection
	{
		get
		{
			return scanDirectionField;
		}
		set
		{
			scanDirectionField = value;
		}
	}

	[XmlIgnore]
	internal bool ScanDirectionSpecified
	{
		get
		{
			return scanDirectionFieldSpecified;
		}
		set
		{
			scanDirectionFieldSpecified = value;
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
	internal bool ForceSeek
	{
		get
		{
			return forceSeekField;
		}
		set
		{
			forceSeekField = value;
		}
	}

	[XmlIgnore]
	internal bool ForceSeekSpecified
	{
		get
		{
			return forceSeekFieldSpecified;
		}
		set
		{
			forceSeekFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal int ForceSeekColumnCount
	{
		get
		{
			return forceSeekColumnCountField;
		}
		set
		{
			forceSeekColumnCountField = value;
		}
	}

	[XmlIgnore]
	internal bool ForceSeekColumnCountSpecified
	{
		get
		{
			return forceSeekColumnCountFieldSpecified;
		}
		set
		{
			forceSeekColumnCountFieldSpecified = value;
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

	[XmlAttribute]
	internal bool DynamicSeek
	{
		get
		{
			return dynamicSeekField;
		}
		set
		{
			dynamicSeekField = value;
		}
	}

	[XmlIgnore]
	internal bool DynamicSeekSpecified
	{
		get
		{
			return dynamicSeekFieldSpecified;
		}
		set
		{
			dynamicSeekFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal string SBSFileUrl
	{
		get
		{
			return sBSFileUrlField;
		}
		set
		{
			sBSFileUrlField = value;
		}
	}
}
