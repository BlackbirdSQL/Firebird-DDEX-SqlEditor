// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ExternalSelectType
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
internal class ExternalSelectType : RelOpBaseType
{
	private RelOpType[] relOpField;

	private string materializeOperationField;

	private string distributionTypeField;

	private bool isDistributedField;

	private bool isDistributedFieldSpecified;

	private bool isExternalField;

	private bool isExternalFieldSpecified;

	private bool isFullField;

	private bool isFullFieldSpecified;

	[XmlElement("RelOp")]
	internal RelOpType[] RelOp
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
	internal string MaterializeOperation
	{
		get
		{
			return materializeOperationField;
		}
		set
		{
			materializeOperationField = value;
		}
	}

	[XmlAttribute]
	internal string DistributionType
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
	internal bool IsDistributed
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
	internal bool IsDistributedSpecified
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
	internal bool IsExternal
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
	internal bool IsExternalSpecified
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
	internal bool IsFull
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
	internal bool IsFullSpecified
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
