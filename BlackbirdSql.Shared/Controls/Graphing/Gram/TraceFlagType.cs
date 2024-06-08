// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TraceFlagType
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
public class TraceFlagType
{
	private ulong valueField;

	private EnTraceFlagScopeType scopeField;

	[XmlAttribute]
	public ulong Value
	{
		get
		{
			return valueField;
		}
		set
		{
			valueField = value;
		}
	}

	[XmlAttribute]
	public EnTraceFlagScopeType Scope
	{
		get
		{
			return scopeField;
		}
		set
		{
			scopeField = value;
		}
	}
}
