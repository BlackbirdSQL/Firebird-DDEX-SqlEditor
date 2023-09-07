// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SetOptionsType
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
public class SetOptionsType
{
	private bool aNSI_NULLSField;

	private bool aNSI_NULLSFieldSpecified;

	private bool aNSI_PADDINGField;

	private bool aNSI_PADDINGFieldSpecified;

	private bool aNSI_WARNINGSField;

	private bool aNSI_WARNINGSFieldSpecified;

	private bool aRITHABORTField;

	private bool aRITHABORTFieldSpecified;

	private bool cONCAT_NULL_YIELDS_NULLField;

	private bool cONCAT_NULL_YIELDS_NULLFieldSpecified;

	private bool nUMERIC_ROUNDABORTField;

	private bool nUMERIC_ROUNDABORTFieldSpecified;

	private bool qUOTED_IDENTIFIERField;

	private bool qUOTED_IDENTIFIERFieldSpecified;

	[XmlAttribute]
	public bool ANSI_NULLS
	{
		get
		{
			return aNSI_NULLSField;
		}
		set
		{
			aNSI_NULLSField = value;
		}
	}

	[XmlIgnore]
	public bool ANSI_NULLSSpecified
	{
		get
		{
			return aNSI_NULLSFieldSpecified;
		}
		set
		{
			aNSI_NULLSFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool ANSI_PADDING
	{
		get
		{
			return aNSI_PADDINGField;
		}
		set
		{
			aNSI_PADDINGField = value;
		}
	}

	[XmlIgnore]
	public bool ANSI_PADDINGSpecified
	{
		get
		{
			return aNSI_PADDINGFieldSpecified;
		}
		set
		{
			aNSI_PADDINGFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool ANSI_WARNINGS
	{
		get
		{
			return aNSI_WARNINGSField;
		}
		set
		{
			aNSI_WARNINGSField = value;
		}
	}

	[XmlIgnore]
	public bool ANSI_WARNINGSSpecified
	{
		get
		{
			return aNSI_WARNINGSFieldSpecified;
		}
		set
		{
			aNSI_WARNINGSFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool ARITHABORT
	{
		get
		{
			return aRITHABORTField;
		}
		set
		{
			aRITHABORTField = value;
		}
	}

	[XmlIgnore]
	public bool ARITHABORTSpecified
	{
		get
		{
			return aRITHABORTFieldSpecified;
		}
		set
		{
			aRITHABORTFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool CONCAT_NULL_YIELDS_NULL
	{
		get
		{
			return cONCAT_NULL_YIELDS_NULLField;
		}
		set
		{
			cONCAT_NULL_YIELDS_NULLField = value;
		}
	}

	[XmlIgnore]
	public bool CONCAT_NULL_YIELDS_NULLSpecified
	{
		get
		{
			return cONCAT_NULL_YIELDS_NULLFieldSpecified;
		}
		set
		{
			cONCAT_NULL_YIELDS_NULLFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool NUMERIC_ROUNDABORT
	{
		get
		{
			return nUMERIC_ROUNDABORTField;
		}
		set
		{
			nUMERIC_ROUNDABORTField = value;
		}
	}

	[XmlIgnore]
	public bool NUMERIC_ROUNDABORTSpecified
	{
		get
		{
			return nUMERIC_ROUNDABORTFieldSpecified;
		}
		set
		{
			nUMERIC_ROUNDABORTFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool QUOTED_IDENTIFIER
	{
		get
		{
			return qUOTED_IDENTIFIERField;
		}
		set
		{
			qUOTED_IDENTIFIERField = value;
		}
	}

	[XmlIgnore]
	public bool QUOTED_IDENTIFIERSpecified
	{
		get
		{
			return qUOTED_IDENTIFIERFieldSpecified;
		}
		set
		{
			qUOTED_IDENTIFIERFieldSpecified = value;
		}
	}
}
