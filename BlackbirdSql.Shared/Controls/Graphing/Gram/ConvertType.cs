// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ConvertType
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
public class ConvertType
{
	private ScalarExpressionType styleField;

	private ScalarType scalarOperatorField;

	private string dataTypeField;

	private int lengthField;

	private bool lengthFieldSpecified;

	private int precisionField;

	private bool precisionFieldSpecified;

	private int scaleField;

	private bool scaleFieldSpecified;

	private int style1Field;

	private bool implicitField;

	public ScalarExpressionType Style
	{
		get
		{
			return styleField;
		}
		set
		{
			styleField = value;
		}
	}

	public ScalarType ScalarOperator
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

	[XmlAttribute]
	public string DataType
	{
		get
		{
			return dataTypeField;
		}
		set
		{
			dataTypeField = value;
		}
	}

	[XmlAttribute]
	public int Length
	{
		get
		{
			return lengthField;
		}
		set
		{
			lengthField = value;
		}
	}

	[XmlIgnore]
	public bool LengthSpecified
	{
		get
		{
			return lengthFieldSpecified;
		}
		set
		{
			lengthFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int Precision
	{
		get
		{
			return precisionField;
		}
		set
		{
			precisionField = value;
		}
	}

	[XmlIgnore]
	public bool PrecisionSpecified
	{
		get
		{
			return precisionFieldSpecified;
		}
		set
		{
			precisionFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int Scale
	{
		get
		{
			return scaleField;
		}
		set
		{
			scaleField = value;
		}
	}

	[XmlIgnore]
	public bool ScaleSpecified
	{
		get
		{
			return scaleFieldSpecified;
		}
		set
		{
			scaleFieldSpecified = value;
		}
	}

	[XmlAttribute("Style")]
	public int Style1
	{
		get
		{
			return style1Field;
		}
		set
		{
			style1Field = value;
		}
	}

	[XmlAttribute]
	public bool Implicit
	{
		get
		{
			return implicitField;
		}
		set
		{
			implicitField = value;
		}
	}
}
