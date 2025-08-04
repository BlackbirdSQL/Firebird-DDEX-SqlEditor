// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SpillToTempDbType
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
internal class SpillToTempDbType
{
	private ulong spillLevelField;

	private bool spillLevelFieldSpecified;

	private ulong spilledThreadCountField;

	private bool spilledThreadCountFieldSpecified;

	[XmlAttribute]
	internal ulong SpillLevel
	{
		get
		{
			return spillLevelField;
		}
		set
		{
			spillLevelField = value;
		}
	}

	[XmlIgnore]
	internal bool SpillLevelSpecified
	{
		get
		{
			return spillLevelFieldSpecified;
		}
		set
		{
			spillLevelFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal ulong SpilledThreadCount
	{
		get
		{
			return spilledThreadCountField;
		}
		set
		{
			spilledThreadCountField = value;
		}
	}

	[XmlIgnore]
	internal bool SpilledThreadCountSpecified
	{
		get
		{
			return spilledThreadCountFieldSpecified;
		}
		set
		{
			spilledThreadCountFieldSpecified = value;
		}
	}
}
