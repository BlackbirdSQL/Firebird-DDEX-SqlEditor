// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ParallelismTypeActivation
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;
using BlackbirdSql.Shared.Controls.Graphing.Gram;

namespace BlackbirdSql.Shared.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
internal class ParallelismTypeActivation
{
	private ObjectType objectField;

	private EnParallelismTypeActivationType typeField;

	private string fragmentEliminationField;

	internal ObjectType Object
	{
		get
		{
			return objectField;
		}
		set
		{
			objectField = value;
		}
	}

	[XmlAttribute]
	internal EnParallelismTypeActivationType Type
	{
		get
		{
			return typeField;
		}
		set
		{
			typeField = value;
		}
	}

	[XmlAttribute]
	internal string FragmentElimination
	{
		get
		{
			return fragmentEliminationField;
		}
		set
		{
			fragmentEliminationField = value;
		}
	}
}
