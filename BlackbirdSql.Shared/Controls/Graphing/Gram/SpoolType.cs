// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SpoolType
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
public class SpoolType : RelOpBaseType
{
	private object itemField;

	private RelOpType relOpField;

	private bool stackField;

	private bool stackFieldSpecified;

	private int primaryNodeIdField;

	private bool primaryNodeIdFieldSpecified;

	[XmlElement("SeekPredicate", typeof(SeekPredicateType))]
	[XmlElement("SeekPredicateNew", typeof(SeekPredicateNewType))]
	public object Item
	{
		get
		{
			return itemField;
		}
		set
		{
			itemField = value;
		}
	}

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
	public bool Stack
	{
		get
		{
			return stackField;
		}
		set
		{
			stackField = value;
		}
	}

	[XmlIgnore]
	public bool StackSpecified
	{
		get
		{
			return stackFieldSpecified;
		}
		set
		{
			stackFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int PrimaryNodeId
	{
		get
		{
			return primaryNodeIdField;
		}
		set
		{
			primaryNodeIdField = value;
		}
	}

	[XmlIgnore]
	public bool PrimaryNodeIdSpecified
	{
		get
		{
			return primaryNodeIdFieldSpecified;
		}
		set
		{
			primaryNodeIdFieldSpecified = value;
		}
	}
}
