// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StarJoinInfoType
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
public class StarJoinInfoType
{
	private bool rootField;

	private bool rootFieldSpecified;

	private EnStarJoinInfoTypeOperationType operationTypeField;

	[XmlAttribute]
	public bool Root
	{
		get
		{
			return rootField;
		}
		set
		{
			rootField = value;
		}
	}

	[XmlIgnore]
	public bool RootSpecified
	{
		get
		{
			return rootFieldSpecified;
		}
		set
		{
			rootFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public EnStarJoinInfoTypeOperationType OperationType
	{
		get
		{
			return operationTypeField;
		}
		set
		{
			operationTypeField = value;
		}
	}
}
