// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FunctionTypeParser
using System.ComponentModel;


namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal sealed class FunctionTypeParser : AbstractXmlPlanParser
{
	private static FunctionTypeParser functionTypeParser;

	public static FunctionTypeParser Instance
	{
		get
		{
			functionTypeParser ??= new FunctionTypeParser();
			return functionTypeParser;
		}
	}

	public override Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		Node node = AbstractXmlPlanParser.NewNode(context);
		bool flag = false;
		if (parentItem != null)
		{
			PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(parentItem)["StoredProc"];
			if (propertyDescriptor != null && propertyDescriptor.GetValue(parentItem) == item)
			{
				flag = true;
			}
		}
		node.Operation = (flag ? OperationTable.GetStoredProc() : OperationTable.GetUdf());
		return node;
	}

	protected override Operation GetNodeOperation(Node node)
	{
		return null;
	}

	protected override double GetNodeSubtreeCost(Node node)
	{
		return 0.0;
	}

	private FunctionTypeParser()
	{
	}
}
