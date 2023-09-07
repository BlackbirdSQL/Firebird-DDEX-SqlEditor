// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.UDAggregateType
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
public class UDAggregateType
{
	private ObjectType uDAggObjectField;

	private ScalarType[] scalarOperatorField;

	private bool distinctField;

	public ObjectType UDAggObject
	{
		get
		{
			return uDAggObjectField;
		}
		set
		{
			uDAggObjectField = value;
		}
	}

	[XmlElement("ScalarOperator")]
	public ScalarType[] ScalarOperator
	{
		get
		{
			return scalarOperatorField;
		}
		set
		{
			scalarOperatorField = value;
		}
	}

	[XmlAttribute]
	public bool Distinct
	{
		get
		{
			return distinctField;
		}
		set
		{
			distinctField = value;
		}
	}
}
