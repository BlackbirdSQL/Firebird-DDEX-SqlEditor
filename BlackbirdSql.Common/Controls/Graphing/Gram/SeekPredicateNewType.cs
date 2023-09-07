// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.SeekPredicateNewType
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
public class SeekPredicateNewType
{
	private SeekPredicateType[] seekKeysField;

	[XmlElement("SeekKeys")]
	public SeekPredicateType[] SeekKeys
	{
		get
		{
			return seekKeysField;
		}
		set
		{
			seekKeysField = value;
		}
	}
}
