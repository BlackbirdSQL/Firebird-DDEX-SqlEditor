// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MoveType
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
public class MoveType : RelOpBaseType
{
	private ColumnReferenceType[] distributionKeyField;

	private RelOpType[] relOpField;

	private string moveType1Field;

	private string distributionTypeField;

	private bool isDistributedField;

	private bool isDistributedFieldSpecified;

	private bool isExternalField;

	private bool isExternalFieldSpecified;

	private bool isFullField;

	private bool isFullFieldSpecified;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] DistributionKey
	{
		get
		{
			return distributionKeyField;
		}
		set
		{
			distributionKeyField = value;
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

	[XmlAttribute("MoveType")]
	public string MoveType1
	{
		get
		{
			return moveType1Field;
		}
		set
		{
			moveType1Field = value;
		}
	}

	[XmlAttribute]
	public string DistributionType
	{
		get
		{
			return distributionTypeField;
		}
		set
		{
			distributionTypeField = value;
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
	public bool IsFull
	{
		get
		{
			return isFullField;
		}
		set
		{
			isFullField = value;
		}
	}

	[XmlIgnore]
	public bool IsFullSpecified
	{
		get
		{
			return isFullFieldSpecified;
		}
		set
		{
			isFullFieldSpecified = value;
		}
	}
}
