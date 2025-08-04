// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ColumnReferenceType
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
internal class ColumnReferenceType
{
	private ScalarType scalarOperatorField;

	private InternalInfoType internalInfoField;

	private string serverField;

	private string databaseField;

	private string schemaField;

	private string tableField;

	private string aliasField;

	private string columnField;

	private bool computedColumnField;

	private bool computedColumnFieldSpecified;

	private string parameterDataTypeField;

	private string parameterCompiledValueField;

	private string parameterRuntimeValueField;

	internal ScalarType ScalarOperator
	{
		get
		{
			return scalarOperatorField;
		}
		set
		{
			scalarOperatorField = value;
		}
	}

	internal InternalInfoType InternalInfo
	{
		get
		{
			return internalInfoField;
		}
		set
		{
			internalInfoField = value;
		}
	}

	[XmlAttribute]
	internal string Server
	{
		get
		{
			return serverField;
		}
		set
		{
			serverField = value;
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

	[XmlAttribute]
	internal string Alias
	{
		get
		{
			return aliasField;
		}
		set
		{
			aliasField = value;
		}
	}

	[XmlAttribute]
	internal string Column
	{
		get
		{
			return columnField;
		}
		set
		{
			columnField = value;
		}
	}

	[XmlAttribute]
	internal bool ComputedColumn
	{
		get
		{
			return computedColumnField;
		}
		set
		{
			computedColumnField = value;
		}
	}

	[XmlIgnore]
	internal bool ComputedColumnSpecified
	{
		get
		{
			return computedColumnFieldSpecified;
		}
		set
		{
			computedColumnFieldSpecified = value;
		}
	}

	[XmlAttribute]
	internal string ParameterDataType
	{
		get
		{
			return parameterDataTypeField;
		}
		set
		{
			parameterDataTypeField = value;
		}
	}

	[XmlAttribute]
	internal string ParameterCompiledValue
	{
		get
		{
			return parameterCompiledValueField;
		}
		set
		{
			parameterCompiledValueField = value;
		}
	}

	[XmlAttribute]
	internal string ParameterRuntimeValue
	{
		get
		{
			return parameterRuntimeValueField;
		}
		set
		{
			parameterRuntimeValueField = value;
		}
	}
}
