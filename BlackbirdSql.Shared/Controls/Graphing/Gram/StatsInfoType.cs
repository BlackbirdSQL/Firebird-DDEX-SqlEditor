// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StatsInfoType
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
public class StatsInfoType
{
	private string databaseField;

	private string schemaField;

	private string tableField;

	private string _StatisticsField;

	private ulong modificationCountField;

	private double samplingPercentField;

	private DateTime lastUpdateField;

	private bool lastUpdateFieldSpecified;

	[XmlAttribute]
	public string Database
	{
		get
		{
			return databaseField;
		}
		set
		{
			databaseField = value;
		}
	}

	[XmlAttribute]
	public string Schema
	{
		get
		{
			return schemaField;
		}
		set
		{
			schemaField = value;
		}
	}

	[XmlAttribute]
	public string Table
	{
		get
		{
			return tableField;
		}
		set
		{
			tableField = value;
		}
	}

	[XmlAttribute]
	public string StatisticsField
	{
		get
		{
			return _StatisticsField;
		}
		set
		{
			_StatisticsField = value;
		}
	}

	[XmlAttribute]
	public ulong ModificationCount
	{
		get
		{
			return modificationCountField;
		}
		set
		{
			modificationCountField = value;
		}
	}

	[XmlAttribute]
	public double SamplingPercent
	{
		get
		{
			return samplingPercentField;
		}
		set
		{
			samplingPercentField = value;
		}
	}

	[XmlAttribute]
	public DateTime LastUpdate
	{
		get
		{
			return lastUpdateField;
		}
		set
		{
			lastUpdateField = value;
		}
	}

	[XmlIgnore]
	public bool LastUpdateSpecified
	{
		get
		{
			return lastUpdateFieldSpecified;
		}
		set
		{
			lastUpdateFieldSpecified = value;
		}
	}
}
