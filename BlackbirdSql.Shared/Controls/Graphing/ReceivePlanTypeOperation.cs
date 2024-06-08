// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ReceivePlanTypeOperation
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
public class ReceivePlanTypeOperation
{
	private QueryPlanType queryPlanField;

	private EnReceivePlanTypeOperationOperationType operationTypeField;

	private bool operationTypeFieldSpecified;

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

	[XmlAttribute]
	public EnReceivePlanTypeOperationOperationType OperationType
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

	[XmlIgnore]
	public bool OperationTypeSpecified
	{
		get
		{
			return operationTypeFieldSpecified;
		}
		set
		{
			operationTypeFieldSpecified = value;
		}
	}
}
