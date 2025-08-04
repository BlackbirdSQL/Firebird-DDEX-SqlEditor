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
internal class StatsInfoType
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
	internal string Database
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
	internal string Schema
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
	internal string Table
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
	internal string StatisticsField
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
	internal ulong ModificationCount
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
	internal double SamplingPercent
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
	internal DateTime LastUpdate
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
	internal bool LastUpdateSpecified
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
