// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.PutType
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
public class PutType : RemoteQueryType
{
	private RelOpType relOpField;

	private bool isExternallyComputedField;

	private bool isExternallyComputedFieldSpecified;

	private string shuffleTypeField;

	private string shuffleColumnField;

	public RelOpType RelOp
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
	public bool IsExternallyComputed
	{
		get
		{
			return isExternallyComputedField;
		}
		set
		{
			isExternallyComputedField = value;
		}
	}

	[XmlIgnore]
	public bool IsExternallyComputedSpecified
	{
		get
		{
			return isExternallyComputedFieldSpecified;
		}
		set
		{
			isExternallyComputedFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public string ShuffleType
	{
		get
		{
			return shuffleTypeField;
		}
		set
		{
			shuffleTypeField = value;
		}
	}

	[XmlAttribute]
	public string ShuffleColumn
	{
		get
		{
			return shuffleColumnField;
		}
		set
		{
			shuffleColumnField = value;
		}
	}
}
