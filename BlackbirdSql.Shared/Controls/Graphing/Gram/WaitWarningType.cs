// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.WaitWarningType
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
internal class WaitWarningType
{
	private EnWaitWarningTypeWaitType waitTypeField;

	private ulong waitTimeField;

	private bool waitTimeFieldSpecified;

	[XmlAttribute]
	internal EnWaitWarningTypeWaitType WaitType
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
	internal ulong WaitTime
	{
		get
		{
			return waitTimeField;
		}
		set
		{
			waitTimeField = value;
		}
	}

	[XmlIgnore]
	internal bool WaitTimeSpecified
	{
		get
		{
			return waitTimeFieldSpecified;
		}
		set
		{
			waitTimeFieldSpecified = value;
		}
	}
}
