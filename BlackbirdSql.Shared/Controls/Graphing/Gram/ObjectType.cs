// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ObjectType
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
public class ObjectType
{
	private string serverField;

	private string databaseField;

	private string schemaField;

	private string tableField;

	private string indexField;

	private bool filteredField;

	private bool filteredFieldSpecified;

	private int onlineInbuildIndexField;

	private bool onlineInbuildIndexFieldSpecified;

	private int onlineIndexBuildMappingIndexField;

	private bool onlineIndexBuildMappingIndexFieldSpecified;

	private string aliasField;

	private int tableReferenceIdField;

	private bool tableReferenceIdFieldSpecified;

	private EnIndexKindType indexKindField;

	private bool indexKindFieldSpecified;

	private EnCloneAccessScopeType cloneAccessScopeField;

	private bool cloneAccessScopeFieldSpecified;

	private EnStorageType storageField;

	private bool storageFieldSpecified;

	private int graphWorkTableTypeField;

	private bool graphWorkTableTypeFieldSpecified;

	private int graphWorkTableIdentifierField;

	private bool graphWorkTableIdentifierFieldSpecified;

	[XmlAttribute]
	public string Server
	{
		get
		{
			return serverField;
		}
		set
		{
			serverField = value;
		}
	}

	[XmlAttribute]
	public string Database
	{
		get
		{
			return databaseField;
		}
		set
		{
			databaseField = value;
		}
	}

	[XmlAttribute]
	public string Schema
	{
		get
		{
			return schemaField;
		}
		set
		{
			schemaField = value;
		}
	}

	[XmlAttribute]
	public string Table
	{
		get
		{
			return tableField;
		}
		set
		{
			tableField = value;
		}
	}

	[XmlAttribute]
	public string Index
	{
		get
		{
			return indexField;
		}
		set
		{
			indexField = value;
		}
	}

	[XmlAttribute]
	public bool Filtered
	{
		get
		{
			return filteredField;
		}
		set
		{
			filteredField = value;
		}
	}

	[XmlIgnore]
	public bool FilteredSpecified
	{
		get
		{
			return filteredFieldSpecified;
		}
		set
		{
			filteredFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int OnlineInbuildIndex
	{
		get
		{
			return onlineInbuildIndexField;
		}
		set
		{
			onlineInbuildIndexField = value;
		}
	}

	[XmlIgnore]
	public bool OnlineInbuildIndexSpecified
	{
		get
		{
			return onlineInbuildIndexFieldSpecified;
		}
		set
		{
			onlineInbuildIndexFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int OnlineIndexBuildMappingIndex
	{
		get
		{
			return onlineIndexBuildMappingIndexField;
		}
		set
		{
			onlineIndexBuildMappingIndexField = value;
		}
	}

	[XmlIgnore]
	public bool OnlineIndexBuildMappingIndexSpecified
	{
		get
		{
			return onlineIndexBuildMappingIndexFieldSpecified;
		}
		set
		{
			onlineIndexBuildMappingIndexFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string Alias
	{
		get
		{
			return aliasField;
		}
		set
		{
			aliasField = value;
		}
	}

	[XmlAttribute]
	public int TableReferenceId
	{
		get
		{
			return tableReferenceIdField;
		}
		set
		{
			tableReferenceIdField = value;
		}
	}

	[XmlIgnore]
	public bool TableReferenceIdSpecified
	{
		get
		{
			return tableReferenceIdFieldSpecified;
		}
		set
		{
			tableReferenceIdFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public EnIndexKindType IndexKind
	{
		get
		{
			return indexKindField;
		}
		set
		{
			indexKindField = value;
		}
	}

	[XmlIgnore]
	public bool IndexKindSpecified
	{
		get
		{
			return indexKindFieldSpecified;
		}
		set
		{
			indexKindFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public EnCloneAccessScopeType CloneAccessScope
	{
		get
		{
			return cloneAccessScopeField;
		}
		set
		{
			cloneAccessScopeField = value;
		}
	}

	[XmlIgnore]
	public bool CloneAccessScopeSpecified
	{
		get
		{
			return cloneAccessScopeFieldSpecified;
		}
		set
		{
			cloneAccessScopeFieldSpecified = value;
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
	public int GraphWorkTableType
	{
		get
		{
			return graphWorkTableTypeField;
		}
		set
		{
			graphWorkTableTypeField = value;
		}
	}

	[XmlIgnore]
	public bool GraphWorkTableTypeSpecified
	{
		get
		{
			return graphWorkTableTypeFieldSpecified;
		}
		set
		{
			graphWorkTableTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int GraphWorkTableIdentifier
	{
		get
		{
			return graphWorkTableIdentifierField;
		}
		set
		{
			graphWorkTableIdentifierField = value;
		}
	}

	[XmlIgnore]
	public bool GraphWorkTableIdentifierSpecified
	{
		get
		{
			return graphWorkTableIdentifierFieldSpecified;
		}
		set
		{
			graphWorkTableIdentifierFieldSpecified = value;
		}
	}
}
