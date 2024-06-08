// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.XmlPlanParserFactory
using System;


namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal static class XmlPlanParserFactory
{
	public static AbstractXmlPlanParser GetParser(Type type)
	{
		while (true)
		{
			switch (type.Name)
			{
				case "RelOpType":
					return RelOpTypeParser.Instance;
				case "BaseStmtInfoType":
					return StatementParser.Instance;
				case "RelOpBaseType":
					return RelOpBaseTypeParser.Instance;
				case "FilterType":
					return FilterTypeParser.Instance;
				case "MergeType":
					return MergeTypeParser.Instance;
				case "StmtCursorType":
					return CursorStatementParser.Instance;
				case "CursorPlanTypeOperation":
					return CursorOperationParser.Instance;
				case "StmtBlockType":
				case "QueryPlanType":
				case "CursorPlanType":
				case "ReceivePlanTypeOperation":
				case "StmtCondTypeThen":
				case "StmtCondTypeElse":
					return XmlPlanHierarchyParser.Instance;
				case "StmtCondTypeCondition":
					return ConditionParser.Instance;
				case "FunctionType":
					return FunctionTypeParser.Instance;
				case "IndexScanType":
				case "CreateIndexType":
					return IndexOpTypeParser.Instance;
				case "Object":
					return null;
			}
			type = type.BaseType;
		}
	}
}
