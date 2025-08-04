// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CursorPlanTypeOperation
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Shared.Controls.Graphing.Enums;
using BlackbirdSql.Shared.Controls.Graphing.Gram;

namespace BlackbirdSql.Shared.Controls.Graphing;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(AnonymousType = true, Namespace = LibraryData.C_ShowPlanNamespace)]
internal class CursorPlanTypeOperation
{
	private ParameterSensitivePredicateType[] dispatcherField;

	private QueryPlanType queryPlanField;

	private FunctionType[] uDFField;

	private EnCursorPlanTypeOperationOperationType operationTypeField;

	[XmlArrayItem("ParameterSensitivePredicate", IsNullable = false)]
	internal ParameterSensitivePredicateType[] Dispatcher
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

	internal QueryPlanType QueryPlan
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
	internal FunctionType[] UDF
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
	internal EnCursorPlanTypeOperationOperationType OperationType
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
