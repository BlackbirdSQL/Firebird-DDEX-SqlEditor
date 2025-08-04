// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StmtSimpleType
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
internal class StmtSimpleType : AbstractStmtInfoType
{
	private ParameterSensitivePredicateType[] dispatcherField;

	private QueryPlanType queryPlanField;

	private FunctionType[] uDFField;

	private FunctionType storedProcField;

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

	internal FunctionType StoredProc
	{
		get
		{
			return storedProcField;
		}
		set
		{
			storedProcField = value;
		}
	}
}
