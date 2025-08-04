// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TraceFlagListType
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
internal class TraceFlagListType
{
	private TraceFlagType[] traceFlagField;

	private bool isCompileTimeField;

	[XmlElement("TraceFlag")]
	internal TraceFlagType[] TraceFlag
	{
		get
		{
			return traceFlagField;
		}
		set
		{
			traceFlagField = value;
		}
	}

	[XmlAttribute]
	internal bool IsCompileTime
	{
		get
		{
			return isCompileTimeField;
		}
		set
		{
			isCompileTimeField = value;
		}
	}
}
