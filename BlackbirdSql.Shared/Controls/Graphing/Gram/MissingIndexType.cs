// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.MissingIndexType
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
internal class MissingIndexType
{
	private ColumnGroupType[] columnGroupField;

	private string databaseField;

	private string schemaField;

	private string tableField;

	[XmlElement("ColumnGroup")]
	internal ColumnGroupType[] ColumnGroup
	{
		get
		{
			return columnGroupField;
		}
		set
		{
			columnGroupField = value;
		}
	}

	[XmlAttribute]
	internal string Database
	{
		get
		{
			return databaseField;
		}
		set
		{
			databaseField = value;
		}
	}

	[XmlAttribute]
	internal string Schema
	{
		get
		{
			return schemaField;
		}
		set
		{
			schemaField = value;
		}
	}

	[XmlAttribute]
	internal string Table
	{
		get
		{
			return tableField;
		}
		set
		{
			tableField = value;
		}
	}
}
