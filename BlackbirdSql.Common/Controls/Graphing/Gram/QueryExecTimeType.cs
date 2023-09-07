// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.QueryExecTimeType
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
public class QueryExecTimeType
{
	private ulong cpuTimeField;

	private ulong elapsedTimeField;

	private ulong udfCpuTimeField;

	private bool udfCpuTimeFieldSpecified;

	private ulong udfElapsedTimeField;

	private bool udfElapsedTimeFieldSpecified;

	[XmlAttribute]
	public ulong CpuTime
	{
		get
		{
			return cpuTimeField;
		}
		set
		{
			cpuTimeField = value;
		}
	}

	[XmlAttribute]
	public ulong ElapsedTime
	{
		get
		{
			return elapsedTimeField;
		}
		set
		{
			elapsedTimeField = value;
		}
	}

	[XmlAttribute]
	public ulong UdfCpuTime
	{
		get
		{
			return udfCpuTimeField;
		}
		set
		{
			udfCpuTimeField = value;
		}
	}

	[XmlIgnore]
	public bool UdfCpuTimeSpecified
	{
		get
		{
			return udfCpuTimeFieldSpecified;
		}
		set
		{
			udfCpuTimeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong UdfElapsedTime
	{
		get
		{
			return udfElapsedTimeField;
		}
		set
		{
			udfElapsedTimeField = value;
		}
	}

	[XmlIgnore]
	public bool UdfElapsedTimeSpecified
	{
		get
		{
			return udfElapsedTimeFieldSpecified;
		}
		set
		{
			udfElapsedTimeFieldSpecified = value;
		}
	}
}
