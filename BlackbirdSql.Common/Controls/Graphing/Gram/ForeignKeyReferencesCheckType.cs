// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ForeignKeyReferencesCheckType
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
[XmlType(Namespace = "http://schemas.microsoft.com/sqlserver/2004/07/showplan")]
public class ForeignKeyReferencesCheckType : RelOpBaseType
{
	private RelOpType relOpField;

	private ForeignKeyReferenceCheckType[] foreignKeyReferenceCheckField;

	private int foreignKeyReferencesCountField;

	private bool foreignKeyReferencesCountFieldSpecified;

	private int noMatchingIndexCountField;

	private bool noMatchingIndexCountFieldSpecified;

	private int partialMatchingIndexCountField;

	private bool partialMatchingIndexCountFieldSpecified;

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

	[XmlElement("ForeignKeyReferenceCheck")]
	public ForeignKeyReferenceCheckType[] ForeignKeyReferenceCheck
	{
		get
		{
			return foreignKeyReferenceCheckField;
		}
		set
		{
			foreignKeyReferenceCheckField = value;
		}
	}

	[XmlAttribute]
	public int ForeignKeyReferencesCount
	{
		get
		{
			return foreignKeyReferencesCountField;
		}
		set
		{
			foreignKeyReferencesCountField = value;
		}
	}

	[XmlIgnore]
	public bool ForeignKeyReferencesCountSpecified
	{
		get
		{
			return foreignKeyReferencesCountFieldSpecified;
		}
		set
		{
			foreignKeyReferencesCountFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int NoMatchingIndexCount
	{
		get
		{
			return noMatchingIndexCountField;
		}
		set
		{
			noMatchingIndexCountField = value;
		}
	}

	[XmlIgnore]
	public bool NoMatchingIndexCountSpecified
	{
		get
		{
			return noMatchingIndexCountFieldSpecified;
		}
		set
		{
			noMatchingIndexCountFieldSpecified = value;
		}
	}

	[XmlAttribute]
	public int PartialMatchingIndexCount
	{
		get
		{
			return partialMatchingIndexCountField;
		}
		set
		{
			partialMatchingIndexCountField = value;
		}
	}

	[XmlIgnore]
	public bool PartialMatchingIndexCountSpecified
	{
		get
		{
			return partialMatchingIndexCountFieldSpecified;
		}
		set
		{
			partialMatchingIndexCountFieldSpecified = value;
		}
	}
}
