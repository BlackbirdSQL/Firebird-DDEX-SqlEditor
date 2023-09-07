// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.UDFType
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
public class UDFType
{
	private ScalarType[] scalarOperatorField;

	private CLRFunctionType cLRFunctionField;

	private string functionNameField;

	private bool isClrFunctionField;

	private bool isClrFunctionFieldSpecified;

	[XmlElement("ScalarOperator")]
	public ScalarType[] ScalarOperator
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

	public CLRFunctionType CLRFunction
	{
		get
		{
			return cLRFunctionField;
		}
		set
		{
			cLRFunctionField = value;
		}
	}

	[XmlAttribute]
	public string FunctionName
	{
		get
		{
			return functionNameField;
		}
		set
		{
			functionNameField = value;
		}
	}

	[XmlAttribute]
	public bool IsClrFunction
	{
		get
		{
			return isClrFunctionField;
		}
		set
		{
			isClrFunctionField = value;
		}
	}

	[XmlIgnore]
	public bool IsClrFunctionSpecified
	{
		get
		{
			return isClrFunctionFieldSpecified;
		}
		set
		{
			isClrFunctionFieldSpecified = value;
		}
	}
}
