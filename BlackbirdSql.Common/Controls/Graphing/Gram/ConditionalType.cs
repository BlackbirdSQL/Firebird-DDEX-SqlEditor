// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ConditionalType
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
public class ConditionalType
{
	private ScalarExpressionType conditionField;

	private ScalarExpressionType thenField;

	private ScalarExpressionType elseField;

	public ScalarExpressionType Condition
	{
		get
		{
			return conditionField;
		}
		set
		{
			conditionField = value;
		}
	}

	public ScalarExpressionType Then
	{
		get
		{
			return thenField;
		}
		set
		{
			thenField = value;
		}
	}

	public ScalarExpressionType Else
	{
		get
		{
			return elseField;
		}
		set
		{
			elseField = value;
		}
	}
}
