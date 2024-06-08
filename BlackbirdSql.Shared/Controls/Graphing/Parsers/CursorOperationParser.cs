// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.CursorOperationParser


namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal class CursorOperationParser : AbstractXmlPlanParser
{
	private static CursorOperationParser cursorOperationParser;

	public static CursorOperationParser Instance
	{
		get
		{
			cursorOperationParser ??= new CursorOperationParser();
			return cursorOperationParser;
		}
	}

	public override Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		return AbstractXmlPlanParser.NewNode(context);
	}

	protected override Operation GetNodeOperation(Node node)
	{
		object obj = node["OperationType"];
		if (obj == null)
		{
			return Operation.Unknown;
		}
		return OperationTable.GetPhysicalOperation(obj.ToString());
	}

	protected override double GetNodeSubtreeCost(Node node)
	{
		return 0.0;
	}

	private CursorOperationParser()
	{
	}
}
