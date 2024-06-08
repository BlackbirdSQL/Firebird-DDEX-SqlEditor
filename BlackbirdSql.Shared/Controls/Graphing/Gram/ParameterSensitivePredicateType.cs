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
public class ParameterSensitivePredicateType
{
	private StatsInfoType[] statisticsInfoField;

	private ScalarExpressionType predicateField;

	private double lowBoundaryField;

	private double highBoundaryField;

	[XmlElement("StatisticsInfo")]
	public StatsInfoType[] StatisticsInfo
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

	public ScalarExpressionType Predicate
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
	public double LowBoundary
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
	public double HighBoundary
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
