// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StmtSimpleType
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
public class StmtSimpleType : AbstractStmtInfoType
{
	private ParameterSensitivePredicateType[] dispatcherField;

	private QueryPlanType queryPlanField;

	private FunctionType[] uDFField;

	private FunctionType storedProcField;

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

	public FunctionType StoredProc
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
