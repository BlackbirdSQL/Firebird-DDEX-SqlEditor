// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CursorPlanTypeOperation
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;
using BlackbirdSql.Common.Controls.Graphing.Gram;

namespace BlackbirdSql.Common.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
public class CursorPlanTypeOperation
{
	private ParameterSensitivePredicateType[] dispatcherField;

	private QueryPlanType queryPlanField;

	private FunctionType[] uDFField;

	private EnCursorPlanTypeOperationOperationType operationTypeField;

	[XmlArrayItem("ParameterSensitivePredicate", IsNullable = false)]
	public ParameterSensitivePredicateType[] Dispatcher
	{
		get
		{
			return dispatcherField;
		}
		set
		{
			dispatcherField = value;
		}
	}

	public QueryPlanType QueryPlan
	{
		get
		{
			return queryPlanField;
		}
		set
		{
			queryPlanField = value;
		}
	}

	[XmlElement("UDF")]
	public FunctionType[] UDF
	{
		get
		{
			return uDFField;
		}
		set
		{
			uDFField = value;
		}
	}

	[XmlAttribute]
	public EnCursorPlanTypeOperationOperationType OperationType
	{
		get
		{
			return operationTypeField;
		}
		set
		{
			operationTypeField = value;
		}
	}
}
