// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TableScanType
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
[XmlType(Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class TableScanType : RowsetType
{
	private ScalarExpressionType predicateField;

	private SingleColumnReferenceType partitionIdField;

	private ObjectType[] indexedViewInfoField;

	private bool orderedField;

	private bool forcedIndexField;

	private bool forcedIndexFieldSpecified;

	private bool forceScanField;

	private bool forceScanFieldSpecified;

	private bool noExpandHintField;

	private bool noExpandHintFieldSpecified;

	private EnStorageType storageField;

	private bool storageFieldSpecified;

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
}
