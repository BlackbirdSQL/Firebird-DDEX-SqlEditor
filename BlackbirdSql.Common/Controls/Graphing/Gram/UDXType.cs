// Warning: Some assembly references could not be resolved automatically. This might lead to incorrect decompilation of some parts,
// for ex. property getter/setter access. To get optimal decompilation results, please manually add the missing references to the list of loaded assemblies.
// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.UDXType
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
public class UDXType : RelOpBaseType
{
	private ColumnReferenceType[] usedUDXColumnsField;

	private RelOpType relOpField;

	private string uDXNameField;

	[XmlArrayItem("ColumnReference", IsNullable = false)]
	public ColumnReferenceType[] UsedUDXColumns
	{
		get
		{
			return usedUDXColumnsField;
		}
		set
		{
			usedUDXColumnsField = value;
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
	public string UDXName
	{
		get
		{
			return uDXNameField;
		}
		set
		{
			uDXNameField = value;
		}
	}
}
