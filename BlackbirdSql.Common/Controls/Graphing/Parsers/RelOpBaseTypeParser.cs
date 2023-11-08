// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.RelOpBaseTypeParser
using System.Collections;
using System.ComponentModel;


namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal class RelOpBaseTypeParser : AbstractXmlPlanParser
{
	private static RelOpBaseTypeParser relOpBaseTypeParser;

	public static RelOpBaseTypeParser Instance
	{
		get
		{
			relOpBaseTypeParser ??= new RelOpBaseTypeParser();
			return relOpBaseTypeParser;
		}
	}

	public override Node GetCurrentNode(object item, object parentItem, Node parentNode, NodeBuilderContext context)
	{
		return parentNode;
	}

	public override IEnumerable GetChildren(object parsedItem)
	{
		PropertyDescriptor propertyDescriptor = TypeDescriptor.GetProperties(parsedItem)["RelOp"];
		if (propertyDescriptor == null)
		{
			yield break;
		}
		object value = propertyDescriptor.GetValue(parsedItem);
		if (value == null)
		{
			yield break;
		}
		if (value is IEnumerable numerator)
		{
			foreach (object item in numerator)
			{
				yield return item;
			}
		}
		else
		{
			yield return value;
		}
	}

	protected RelOpBaseTypeParser()
	{
	}
}
