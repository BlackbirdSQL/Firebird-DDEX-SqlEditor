// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ScalarInsertType
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
internal class ScalarInsertType : RowsetType
{
	private ScalarExpressionType setPredicateField;

	private bool dMLRequestSortField;

	private bool dMLRequestSortFieldSpecified;

	internal ScalarExpressionType SetPredicate
	{
		get
		{
			return setPredicateField;
		}
		set
		{
			setPredicateField = value;
		}
	}

	[XmlAttribute]
	internal bool DMLRequestSort
	{
		get
		{
			return dMLRequestSortField;
		}
		set
		{
			dMLRequestSortField = value;
		}
	}

	[XmlIgnore]
	internal bool DMLRequestSortSpecified
	{
		get
		{
			return dMLRequestSortFieldSpecified;
		}
		set
		{
			dMLRequestSortFieldSpecified = value;
		}
	}
}
