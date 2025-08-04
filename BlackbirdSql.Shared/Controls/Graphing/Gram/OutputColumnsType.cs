// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.OutputColumnsType
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
internal class OutputColumnsType
{
	private DefinedValuesListTypeDefinedValue[] definedValuesField;

	private ObjectType[] objectField;

	[XmlArrayItem("DefinedValue", IsNullable = false)]
	internal DefinedValuesListTypeDefinedValue[] DefinedValues
	{
		get
		{
			return definedValuesField;
		}
		set
		{
			definedValuesField = value;
		}
	}

	[XmlElement("Object")]
	internal ObjectType[] Object
	{
		get
		{
			return objectField;
		}
		set
		{
			objectField = value;
		}
	}
}
