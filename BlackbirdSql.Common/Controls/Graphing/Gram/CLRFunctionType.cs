// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CLRFunctionType
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
public class CLRFunctionType
{
	private string assemblyField;

	private string classField;

	private string methodField;

	[XmlAttribute]
	public string Assembly
	{
		get
		{
			return assemblyField;
		}
		set
		{
			assemblyField = value;
		}
	}

	[XmlAttribute]
	public string Class
	{
		get
		{
			return classField;
		}
		set
		{
			classField = value;
		}
	}

	[XmlAttribute]
	public string Method
	{
		get
		{
			return methodField;
		}
		set
		{
			methodField = value;
		}
	}
}
