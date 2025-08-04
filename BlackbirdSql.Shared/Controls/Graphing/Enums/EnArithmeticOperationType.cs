// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.ArithmeticOperationType

using System;
using System.CodeDom.Compiler;
using System.Xml.Serialization;



namespace BlackbirdSql.Shared.Controls.Graphing.Enums;

[Serializable]
[GeneratedCode("xsd", "4.8.3928.0")]
[XmlType(Namespace = LibraryData.C_ShowPlanNamespace)]


internal enum EnArithmeticOperationType
{
	ADD,
	BIT_ADD,
	BIT_AND,
	BIT_COMBINE,
	BIT_NOT,
	BIT_OR,
	BIT_XOR,
	DIV,
	HASH,
	MINUS,
	MOD,
	MULT,
	SUB
}
