// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SimpleUpdateType
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
[XmlType(Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class SimpleUpdateType : RowsetType
{
	private object itemField;

	private ScalarExpressionType setPredicateField;

	private bool dMLRequestSortField;

	private bool dMLRequestSortFieldSpecified;

	[XmlElement("SeekPredicate", typeof(SeekPredicateType))]
	[XmlElement("SeekPredicateNew", typeof(SeekPredicateNewType))]
	public object Item
	{
		get
		{
			return itemField;
		}
		set
		{
			itemField = value;
		}
	}

	public ScalarExpressionType SetPredicate
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
	public bool DMLRequestSort
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
	public bool DMLRequestSortSpecified
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
