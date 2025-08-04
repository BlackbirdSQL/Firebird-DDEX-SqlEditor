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
internal class ConvertType
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

	internal ScalarExpressionType Style
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

	[XmlAttribute]
	internal string DataType
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
	internal int Length
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
	internal bool LengthSpecified
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
	internal int Precision
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
	internal bool PrecisionSpecified
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
	internal int Scale
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
	internal bool ScaleSpecified
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
	internal int Style1
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
	internal bool Implicit
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
