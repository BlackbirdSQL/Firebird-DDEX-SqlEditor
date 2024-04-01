// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TopSortType
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
public class TopSortType : SortType
{
	private int rowsField;

	private bool withTiesField;

	private bool withTiesFieldSpecified;

	[XmlAttribute]
	public int Rows
	{
		get
		{
			return rowsField;
		}
		set
		{
			rowsField = value;
		}
	}

	[XmlAttribute]
	public bool WithTies
	{
		get
		{
			return withTiesField;
		}
		set
		{
			withTiesField = value;
		}
	}

	[XmlIgnore]
	public bool WithTiesSpecified
	{
		get
		{
			return withTiesFieldSpecified;
		}
		set
		{
			withTiesFieldSpecified = value;
		}
	}
}
