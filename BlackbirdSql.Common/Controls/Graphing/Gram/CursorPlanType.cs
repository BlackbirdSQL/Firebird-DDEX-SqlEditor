// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CursorPlanType
using System;
using System.CodeDom.Compiler;
using System.ComponentModel;
using System.Diagnostics;
using System.Xml.Serialization;
using BlackbirdSql.Common.Controls.Graphing.Enums;

namespace BlackbirdSql.Common.Controls.Graphing.Gram;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[DebuggerStepThrough]
[DesignerCategory("code")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]
public class CursorPlanType
{
	private CursorPlanTypeOperation[] operationField;

	private string cursorNameField;

	private EnCursorType cursorActualTypeField;

	private bool cursorActualTypeFieldSpecified;

	private EnCursorType cursorRequestedTypeField;

	private bool cursorRequestedTypeFieldSpecified;

	private EnCursorPlanTypeCursorConcurrency cursorConcurrencyField;

	private bool cursorConcurrencyFieldSpecified;

	private bool forwardOnlyField;

	private bool forwardOnlyFieldSpecified;

	[XmlElement("Operation")]
	public CursorPlanTypeOperation[] Operation
	{
		get
		{
			return operationField;
		}
		set
		{
			operationField = value;
		}
	}

	[XmlAttribute]
	public string CursorName
	{
		get
		{
			return cursorNameField;
		}
		set
		{
			cursorNameField = value;
		}
	}

	[XmlAttribute]
	public EnCursorType CursorActualType
	{
		get
		{
			return cursorActualTypeField;
		}
		set
		{
			cursorActualTypeField = value;
		}
	}

	[XmlIgnore]
	public bool CursorActualTypeSpecified
	{
		get
		{
			return cursorActualTypeFieldSpecified;
		}
		set
		{
			cursorActualTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public EnCursorType CursorRequestedType
	{
		get
		{
			return cursorRequestedTypeField;
		}
		set
		{
			cursorRequestedTypeField = value;
		}
	}

	[XmlIgnore]
	public bool CursorRequestedTypeSpecified
	{
		get
		{
			return cursorRequestedTypeFieldSpecified;
		}
		set
		{
			cursorRequestedTypeFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public EnCursorPlanTypeCursorConcurrency CursorConcurrency
	{
		get
		{
			return cursorConcurrencyField;
		}
		set
		{
			cursorConcurrencyField = value;
		}
	}

	[XmlIgnore]
	public bool CursorConcurrencySpecified
	{
		get
		{
			return cursorConcurrencyFieldSpecified;
		}
		set
		{
			cursorConcurrencyFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public bool ForwardOnly
	{
		get
		{
			return forwardOnlyField;
		}
		set
		{
			forwardOnlyField = value;
		}
	}

	[XmlIgnore]
	public bool ForwardOnlySpecified
	{
		get
		{
			return forwardOnlyFieldSpecified;
		}
		set
		{
			forwardOnlyFieldSpecified = value;
		}
	}
}
