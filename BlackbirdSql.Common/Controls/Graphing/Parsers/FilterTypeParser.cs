// sqlmgmt, Version=16.200.0.0, Culture=neutral, PublicKeyToken=89845dcd8080cc91
// Microsoft.SqlServer.Management.SqlMgmt.ShowPlan.FilterTypeParser
using System.ComponentModel;
using BlackbirdSql.Common.Controls.Graphing.ComponentModel;
using BlackbirdSql.Common.Controls.Graphing.Gram;
using BlackbirdSql.Common.Properties;

namespace BlackbirdSql.Common.Controls.Graphing.Parsers;

internal sealed class FilterTypeParser : RelOpBaseTypeParser
{
	private static FilterTypeParser filterTypeParser;

	public new static FilterTypeParser Instance
	{
		get
		{
			filterTypeParser ??= new FilterTypeParser();
			return filterTypeParser;
		}
	}

	private FilterTypeParser()
	{
	}

	public override void ParseProperties(object parsedItem, PropertyDescriptorCollection targetPropertyBag, NodeBuilderContext context)
	{
		base.ParseProperties(parsedItem, targetPropertyBag, context);
		if ((parsedItem as FilterType).StartupExpression && targetPropertyBag["Predicate"] is PropertyValue propertyValue)
		{
			propertyValue.SetDisplayNameAndDescription(ControlsResources.StartupExpressionPredicate, null);
		}
	}
}
