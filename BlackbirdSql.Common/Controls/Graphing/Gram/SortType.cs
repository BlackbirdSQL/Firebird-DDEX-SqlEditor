// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SortType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;


namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[XmlInclude(typeof(TopSortType))]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class SortType : RelOpBaseType
{
	private OrderByTypeOrderByColumn[] orderByField;

	private SingleColumnReferenceType partitionIdField;

	private RelOpType relOpField;

	private bool distinctField;

	[XmlArrayItem("OrderByColumn", IsNullable = false)]
	public OrderByTypeOrderByColumn[] OrderBy
	{
		get
		{
			return orderByField;
		}
		set
		{
			orderByField = value;
		}
	}

	public SingleColumnReferenceType PartitionId
	{
		get
		{
			return partitionIdField;
		}
		set
		{
			partitionIdField = value;
		}
	}

	public RelOpType RelOp
	{
		get
		{
			return relOpField;
		}
		set
		{
			relOpField = value;
		}
	}

	[XmlAttribute]
	public bool Distinct
	{
		get
		{
			return distinctField;
		}
		set
		{
			distinctField = value;
		}
	}
}
