// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.IndexScanType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class IndexScanType : RowsetType
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

	public SeekPredicatesType SeekPredicates
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
	public ScalarExpressionType[] Predicate
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

	[XmlArrayItem("Object", IsNullable = false)]
	public ObjectType[] IndexedViewInfo
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
	public bool Lookup
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
	public bool LookupSpecified
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
	public bool Ordered
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
	public EnOrderType ScanDirection
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
	public bool ScanDirectionSpecified
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
	public bool ForcedIndex
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
	public bool ForcedIndexSpecified
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
	public bool ForceSeek
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
	public bool ForceSeekSpecified
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
	public int ForceSeekColumnCount
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
	public bool ForceSeekColumnCountSpecified
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
	public bool ForceScan
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
	public bool ForceScanSpecified
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
	public bool NoExpandHint
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
	public bool NoExpandHintSpecified
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
	public EnStorageType Storage
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
	public bool StorageSpecified
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
	public bool DynamicSeek
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
	public bool DynamicSeekSpecified
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
	public string SBSFileUrl
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
