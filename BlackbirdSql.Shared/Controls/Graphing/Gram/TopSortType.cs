// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.TopSortType
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
internal class TopSortType : SortType
{
	private int rowsField;

	private bool withTiesField;

	private bool withTiesFieldSpecified;

	[XmlAttribute]
	internal int Rows
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
	internal bool WithTies
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
	internal bool WithTiesSpecified
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
