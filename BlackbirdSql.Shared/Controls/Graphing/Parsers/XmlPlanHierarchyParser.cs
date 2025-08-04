// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.XmlPlanHierarchyParser
using System.Collections.Generic;


namespace BlackbirdSql.Shared.Controls.Graphing.Parsers;

internal class XmlPlanHierarchyParser : AbstractXmlPlanParser
{
	private static XmlPlanHierarchyParser xmlPlanHierarchyParser;

	internal static XmlPlanHierarchyParser Instance
	{
		get
		{
			xmlPlanHierarchyParser ??= new XmlPlanHierarchyParser();
			return xmlPlanHierarchyParser;
		}
	}

	internal override Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		return parentNode;
	}

	internal override IEnumerable<FunctionTypeItem> ExtractFunctions(object parsedItem)
	{
		foreach (object child in GetChildren(parsedItem))
		{
			AbstractXmlPlanParser parser = XmlPlanParserFactory.GetParser(child.GetType());
			foreach (FunctionTypeItem item in parser.ExtractFunctions(child))
			{
				yield return item;
			}
		}
	}

	protected XmlPlanHierarchyParser()
	{
	}
}
