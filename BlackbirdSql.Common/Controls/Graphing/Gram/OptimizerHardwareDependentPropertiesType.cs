// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.OptimizerHardwareDependentPropertiesType
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
public class OptimizerHardwareDependentPropertiesType
{
	private ulong estimatedAvailableMemoryGrantField;

	private ulong estimatedPagesCachedField;

	private ulong estimatedAvailableDegreeOfParallelismField;

	private bool estimatedAvailableDegreeOfParallelismFieldSpecified;

	private ulong maxCompileMemoryField;

	private bool maxCompileMemoryFieldSpecified;

	[XmlAttribute]
	public ulong EstimatedAvailableMemoryGrant
	{
		get
		{
			return estimatedAvailableMemoryGrantField;
		}
		set
		{
			estimatedAvailableMemoryGrantField = value;
		}
	}

	[XmlAttribute]
	public ulong EstimatedPagesCached
	{
		get
		{
			return estimatedPagesCachedField;
		}
		set
		{
			estimatedPagesCachedField = value;
		}
	}

	[XmlAttribute]
	public ulong EstimatedAvailableDegreeOfParallelism
	{
		get
		{
			return estimatedAvailableDegreeOfParallelismField;
		}
		set
		{
			estimatedAvailableDegreeOfParallelismField = value;
		}
	}

	[XmlIgnore]
	public bool EstimatedAvailableDegreeOfParallelismSpecified
	{
		get
		{
			return estimatedAvailableDegreeOfParallelismFieldSpecified;
		}
		set
		{
			estimatedAvailableDegreeOfParallelismFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public ulong MaxCompileMemory
	{
		get
		{
			return maxCompileMemoryField;
		}
		set
		{
			maxCompileMemoryField = value;
		}
	}

	[XmlIgnore]
	public bool MaxCompileMemorySpecified
	{
		get
		{
			return maxCompileMemoryFieldSpecified;
		}
		set
		{
			maxCompileMemoryFieldSpecified = value;
		}
	}
}
