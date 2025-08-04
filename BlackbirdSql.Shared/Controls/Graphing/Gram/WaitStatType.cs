// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.WaitStatType
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
internal class WaitStatType
{
	private string waitTypeField;

	private ulong waitTimeMsField;

	private ulong waitCountField;

	[XmlAttribute]
	internal string WaitType
	{
		get
		{
			return waitTypeField;
		}
		set
		{
			waitTypeField = value;
		}
	}

	[XmlAttribute]
	internal ulong WaitTimeMs
	{
		get
		{
			return waitTimeMsField;
		}
		set
		{
			waitTimeMsField = value;
		}
	}

	[XmlAttribute]
	internal ulong WaitCount
	{
		get
		{
			return waitCountField;
		}
		set
		{
			waitCountField = value;
		}
	}
}
