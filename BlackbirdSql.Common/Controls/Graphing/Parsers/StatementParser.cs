// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.StatementParser
using System;
using System.Collections.Generic;
using System.Globalization;
using BlackbirdSql.Common.Controls.Graphing.Gram;

namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal class StatementParser : XmlPlanParser
{
	private static StatementParser statementParser;

	public static StatementParser Instance
	{
		get
		{
			statementParser ??= new StatementParser();
			return statementParser;
		}
	}

	public override Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		return XmlPlanParser.NewNode(context);
	}

	protected override Operation GetNodeOperation(Node node)
	{
		object obj = node["StatementType"];
		if (obj == null)
		{
			return Operation.Unknown;
		}
		return OperationTable.GetStatement(obj.ToString());
	}

	protected override double GetNodeSubtreeCost(Node node)
	{
		object obj = node["StatementSubTreeCost"];
		if (obj == null)
		{
			return 0.0;
		}
		return Convert.ToDouble(obj, CultureInfo.CurrentCulture);
	}

	protected override bool ShouldParseItem(object parsedItem)
	{
		if (parsedItem is StmtSimpleType stmtSimpleType && !stmtSimpleType.StatementIdSpecified)
		{
			return false;
		}
		return true;
	}

	public override IEnumerable<FunctionTypeItem> ExtractFunctions(object parsedItem)
	{
		if (parsedItem is StmtSimpleType statement)
		{
			if (statement.UDF != null)
			{
				FunctionType[] uDF = statement.UDF;
				foreach (FunctionType function in uDF)
				{
					yield return new FunctionTypeItem(function, FunctionTypeItem.ItemType.Udf);
				}
				statement.UDF = null;
			}
			if (statement.StoredProc != null)
			{
				yield return new FunctionTypeItem(statement.StoredProc, FunctionTypeItem.ItemType.StoredProcedure);
				statement.StoredProc = null;
			}
			yield break;
		}
		foreach (object child in GetChildren(parsedItem))
		{
			XmlPlanParser parser = XmlPlanParserFactory.GetParser(child.GetType());
			foreach (FunctionTypeItem item in parser.ExtractFunctions(child))
			{
				yield return item;
			}
		}
	}

	protected StatementParser()
	{
	}
}
