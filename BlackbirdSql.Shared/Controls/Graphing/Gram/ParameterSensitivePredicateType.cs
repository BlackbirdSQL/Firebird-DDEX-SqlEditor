// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ParameterSensitivePredicateType
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
internal class ParameterSensitivePredicateType
{
	private StatsInfoType[] statisticsInfoField;

	private ScalarExpressionType predicateField;

	private double lowBoundaryField;

	private double highBoundaryField;

	[XmlElement("StatisticsInfo")]
	internal StatsInfoType[] StatisticsInfo
	{
		get
		{
			return statisticsInfoField;
		}
		set
		{
			statisticsInfoField = value;
		}
	}

	internal ScalarExpressionType Predicate
	{
		get
		{
			return predicateField;
		}
		set
		{
			predicateField = value;
		}
	}

	[XmlAttribute]
	internal double LowBoundary
	{
		get
		{
			return lowBoundaryField;
		}
		set
		{
			lowBoundaryField = value;
		}
	}

	[XmlAttribute]
	internal double HighBoundary
	{
		get
		{
			return highBoundaryField;
		}
		set
		{
			highBoundaryField = value;
		}
	}
}
