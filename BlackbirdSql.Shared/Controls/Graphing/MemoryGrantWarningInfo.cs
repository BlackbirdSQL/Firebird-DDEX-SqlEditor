// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MemoryGrantWarningInfo
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;

namespace BlackbirdSql.Shared.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class MemoryGrantWarningInfo
{
	private EnMemoryGrantWarningType grantWarningKindField;

	private ulong requestedMemoryField;

	private ulong grantedMemoryField;

	private ulong maxUsedMemoryField;

	[XmlAttribute]
	public EnMemoryGrantWarningType GrantWarningKind
	{
		get
		{
			return grantWarningKindField;
		}
		set
		{
			grantWarningKindField = value;
		}
	}

	[XmlAttribute]
	public ulong RequestedMemory
	{
		get
		{
			return requestedMemoryField;
		}
		set
		{
			requestedMemoryField = value;
		}
	}

	[XmlAttribute]
	public ulong GrantedMemory
	{
		get
		{
			return grantedMemoryField;
		}
		set
		{
			grantedMemoryField = value;
		}
	}

	[XmlAttribute]
	public ulong MaxUsedMemory
	{
		get
		{
			return maxUsedMemoryField;
		}
		set
		{
			maxUsedMemoryField = value;
		}
	}
}
