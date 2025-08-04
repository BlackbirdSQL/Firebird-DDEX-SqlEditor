// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ForeignKeyReferencesCheckType
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
internal class ForeignKeyReferencesCheckType : RelOpBaseType
{
	private RelOpType relOpField;

	private ForeignKeyReferenceCheckType[] foreignKeyReferenceCheckField;

	private int foreignKeyReferencesCountField;

	private bool foreignKeyReferencesCountFieldSpecified;

	private int noMatchingIndexCountField;

	private bool noMatchingIndexCountFieldSpecified;

	private int partialMatchingIndexCountField;

	private bool partialMatchingIndexCountFieldSpecified;

	internal RelOpType RelOp
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
	internal ForeignKeyReferenceCheckType[] ForeignKeyReferenceCheck
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
	internal int ForeignKeyReferencesCount
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
	internal bool ForeignKeyReferencesCountSpecified
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
	internal int NoMatchingIndexCount
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
	internal bool NoMatchingIndexCountSpecified
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
	internal int PartialMatchingIndexCount
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
	internal bool PartialMatchingIndexCountSpecified
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
